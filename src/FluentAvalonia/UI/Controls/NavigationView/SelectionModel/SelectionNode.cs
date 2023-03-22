using Avalonia.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

internal enum SelectionState
{
    Selected,
    NotSelected,
    PartiallySelected
}

/// <summary>
/// Tracks nested selection.
/// </summary>
/// <remarks>
/// SelectionNode is the internal tree data structure that we keep track of for selection in 
/// a nested scenario. This would map to one ItemsSourceView/Collection. This node reacts to
/// collection changes and keeps the selected indices up to date. This can either be a leaf
/// node or a non leaf node.
/// </remarks>
internal class SelectionNode : IDisposable
{
    public SelectionNode(SelectionModel manager, SelectionNode parent)
    {
        _manager = manager;
        _parent = parent;
    }

    public object Source
    {
        get => _source;
        set
        {
            if (_source != value)
            {
                ClearSelection();
                UnhookCollectionChangedHandler();

                _source = value;

                // Setup ItemsSourceView
                var newDataSource = value as ItemsSourceView;
                if (value != null && newDataSource == null)
                {
                    newDataSource = ItemsSourceView.GetOrCreate(value as IEnumerable);
                }

                _dataSource = newDataSource;

                HookupCollectionChangedHandler();
                OnSelectionChanged();
            }
        }
    }

    public ItemsSourceView ItemsSourceView => _dataSource;

    public int DataCount => _dataSource != null ? _dataSource.Count : 0;

    public int ChildrenNodeCount => _childrenNodes.Count;

    public int RealizedChildrenNodeCount => _realizedChildrenNodeCount;

    public int AnchorIndex
    {
        get => _anchorIndex;
        set => _anchorIndex = value;
    }

    public IndexPath IndexPath
    {
        get
        {
            var path = new List<int>();
            var parent = _parent;
            var child = this;
            while (parent != null)
            {
                var childNodes = parent._childrenNodes;
                var index = childNodes.IndexOf(child);
                Debug.Assert(index >= 0);
                // we are walking up to the parent, so the path will be backwards
                path.Insert(0, index);
                child = parent;
                parent = parent._parent;
            }

            return new IndexPath(path);
        }
    }

    // For a genuine tree view, we dont know which node is leaf until we 
    // actually walk to it, so currently the tree builds up to the leaf. I don't 
    // create a bunch of leaf node instances - instead i use the same instance m_leafNode to avoid 
    // an explosion of node objects. However, I'm still creating the m_childrenNodes 
    // collection unfortunately.
    public SelectionNode GetAt(int index, bool realizeChild)
    {
        SelectionNode child = null;
        if (realizeChild)
        {
            if (_childrenNodes.Count == 0)
            {
                if (_dataSource != null)
                {
                    for (int i = 0; i < _dataSource.Count; i++)
                    {
                        _childrenNodes.Add(null);
                    }
                }
            }

            Debug.Assert(0 <= index && index <= _childrenNodes.Count);

            if (_childrenNodes[index] == null)
            {
                var childData = _dataSource.GetAt(index);
                if (childData != null)
                {
                    var childDataIndexPath = IndexPath.CloneWithChildIndex(index);
                    var resolvedChild = _manager.ResolvePath(childData, childDataIndexPath);
                    if (resolvedChild != null)
                    {
                        child = new SelectionNode(_manager, this);
                        child.Source = resolvedChild;
                    }
                    else
                    {
                        child = _manager.SharedLeafNode;
                    }
                }
                else
                {
                    child = _manager.SharedLeafNode;
                }

                _childrenNodes[index] = child;
                _realizedChildrenNodeCount++;
            }
            else
            {
                child = _childrenNodes[index];
            }
        }
        else
        {
            if (_childrenNodes.Count > 0)
            {
                Debug.Assert(0 <= index && index <= _childrenNodes.Count);
                child = _childrenNodes[index];
            }
        }

        return child;
    }

    public int SelectedCount => _selectedCount;

    public bool IsSelected(int index)
    {
        bool isSelected = false;
        for (int i = 0; i < _selected.Count; i++)
        {
            if (_selected[i].Contains(index))
            {
                isSelected = true;
                break;
            }
        }

        return isSelected;
    }

    // True  -> Selected
    // False -> Not Selected
    // Null  -> Some descendents are selected and some are not
    public bool? IsSelectedWithPartial()
    {
        bool? isSelected = false;
        if (_parent != null)
        {
            var parentsChildren = _parent._childrenNodes;
            var index = parentsChildren.IndexOf(this);
            if (index != -1)
            {
                isSelected = _parent.IsSelectedWithPartial(index);
            }
        }

        return isSelected;
    }

    // True  -> Selected
    // False -> Not Selected
    // Null  -> Some descendents are selected and some are not
    public bool? IsSelectedWithPartial(int index)
    {
        var state = SelectionState.NotSelected;
        Debug.Assert(index >= 0);

        if (_childrenNodes.Count == 0 || //no nodes realized
            _childrenNodes.Count <= index || // target node is not realized
            _childrenNodes[index] == null || // target node is not realized
            _childrenNodes[index] == _manager.SharedLeafNode) // target node is a leaf node
        {
            // Ask parent if the target node is selected
            state = IsSelected(index) ? SelectionState.Selected : SelectionState.NotSelected;
        }
        else
        {
            // targetNode is the node representing the index. This node is the parent. 
            // targetNode is a non-leaf node, containing one or many children nodes. Evaluate 
            // based on children of targetNode.
            var targetNode = _childrenNodes[index];
            state = targetNode.EvaluateIsSelectedBasedOnChildrenNodes();
        }

        return ConvertToNullableBool(state);
    }

    public int SelectedIndex()
    {
        return SelectedCount > 0 ? SelectedIndices()[0] : -1;
    }

    public void SelectedIndex(int value)
    {
        if (IsValidIndex(value) && (SelectedCount != -1 || !IsSelected(value)))
        {
            ClearSelection();

            if (value != -1)
            {
                Select(value, true);
            }
        }
    }

    public List<int> SelectedIndices()
    {
        if (!_selectedIndicesCacheIsValid)
        {
            _selectedIndicesCacheIsValid = true;
            foreach(var range in _selected)
            {
                for (int index = range.Begin; index <= range.End; index++)
                {
                    // Avoid duplicates
                    if (!_selectedIndicesCached.Contains(index))
                    {
                        _selectedIndicesCached.Add(index);
                    }
                }
            }

            // Sort the list for easy consumption
            _selectedIndicesCached.Sort();
        }

        return _selectedIndicesCached;
    }

    public bool Select(int index, bool select)
    {
        return Select(index, select, true /* raiseOnSelectionChanged */);
    }

    public bool ToggleSelect(int index)
    {
        return Select(index, !IsSelected(index));
    }

    public void SelectAll()
    {
        if (_dataSource != null)
        {
            var count = _dataSource.Count;
            if (count > 0)
            {
                SelectRange(new IndexRange(0, count - 1), true /*select*/);
            }
        }
    }

    public void Clear()
    {
        ClearSelection();
    }

    public bool SelectRange(IndexRange range, bool select)
    {
        if (IsValidIndex(range.Begin) && IsValidIndex(range.End))
        {
            if (select)
            {
                AddRange(range, true /* raiseOnSelectionChanged */);
            }
            else
            {
                RemoveRange(range, true /* raiseOnSelectionChanged */);
            }

            return true;
        }

        return false;
    }

    public void Dispose()
    {
        UnhookCollectionChangedHandler();
    }

    private void HookupCollectionChangedHandler()
    {
        if (_dataSource != null)
        {
            _dataSource.CollectionChanged += OnSourceListChanged;
        }
    }

    private void UnhookCollectionChangedHandler()
    {
        if (_dataSource != null)
        {
            _dataSource.CollectionChanged -= OnSourceListChanged;
        }
    }

    private bool IsValidIndex(int index)
    {
        return ItemsSourceView == null || (index >= 0 && index <= ItemsSourceView.Count);
    }

    private void AddRange(IndexRange addRange, bool raiseOnSelectionChanged)
    {
        // TODO: Check for duplicates (Task 14107720)
        // TODO: Optimize by merging adjacent ranges (Task 14107720)

        int oldCount = SelectedCount;
        for (int i = addRange.Begin; i <= addRange.End; i++)
        {
            if (!IsSelected(i))
            {
                _selectedCount++;
            }
        }

        if (oldCount != _selectedCount)
        {
            _selected.Add(addRange);

            if (raiseOnSelectionChanged)
            {
                OnSelectionChanged();
            }
        }
    }

    private void RemoveRange(IndexRange removeRange, bool raiseOnSelectionChanged)
    {
        int oldCount = _selectedCount;

        // TODO: Prevent overlap of Ranges in _selected (Task 14107720)
        for (int i = removeRange.Begin; i <= removeRange.End; i++)
        {
            if (IsSelected(i))
            {
                _selectedCount--;
            }
        }

        if (oldCount != _selectedCount)
        {
            // Build up a both a list of Ranges to remove and ranges to add
            var toRemove = new List<IndexRange>();
            var toAdd = new List<IndexRange>();

            foreach (var range in _selected)
            {
                // If this range intersects the remove range, we have to do something
                if (removeRange.Intersects(range))
                {
                    var before = new IndexRange(-1, -1);
                    var cut = new IndexRange(-1, -1);
                    var after = new IndexRange(-1, -1);

                    // Intersection with the beginning of the range
                    //  Anything to the left of the point (exclusive) stays
                    //  Anything to the right of the point (inclusive) gets clipped
                    if (range.Contains(removeRange.Begin - 1))
                    {
                        range.Split(removeRange.Begin - 1, out before, out cut);
                        toAdd.Add(before);
                    }

                    // Intersection with the end of the range
                    //  Anything to the left of the point (inclusive) gets clipped
                    //  Anything to the right of the point (exclusive) stays
                    if (range.Contains(removeRange.End))
                    {
                        if (range.Split(removeRange.End, out cut, out after))
                        {
                            toAdd.Add(after);
                        }
                    }

                    // Remove this Range from the collection
                    // New ranges will be added for any remaining subsections
                    toRemove.Add(range);
                }
            }

            bool change = (toRemove.Count > 0) || (toAdd.Count > 0);

            if (change)
            {
                // Remove tagged ranges
                foreach (var remove in toRemove)
                {
                    var index = _selected.IndexOf(remove);
                    _selected.RemoveAt(index);
                }

                // Add new ranges
                foreach (var add in toAdd)
                {
                    _selected.Add(add);
                }

                if (raiseOnSelectionChanged)
                {
                    OnSelectionChanged();
                }
            }
        }
    }

    private void ClearSelection()
    {
        if (_selected.Count > 0)
        {
            _selected.Clear();
            OnSelectionChanged();
        }

        _selectedCount = 0;
        AnchorIndex = -1;

        // This will throw away all the children SelectionNodes
        // causing them to be unhooked from their data source. This
        // essentially cleans up the tree.
        foreach (var node in _childrenNodes)
            node?.Dispose();
        _childrenNodes.Clear();
    }

    private bool Select(int index, bool select, bool raiseOnSelectionChanged)
    {
        if (IsValidIndex(index))
        {
            // Ignore duplicate selection calls
            if (IsSelected(index) == select)
            {
                return true;
            }

            var range = new IndexRange(index, index);

            if (select)
            {
                AddRange(range, raiseOnSelectionChanged);
            }
            else
            {
                RemoveRange(range, raiseOnSelectionChanged);
            }

            return true;
        }

        return false;
    }

    private void OnSourceListChanged(object dataSource, NotifyCollectionChangedEventArgs args)
    {
        bool selectionInvalidated = false;
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                selectionInvalidated = OnItemsAdded(args.NewStartingIndex, args.NewItems.Count);
                break;

            case NotifyCollectionChangedAction.Remove:
                selectionInvalidated = OnItemsRemoved(args.OldStartingIndex, args.OldItems.Count);
                break;

            case NotifyCollectionChangedAction.Reset:
                ClearSelection();
                selectionInvalidated = true;
                break;

            case NotifyCollectionChangedAction.Replace:
                selectionInvalidated = OnItemsRemoved(args.OldStartingIndex, args.OldItems.Count);
                selectionInvalidated |= OnItemsAdded(args.NewStartingIndex, args.NewItems.Count);
                break;
        }

        if (selectionInvalidated)
        {
            OnSelectionChanged();
            _manager.OnSelectionInvalidatedDueToCollectionChange();
        }
    }

    private bool OnItemsAdded(int index, int count)
    {
        bool selectionInvalidated = false;
        // Update ranges for leaf items
        var toAdd = new List<IndexRange>();
        for (int i = 0; i < _selected.Count; i++)
        {
            var range = _selected[i];

            // The range is after the inserted items, need to shift the range right
            if (range.End >= index)
            {
                int begin = range.Begin;
                // If the index left of newIndex is inside the range,
                // Split the range and remember the left piece to add later
                if (range.Contains(index - 1))
                {
                    var before = new IndexRange(-1, -1);
                    var after = new IndexRange(-1, -1);
                    range.Split(index - 1, out before, out after);
                    toAdd.Add(before);
                    begin = index;
                }

                // Shift the range to the right
                _selected[i] = new IndexRange(begin + count, range.End + count);
                selectionInvalidated = true;
            }
        }

        if (toAdd.Count > 0)
        {
            _selected.AddRange(toAdd);
        }

        // Update for non-leaf if we are tracking non-leaf nodes
        if (_childrenNodes.Count > 0)
        {
            selectionInvalidated = true;
            for (int i = 0; i < count; i++)
            {
                _childrenNodes.Insert(index, null);
            }
        }

        // Adjust the anchor
        if (AnchorIndex >= index)
        {
            AnchorIndex = AnchorIndex + count;
        }

        // Check if adding a node invalidated an ancestors
        // selection state. For example if parent was selected before
        // adding a new item makes the parent partially selected now.
        if (!selectionInvalidated)
        {
            var parent = _parent;
            while (parent != null)
            {
                var isSelected = parent.IsSelectedWithPartial();
                // If a parent is selected, then it will become partially selected.
                // If it is not selected or partially selected - there is no change.
                if (isSelected.HasValue && isSelected.Value)
                {
                    selectionInvalidated = true;
                    break;
                }

                parent = parent._parent;
            }
        }

        return selectionInvalidated;
    }

    private bool OnItemsRemoved(int index, int count)
    {
        bool selectionInvalidated = false;
        // Remove the items from the selection for leaf
        if (ItemsSourceView.Count > 0)
        {
            bool isSelected = false;
            for (int i = index; i <= index + count - 1; i++)
            {
                if (IsSelected(i))
                {
                    isSelected = true;
                    break;
                }
            }

            if (isSelected)
            {
                RemoveRange(new IndexRange(index, index + count - 1), false /* raiseOnSelectionChanged */);
                selectionInvalidated = true;
            }

            for (int i = 0; i < _selected.Count; i++)
            {
                var range = _selected[i];

                // The range is after the removed items, need to shift the range left
                if (range.End > index)
                {
                    Debug.Assert(!range.Contains(index));

                    // Shift the range to the left
                    _selected[i] = new IndexRange(range.Begin - count, range.End - count);
                    selectionInvalidated = true;
                }
            }

            // Update for non-leaf if we are tracking non-leaf nodes
            if (_childrenNodes.Count > 0)
            {
                selectionInvalidated = true;
                for (int i = 0; i < count; i++)
                {
                    if (_childrenNodes[index] != null)
                    {
                        _realizedChildrenNodeCount--;
                    }
                    _childrenNodes.RemoveAt(index);
                }
            }

            // Adjust the anchor
            if (AnchorIndex >= index)
            {
                AnchorIndex = AnchorIndex - count;
            }
        }
        else
        {
            // No more items in the list, clear
            ClearSelection();
            _realizedChildrenNodeCount = 0;
            selectionInvalidated = true;
        }

        // Check if removing a node invalidated an ancestors
        // selection state. For example if parent was partially selected before
        // removing an item, it could be selected now.
        if (!selectionInvalidated)
        {
            var parent = _parent;
            while (parent != null)
            {
                var isSelected = parent.IsSelectedWithPartial();
                // If a parent is partially selected, then it will become selected.
                // If it is selected or not selected - there is no change.
                if (isSelected == null)
                {
                    selectionInvalidated = true;
                    break;
                }

                parent = parent._parent;
            }
        }

        return selectionInvalidated;
    }

    private void OnSelectionChanged()
    {
        _selectedIndicesCacheIsValid = false;
        _selectedIndicesCached.Clear();
    }

    internal static bool? ConvertToNullableBool(SelectionState isSelected)
    {
        return isSelected == SelectionState.Selected ? true :
            isSelected == SelectionState.NotSelected ? false : null;
    }

    internal SelectionState EvaluateIsSelectedBasedOnChildrenNodes()
    {
        var selectionState = SelectionState.NotSelected;
        var realizedChildrenNodeCount = RealizedChildrenNodeCount;
        int selectedCount = SelectedCount;

        if (realizedChildrenNodeCount != 0 || selectedCount != 0)
        {
            // There are realized children or some selected leaves.
            var dataCount = DataCount;
            if (realizedChildrenNodeCount == 0 && selectedCount > 0)
            {
                // All nodes are leaves under it - we didn't create children nodes as an optimization.
                // See if all/some or none of the leaves are selected.
                selectionState = dataCount != selectedCount ?
                    SelectionState.PartiallySelected :
                    dataCount == selectedCount ? SelectionState.Selected : SelectionState.NotSelected;
            }
            else
            {
                // There are child nodes, walk them individually and evaluate based on each child
                // being selected/not selected or partially selected.
                selectedCount = 0;
                int notSelectedCount = 0;
                for (int i = 0; i < ChildrenNodeCount; i++)
                {
                    var child = GetAt(i, false /* realizeChild */);
                    if (child != null)
                    {
                        // child is realized, ask it.
                        var isChildSelected = IsSelectedWithPartial(i);
                        if (isChildSelected == null)
                        {
                            selectionState = SelectionState.PartiallySelected;
                            break;
                        }
                        else if (isChildSelected.HasValue && isChildSelected.Value)
                        {
                            selectedCount++;
                        }
                        else
                        {
                            notSelectedCount++;
                        }
                    }
                    else
                    {
                        // not realized
                        if (IsSelected(i))
                        {
                            selectedCount++;
                        }
                        else
                        {
                            notSelectedCount++;
                        }
                    }

                    if (selectedCount > 0 && notSelectedCount > 0)
                    {
                        selectionState = SelectionState.PartiallySelected;
                        break;
                    }
                }

                if (selectionState != SelectionState.PartiallySelected)
                {
                    if (selectedCount != 0 && selectedCount != dataCount)
                    {
                        selectionState = SelectionState.PartiallySelected;
                    }
                    else
                    {
                        selectionState = selectedCount == dataCount ? SelectionState.Selected : SelectionState.NotSelected;
                    }
                }
            }
        }

        return selectionState;
    }


    private readonly SelectionModel _manager;
    private readonly List<SelectionNode> _childrenNodes = new List<SelectionNode>();
    private readonly SelectionNode _parent;
    private readonly List<IndexRange> _selected = new List<IndexRange>();
    private object _source;
    private ItemsSourceView _dataSource;
    private int _selectedCount;
    private readonly List<int> _selectedIndicesCached = new List<int>();
    private bool _selectedIndicesCacheIsValid;
    private int _anchorIndex = -1;
    private int _realizedChildrenNodeCount;
}

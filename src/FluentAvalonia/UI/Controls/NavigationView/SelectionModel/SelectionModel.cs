using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

internal class SelectionModel : INotifyPropertyChanged, IDisposable
{
    public SelectionModel()
    {
        // Parent is null for root node.
        _rootNode = new SelectionNode(this, null);
        // Parent is null for leaf node since it is shared. This is ok since we just
        // use the leaf as a placeholder and never ask stuff of it.
        SharedLeafNode = new SelectionNode(this, null);
    }

    public object Source
    {
        get => _rootNode.Source;
        set
        {
            ClearSelection();
            _rootNode.Source = value;
            OnSelectionChanged();
            RaisePropertyChanged();
        }
    }

    public bool SingleSelect
    {
        get => _singleSelect;
        set
        {
            if (_singleSelect != value)
            {
                _singleSelect = value;
                var selectedIndices = SelectedIndices;

                // Only update selection and raise SelectionChanged event when:
                // - we switch from SelectionMode::Multiple to SelectionMode::Single and
                // - more than one item was selected at the time of the switch
                if (value && selectedIndices != null && selectedIndices.Count > 1)
                {
                    // We want to be single select, so make sure there is only 
                    // one selected item.
                    var firstSelectionIndexPath = selectedIndices[0];
                    ClearSelection();
                    SelectWithPathImpl(firstSelectionIndexPath, true /* select */, true /* raiseSelectionChanged */);
                }

                RaisePropertyChanged();
            }
        }
    }

    public IndexPath AnchorIndex
    {
        get
        {
            IndexPath anchor = IndexPath.Unselected;
            if (_rootNode.AnchorIndex >= 0)
            {
                var path = new List<int>();
                var current = _rootNode;
                while (current != null && current.AnchorIndex >= 0)
                {
                    path.Add(current.AnchorIndex);
                    current = current.GetAt(current.AnchorIndex, false);
                }

                anchor = new IndexPath(path);
            }

            return anchor;
        }
        set
        {
            if (value != IndexPath.Unselected)
            {
                SelectionTreeHelper.TraverseIndexPath(_rootNode, value, true /* realizeChildren */,
                    (childNode, path, depth, childIndex) =>
                    {
                        childNode.AnchorIndex = path.GetAt(depth);
                    });
            }
            else
            {
                _rootNode.AnchorIndex = -1;
            }

            RaisePropertyChanged();
        }
    }

    public IndexPath SelectedIndex
    {
        get
        {
            var selectedIndex = IndexPath.Unselected;
            var selectedIndices = SelectedIndices;
            if (selectedIndices != null && selectedIndices.Count > 0)
            {
                selectedIndex = selectedIndices[0];
            }

            return selectedIndex;
        }
        set
        {
            var isSelected = IsSelectedAt(value);
            if (isSelected == null || (isSelected.HasValue && !isSelected.Value))
            {
                ClearSelection(true /* resetAnchor */, false /* raiseSelectionChanged */);
                SelectWithPathImpl(value, true /* select */, false /* raiseSelectionChanged */);
                OnSelectionChanged();
            }
        }
    }

    public object SelectedItem
    {
        get
        {
            object item = null;
            var selectedItems = SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
                item = selectedItems[0];
            }

            return item;
        }
    }

    public IReadOnlyList<object> SelectedItems
    {
        get
        {
            if (_selectedItemsCached == null)
            {
                var selectedInfos = new List<SelectedItemInfo>();
                if (_rootNode.Source != null)
                {
                    SelectionTreeHelper.Traverse(_rootNode, false /* realizeChildren */,
                        currentInfo =>
                        {
                            if (currentInfo.Node.SelectedCount > 0)
                            {
                                selectedInfos.Add(new SelectedItemInfo(currentInfo.Node, currentInfo.Path));
                            }
                        });
                }

                // Instead of creating a dumb vector that takes up the space for all the selected items,
                // we create a custom VectorView implimentation that calls back using a delegate to find 
                // the selected item at a particular index. This avoid having to create the storage and copying
                // needed in a dumb vector. This also allows us to expose a tree of selected nodes into an 
                // easier to consume flat vector view of objects.
                var selectedItems = new SelectedItems<object>(selectedInfos,
                    (infos, index) =>
                    {
                        var currentIndex = 0;
                        object item = null;
                        foreach (var info in infos)
                        {
                            if (info.Node.TryGetTarget(out var node))
                            {
                                var currentCount = node.SelectedCount;
                                if (index >= currentIndex && index < currentIndex + currentCount)
                                {
                                    int targetIndex = node.SelectedIndices()[index - currentIndex];
                                    item = node.ItemsSourceView.GetAt(targetIndex);
                                    break;
                                }

                                currentIndex += currentCount;
                            }
                            else
                            {
                                throw new InvalidOperationException("Selection has changed since SelectedItems property was read.");
                            }
                        }

                        return item;
                    });

                _selectedItemsCached = selectedItems;
            }

            return _selectedItemsCached;
        }
    }

    public IReadOnlyList<IndexPath> SelectedIndices
    {
        get
        {
            if (_selectedIndicesCached == null)
            {
                var selectedInfos = new List<SelectedItemInfo>();
                SelectionTreeHelper.Traverse(_rootNode, false /* realizeChildren */,
                    currentInfo =>
                    {
                        if (currentInfo.Node.SelectedCount > 0)
                        {
                            selectedInfos.Add(new SelectedItemInfo(currentInfo.Node, currentInfo.Path));
                        }
                    });

                // Instead of creating a dumb vector that takes up the space for all the selected indices,
                // we create a custom VectorView implimentation that calls back using a delegate to find 
                // the IndexPath at a particular index. This avoid having to create the storage and copying
                // needed in a dumb vector. This also allows us to expose a tree of selected nodes into an 
                // easier to consume flat vector view of IndexPaths.
                var indices = new SelectedItems<IndexPath>(selectedInfos,
                    (infos, index) =>
                    {
                        int currentIndex = 0;
                        IndexPath path = IndexPath.Unselected;
                        foreach(var info in infos)
                        {
                            if (info.Node.TryGetTarget(out var node))
                            {
                                int currentCount = node.SelectedCount;
                                if (index >= currentIndex && index < currentIndex + currentCount)
                                {
                                    int targetIndex = node.SelectedIndices()[index - currentIndex];
                                    path = info.Path.CloneWithChildIndex(targetIndex);
                                    break;
                                }

                                currentIndex += currentCount;
                            }
                            else
                            {
                                throw new InvalidOperationException("Selection has changed since SelectedIndices property was read.");
                            }
                        }

                        return path;
                    });

                _selectedIndicesCached = indices;
            }

            return _selectedIndicesCached;
        }
    }

    internal SelectionNode SharedLeafNode { get; private set; }

    public event EventHandler<SelectionModelChildrenRequestedEventArgs> ChildrenRequested;
    public event EventHandler<SelectionModelSelectionChangedEventArgs> SelectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public void SetAnchorIndex(int index) => AnchorIndex = new IndexPath(index);

    public void SetAnchorIndex(int groupIndex, int itemIndex) => new IndexPath(groupIndex, itemIndex);

    public void Select(int index)
    {
        SelectImpl(index, true /* select */);
    }

    public void Select(int groupIndex, int itemIndex)
    {
        SelectWithGroupImpl(groupIndex, itemIndex, true /* select */);
    }

    public void SelectAt(IndexPath index)
    {
        SelectWithPathImpl(index, true /* select */, true /* raiseSelectionChanged */);
    }

    public void Deselect(int index)
    {
        SelectImpl(index, false /* select */);
    }

    public void Deselect(int groupIndex, int itemIndex)
    {
        SelectWithGroupImpl(groupIndex, itemIndex, false /* select */);
    }

    public void DeselectAt(IndexPath index)
    {
        SelectWithPathImpl(index, false /* select */, true /* raiseSelectionChanged */);
    }

    public bool? IsSelected(int index)
    {
        Debug.Assert(index >= 0);
        return _rootNode.IsSelectedWithPartial(index);
    }

    public bool? IsSelected(int groupIndex, int itemIndex)
    {
        Debug.Assert(groupIndex >= 0 && itemIndex >= 0);
        bool? isSelected = false;
        var childNode = _rootNode.GetAt(groupIndex, false);
        if (childNode != null)
        {
            isSelected = childNode.IsSelectedWithPartial(itemIndex);
        }

        return isSelected;
    }

    public bool? IsSelectedAt(IndexPath index)
    {
        var path = index;
        Debug.Assert(path.IsValid());
        bool isRealized = true;
        var node = _rootNode;
        for (int i = 0; i < path.GetSize() - 1; i++)
        {
            var childIndex = path.GetAt(i);
            node = node.GetAt(childIndex, false);
            if (node == null)
            {
                isRealized = false;
                break;
            }
        }

        bool? isSelected = false;
        if (isRealized)
        {
            var size = path.GetSize();
            if (size == 0)
            {
                isSelected = SelectionNode.ConvertToNullableBool(node.EvaluateIsSelectedBasedOnChildrenNodes());
            }
            else
            {
                isSelected = node.IsSelectedWithPartial(path.GetAt(size - 1));
            }
        }

        return isSelected;
    }

    public void SelectRangeFromAnchor(int index)
    {
        SelectRangeFromAnchorImpl(index, true);
    }

    public void SelectRangeFromAnchor(int endGroupIndex, int endItemIndex)
    {
        SelectRangeFromAnchorWithGroupImpl(endGroupIndex, endItemIndex, true);
    }

    public void SelectRangeFromAnchorTo(IndexPath index)
    {
        SelectRangeImpl(AnchorIndex, index, true);
    }

    public void DeselectRangeFromAnchor(int index)
    {
        SelectRangeFromAnchorImpl(index, false);
    }

    public void DeselectRangeFromAnchor(int endGroupIndex, int endItemIndex)
    {
        SelectRangeFromAnchorWithGroupImpl(endGroupIndex, endItemIndex, false);
    }

    public void DeselectRangeFromAnchorTo(IndexPath index)
    {
        SelectRangeImpl(AnchorIndex, index, false);
    }

    public void SelectRange(IndexPath start, IndexPath end)
    {
        SelectRangeImpl(start, end, true);
    }

    public void DeselectRange(IndexPath start, IndexPath end)
    {
        SelectRangeImpl(start, end, false);
    }

    public void SelectAll()
    {
        SelectionTreeHelper.Traverse(_rootNode, true,
            info =>
            {
                if (info.Node.DataCount > 0)
                {
                    info.Node.SelectAll();
                }
            });

        OnSelectionChanged();
    }

    public void ClearSelection()
    {
        ClearSelection(true, true);
    }

    public void Dispose()
    {
        ClearSelection(false, false);
        _rootNode = null;
        SharedLeafNode = null;
        _selectedIndicesCached = null;
        _selectedItemsCached = null;
    }

    // Skip ICustomPropertyProvider...

    private void OnPropertyChanged(string propertyName = null) // Needed??
    {
        RaisePropertyChanged(propertyName);
    }

    private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void OnSelectionInvalidatedDueToCollectionChange()
    {
        OnSelectionChanged();
    }

    internal object ResolvePath(object data, IndexPath dataIndexPath)
    {
        object resolved = null;
        // Raise ChildrenRequested event if there is a handler
        if (ChildrenRequested != null)
        {
            if (_childrenRequestedEventArgs == null)
            {
                _childrenRequestedEventArgs = new SelectionModelChildrenRequestedEventArgs(data, dataIndexPath, false);
            }
            else
            {
                _childrenRequestedEventArgs.Initialize(data, dataIndexPath, false);
            }

            ChildrenRequested?.Invoke(this, _childrenRequestedEventArgs);
            resolved = _childrenRequestedEventArgs.Children;

            // Clear out the values in the args so that it cannot be used after the event handler call.
            _childrenRequestedEventArgs.Initialize(null, IndexPath.Unselected, true);
        }
        else
        {
            // No handlers for ChildrenRequested event. If data is of type ItemsSourceView
            // or a type that can be used to create a ItemsSourceView using ItemsSourceView::CreateFrom, then we can
            // auto-resolve that as the child. If not, then we consider the value as a leaf. This is to 
            // avoid having to provide the event handler for the most common scenarios. If the app dev does
            // not want this default behavior, they can provide the handler to override.
            if (data is ItemsSourceView || data is IEnumerable)
            {
                resolved = data;
            }
        }

        return resolved;
    }

    private void ClearSelection(bool resetAnchor, bool raiseSelectionChanged)
    {
        SelectionTreeHelper.Traverse(_rootNode, false,
            info =>
            {
                info.Node.Clear();
            });

        if (resetAnchor)
        {
            AnchorIndex = IndexPath.Unselected;
        }

        if (raiseSelectionChanged)
        {
            OnSelectionChanged();
        }
    }

    private void OnSelectionChanged()
    {
        _selectedIndicesCached = null;
        _selectedItemsCached = null;

        if (SelectionChanged != null)
        {
            if (_selectionChangedEventArgs == null)
            {
                _selectionChangedEventArgs = new SelectionModelSelectionChangedEventArgs();
            }

            SelectionChanged.Invoke(this, _selectionChangedEventArgs);
        }

        RaisePropertyChanged(nameof(SelectedIndex));
        RaisePropertyChanged(nameof(SelectedIndices));
        if (_rootNode.Source != null)
        {
            RaisePropertyChanged(nameof(SelectedItem));
            RaisePropertyChanged(nameof(SelectedItems));
        }
    }

    private void SelectImpl(int index, bool select)
    {
        if (_rootNode.IsSelected(index) != select)
        {
            if (_singleSelect)
            {
                ClearSelection(true, false);
            }
            var selected = _rootNode.Select(index, select);
            if (selected)
            {
                AnchorIndex = new IndexPath(index);
            }
            OnSelectionChanged();
        }
    }

    private void SelectWithGroupImpl(int groupIndex, int itemIndex, bool select)
    {
        if (_singleSelect)
        {
            ClearSelection(true, false);
        }

        var childNode = _rootNode.GetAt(groupIndex, true);
        var selected = childNode.Select(itemIndex, select);
        if (selected)
        {
            AnchorIndex = new IndexPath(groupIndex, itemIndex);
        }

        OnSelectionChanged();
    }

    private void SelectWithPathImpl(IndexPath index, bool select, bool raiseSelectionChanged)
    {
        bool newSelection = true;

        // Handle single select differently as comparing indexpaths is faster
        if (_singleSelect)
        {
            var selectedIndex = SelectedIndex;
            if (selectedIndex != IndexPath.Unselected)
            {
                if (select && selectedIndex.CompareTo(index) == 0)
                {
                    newSelection = false;
                }
            }
            else
            {
                // If we are in single select and selectedIndex is null, deselecting is not a new change.
                // Selecting something is a new change, so set flag to appropriate value here.
                newSelection = select;
            }
        }

        // Selection is actually different from previous one, so update.
        if (newSelection)
        {
            bool selected = false;
            // If we unselect something, raise event any way, otherwise changedSelection is false
            bool changedSelection = false;

            // We only need to clear selection by walking the data structure from the beginning when:
            // - we are in single selection mode and 
            // - want to select something.
            // 
            // If we want to unselect something we unselect it directly in TraverseIndexPath below and raise the SelectionChanged event
            // if required.
            if (_singleSelect && select)
            {
                ClearSelection(true, false);
            }

            SelectionTreeHelper.TraverseIndexPath(_rootNode, index, true,
                (currentNode, path, depth, childIndex) =>
                {
                    if (depth == path.GetSize() - 1)
                    {
                        if (currentNode.IsSelected(childIndex) != select)
                        {
                            // Node has different value then we want to set, so lets update!
                            changedSelection = true;
                        }
                        selected = currentNode.Select(childIndex, select);
                    }
                });

            if (selected)
            {
                AnchorIndex = index;
            }

            // The walk tree operation can change the indices, and the next time it get's read,
            // we would throw an exception. That's what we are preventing with next two lines
            _selectedIndicesCached = null;
            _selectedItemsCached = null;

            if (raiseSelectionChanged && changedSelection)
            {
                OnSelectionChanged();
            }
        }
    }

    private void SelectRangeFromAnchorImpl(int index, bool select)
    {
        int anchorIndex = 0;
        var anchor = AnchorIndex;
        if (anchor != IndexPath.Unselected)
        {
            Debug.Assert(anchor.GetSize() == 1);
            anchorIndex = anchor.GetAt(0);
        }

        bool selected = _rootNode.SelectRange(new IndexRange(anchorIndex, index), select);
        if (selected)
        {
            OnSelectionChanged();
        }
    }

    private void SelectRangeFromAnchorWithGroupImpl(int endGroupIndex, int endItemIndex, bool select)
    {
        int startGroupIndex = 0;
        int startItemIndex = 0;
        var anchorIndex = AnchorIndex;
        if (anchorIndex != IndexPath.Unselected)
        {
            Debug.Assert(anchorIndex.GetSize() == 2);
            startGroupIndex = anchorIndex.GetAt(0);
            startItemIndex = anchorIndex.GetAt(1);
        }

        // make sure start > end
        if (startGroupIndex > endGroupIndex ||
            (startGroupIndex == endGroupIndex && startItemIndex > endItemIndex))
        {
            int temp = startGroupIndex;
            startGroupIndex = endGroupIndex;
            endGroupIndex = temp;
            temp = startItemIndex;
            startItemIndex = endItemIndex;
            endItemIndex = temp;
        }

        bool selected = false;
        for (int groupIdx = startGroupIndex; groupIdx <= endGroupIndex; groupIdx++)
        {
            var groupNode = _rootNode.GetAt(groupIdx, true);
            var startIndex = groupIdx == startGroupIndex ? startItemIndex : 0;
            var endIndex = groupIdx == endGroupIndex ? endItemIndex : groupNode.DataCount - 1;
            selected |= groupNode.SelectRange(new IndexRange(startIndex, endIndex), select);
        }

        if (selected)
        {
            OnSelectionChanged();
        }
    }

    private void SelectRangeImpl(IndexPath start, IndexPath end, bool select)
    {
        var winrtStart = start;
        var winrtEnd = end;

        // Make sure start <= end
        if (winrtEnd.CompareTo(winrtStart) == -1)
        {
            var temp = winrtStart;
            winrtStart = winrtEnd;
            winrtEnd = temp;
        }

        // Note: Since we do not know the depth of the tree, we have to walk to each leaf
        SelectionTreeHelper.TraverseRangeRealizeChildren(_rootNode, winrtStart, winrtEnd,
            info =>
            {
                if (info.Node.DataCount == 0)
                {
                    info.ParentNode.Select(info.Path.GetAt(info.Path.GetSize() - 1), select);
                }
            });

        OnSelectionChanged();
    }


    private SelectionNode _rootNode;
    private bool _singleSelect;
    private IReadOnlyList<IndexPath> _selectedIndicesCached;
    private IReadOnlyList<object> _selectedItemsCached;

    private SelectionModelChildrenRequestedEventArgs _childrenRequestedEventArgs;
    private SelectionModelSelectionChangedEventArgs _selectionChangedEventArgs;
}



using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using FluentAvalonia.Core;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

internal class Phaser
{
    public Phaser(ItemsRepeater owner)
    {
        _owner = owner;
        // ItemsRepeater is not fully constructed yet. Don't interact with it.
    }

    public void PhaseElement(Control element, VirtualizationInfo virtInfo, ContainerContentChangingEventArgs cArgs)
    {
        _pendingElements ??= new List<ElementInfo>();

        // Insert at the top since we remove from bottom during DoPhasedWorkCallback. This keeps the ordering of items
        // the same as the order in which items are realized.
        _pendingElements.Insert(0, new ElementInfo(element, cArgs.Phase, cArgs.callback, virtInfo));

        RegisterForCallback();        
    }

    public void StopPhasing(Control element, VirtualizationInfo virtInfo)
    {
        // We need to remove the element from the pending elements list. We cannot just change the phase to -1
        // since it will get updated when the element gets recycled.
        for (int i = _pendingElements.Count - 1; i >= 0; i--)
        {
            if (_pendingElements[i].Element == element)
            {
                _pendingElements.RemoveAt(i);
                // Because of the way we do this compared to WinUI, its possible to have multiple entries in 
                // _pendingElements with the same control - so we can't break here, we must search
                //break;
            }
        }

        // We no longer store the phase information on VirtualizationInfo because we handle it differently
        virtInfo.UpdatePhasingInfo(null /*data*/);
    }

    public void DoPhasedWorkCallback()
    {
        MarkCallbackReceived();

        if (_pendingElements != null && _pendingElements.Count > 0 && !BuildTreeScheduler.ShouldYield())
        {
            var visibleWindow = _owner.VisibleWindow;
            SortElements(visibleWindow);
            int currentIndex = _pendingElements.Count - 1;
            do
            {
                var info = _pendingElements[currentIndex];
                var element = info.Element;
                var virtInfo = info.VirtInfo;
                var dataIndex = virtInfo.Index;

                var currentPhase = info.Phase;

                if (currentPhase > 0)
                {
                    var args = new ContainerContentChangingEventArgs(dataIndex,
                        virtInfo.Data, element, virtInfo, currentPhase, this);

                    info.Callback.Invoke(_owner, args);

                    int nextPhase = VirtualizationInfo.PhaseReachedEnd;
                    
                    ValidatePhaseOrdering(currentPhase, nextPhase);

                    var previousAvailableSize = LayoutInformation.GetPreviousMeasureConstraint(element);
                    element.Measure(previousAvailableSize.Value);
                    
                    if (nextPhase > 0)
                    {
                        // If we are the first item or 
                        // If the current items phase is higher than the next items phase, then move to the next item.
                        if (currentIndex == 0 ||
                            nextPhase > _pendingElements[currentIndex - 1].Phase)
                        {
                            currentIndex--;
                        }
                    }
                    else
                    {
                        _pendingElements.RemoveAt(currentIndex);
                        currentIndex--;
                    }
                }
                else
                {
                    _pendingElements.RemoveAt(currentIndex);
                    currentIndex--;
                    //throw new InvalidOperationException("Cleared element found in pending list which is not expected");
                }

                var pendingCount = _pendingElements.Count;
                if (currentIndex == -1)
                {
                    // Reached the top, start from the bottom again
                    currentIndex = pendingCount - 1;
                }
                else if (currentIndex > -1 && currentIndex < pendingCount - 1)
                {
                    // If the next element is oustide the visible window and there are elements in the visible window
                    // go back to the visible window.
                    bool nextItemIsVisible = visibleWindow.Intersects(
                        _pendingElements[currentIndex].LastArrangeBounds);
                    if (!nextItemIsVisible)
                    {
                        bool haveVisibleItems = visibleWindow.Intersects(
                            _pendingElements[pendingCount - 1].LastArrangeBounds);
                        if (haveVisibleItems)
                        {
                            currentIndex = pendingCount - 1;
                        }
                    }
                }
            }
            while (_pendingElements.Count > 0 && !BuildTreeScheduler.ShouldYield());
        }

        //Debug.Assert(!_registeredForCallbacks);

        if (_pendingElements.Count > 0)
            RegisterForCallback();        
    }

    private void RegisterForCallback()
    {
        if (!_registeredForCallbacks)
        {
            Debug.Assert(_pendingElements.Count > 0);
            _registeredForCallbacks = true;
            BuildTreeScheduler.RegisterWork(
                _pendingElements[_pendingElements.Count - 1].Phase,// Use the phase of the last one in the sorted list
                DoPhasedWorkCallback);
        }
    }

    private void MarkCallbackReceived()
    {
        _registeredForCallbacks = false;
    }

    private static void ValidatePhaseOrdering(int currentPhase, int nextPhase)
    {
        if (nextPhase > 0 && nextPhase <= currentPhase)
        {
            // nextPhase <= currentPhase is invalid
            throw new InvalidOperationException("Phases are required to be monotonically increasing.");
        }
    }

    private void SortElements(Rect visibleWindow)
    {
        // TODO: std::sort returns true/false, C# wants -1,0,1; not sure if this is 100% correct
        _pendingElements.Sort((lhs, rhs) =>
        {
            var lhsBounds = lhs.LastArrangeBounds;
            var lhsIntersects = visibleWindow.Intersects(lhsBounds);
            var rhsBounds = rhs.LastArrangeBounds;
            var rhsIntersects = visibleWindow.Intersects(rhsBounds);

            if ((lhsIntersects && rhsIntersects) ||
                (!lhsIntersects && !rhsIntersects))
            {
                // Both are in the visible window or both are not
                return lhs.Phase.CompareTo(rhs.Phase); // ??
            }
            else if (lhsIntersects)
            {
                // Left is in the visible window
                return 0; // C++ returns false
            }
            else
            {
                return 1; // C++ returns true
            }
        });
    }

    private readonly ItemsRepeater _owner;
    private List<ElementInfo> _pendingElements;
    private bool _registeredForCallbacks;

    private readonly struct ElementInfo
    {
        public ElementInfo(Control element, int phase, TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> callback, 
            VirtualizationInfo virtInfo)
        {
            Element = element;
            VirtInfo = virtInfo;
            Phase = phase;
            LastArrangeBounds = virtInfo.ArrangeBounds;
            Callback = callback;
        }

        public Control Element { get; }

        public int Phase { get; }

        public Rect LastArrangeBounds { get; }

        public TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> Callback { get; }

        public VirtualizationInfo VirtInfo { get; }
    }
}

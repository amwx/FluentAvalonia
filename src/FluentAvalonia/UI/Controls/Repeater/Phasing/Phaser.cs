using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

internal class Phaser
{
    public Phaser(ItemsRepeater owner)
    {
        _owner = owner;
        // ItemsRepeater is not fully constructed yet. Don't interact with it.
    }

    public void PhaseElement(Control element, VirtualizationInfo virtInfo)
    {
       // var dataTemplateComponent = virtInfo.DataTemplateComponent;
        var nextPhase = virtInfo.Phase;
        bool shouldPhase = false;

        if (nextPhase > 0)
        {
            //if (dataTemplateComponent != null)
            //{
            //    // Perf optimization: RecyclingElementFactory sets up the virtualization info so we don't need to update it here.
                shouldPhase = true;
            //}
            //else
            //{
            //    throw new InvalidOperationException("Phase was set on virtualization info, but dataTemplateComponent was not.");
            //}
        }
        else if (nextPhase == VirtualizationInfo.PhaseNotSpecified)
        {
            // If virtInfo->Phase is not specified, virtInfo->DataTemplateComponent cannot be valid.
            //Debug.Assert(dataTemplateComponent == null);
            // ItemsRepeater might be using a custom view generator in which case, virtInfo would not be bootstrapped.
            // In this case, fallback to querying for the data template component and setup virtualization info now.

            //if (dataTemplateComponent)
            //{
            //    // Clear out old data. 
            //    dataTemplateComponent.Recycle();

            //    nextPhase = VirtualizationInfo::PhaseReachedEnd;
            //    const auto index = virtInfo->Index();
            //    const auto data = m_owner->ItemsSourceView().GetAt(index);
            //    // Run Phase 0
            //    dataTemplateComponent.ProcessBindings(data, index, 0 /* currentPhase */, nextPhase);

            //    // Update phase on virtInfo. Set data and templateComponent only if x:Phase was used.
            //    virtInfo->UpdatePhasingInfo(nextPhase, nextPhase > 0 ? data : nullptr, nextPhase > 0 ? dataTemplateComponent : nullptr);
            //    shouldPhase = nextPhase > 0;
            //}
        }

        if (shouldPhase)
        {
            _pendingElements ??= new List<ElementInfo>();
            // Insert at the top since we remove from bottom during DoPhasedWorkCallback. This keeps the ordering of items
            // the same as the order in which items are realized.
            _pendingElements.Insert(0, new ElementInfo(element, virtInfo));
            RegisterForCallback();
        }
    }

    public void StopPhasing(Control element, VirtualizationInfo virtInfo)
    {
        // We need to remove the element from the pending elements list. We cannot just change the phase to -1
        // since it will get updated when the element gets recycled.
        if (virtInfo.ContainerChangingCallback != null)
        {
            for (int i = _pendingElements.Count - 1; i >= 0; i--)
            {
                if (_pendingElements[i].Element == element)
                {
                    _pendingElements.RemoveAt(i);
                    break;
                }
            }
        }

        virtInfo.UpdatePhasingInfo(VirtualizationInfo.PhaseNotSpecified, null /*data*/, null /*dataTemplateComponent*/);
    }

    public void DoPhasedWorkCallback()
    {
        MarkCallbackReceived();

        if (_pendingElements != null && _pendingElements.Count > 0 && !BuildTreeScheduler.ShouldYield())
        {
            var visibleWindow = (Rect)_owner.VisibleWindow;
            SortElements(visibleWindow);
            int currentIndex = _pendingElements.Count - 1;
            do
            {
                var info = _pendingElements[currentIndex];
                var element = info.Element;
                var virtInfo = info.VirtInfo;
                var dataIndex = virtInfo.Index;

                var currentPhase = virtInfo.Phase;
                if (currentPhase > 0)
                {
                    var args = new ContainerContentChangingEventArgs(dataIndex,
                        virtInfo.Data, element, virtInfo, currentPhase);
                    virtInfo.ContainerChangingCallback.Invoke(_owner, args);
                    int nextPhase = virtInfo.Phase == args.Phase ?
                        VirtualizationInfo.PhaseReachedEnd : virtInfo.Phase;// VirtualizationInfo.PhaseReachedEnd;
                    //virtInfo.DataTemplateComponent.ProcessBindings(virtInfo.Data, -1, currentPhase, nextPhase);
                    ValidatePhaseOrdering(currentPhase, nextPhase);

                    var previousAvailableSize = LayoutInformation.GetPreviousMeasureConstraint(element);
                    element.Measure(previousAvailableSize.Value);
                    
                    if (nextPhase > 0)
                    {
                        virtInfo.Phase = nextPhase;
                        // If we are the first item or 
                        // If the current items phase is higher than the next items phase, then move to the next item.
                        if (currentIndex == 0 ||
                            virtInfo.Phase > _pendingElements[currentIndex - 1].VirtInfo.Phase)
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
                        _pendingElements[currentIndex].VirtInfo.ArrangeBounds);
                    if (!nextItemIsVisible)
                    {
                        bool haveVisibleItems = visibleWindow.Intersects(
                            _pendingElements[pendingCount - 1].VirtInfo.ArrangeBounds);
                        if (haveVisibleItems)
                        {
                            currentIndex = pendingCount - 1;
                        }
                    }
                }
            }
            while (_pendingElements.Count > 0 && !BuildTreeScheduler.ShouldYield());
        }

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
                _pendingElements[_pendingElements.Count - 1].VirtInfo.Phase,// Use the phase of the last one in the sorted list
                () => DoPhasedWorkCallback());
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
            var lhsBounds = lhs.VirtInfo.ArrangeBounds;
            var lhsIntersects = visibleWindow.Intersects(lhsBounds);
            var rhsBounds = rhs.VirtInfo.ArrangeBounds;
            var rhsIntersects = visibleWindow.Intersects(rhsBounds);

            if ((lhsIntersects && rhsIntersects) ||
                (!lhsIntersects && !rhsIntersects))
            {
                // Both are in the visible window or both are not
                return lhs.VirtInfo.Phase.CompareTo(rhs.VirtInfo.Phase); // ??
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

    private struct ElementInfo
    {
        public ElementInfo(Control element, VirtualizationInfo virtInfo)
        {
            Element = element;
            VirtInfo = virtInfo;
        }

        public Control Element { get; }

        public VirtualizationInfo VirtInfo { get; }
    }
}

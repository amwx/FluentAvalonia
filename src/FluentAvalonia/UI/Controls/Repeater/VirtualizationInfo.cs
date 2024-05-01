using Avalonia;

namespace FluentAvalonia.UI.Controls;

internal class VirtualizationInfo
{
    public VirtualizationInfo()
    {
        ArrangeBounds = ItemsRepeater.InvalidRect;
    }

    public int Index => _index;

    public bool IsPinned => _pinCounter > 0;

    public bool IsHeldByLayout => _owner == ElementOwner.Layout;

    public bool IsRealized => IsHeldByLayout || _owner == ElementOwner.PinnedPool;

    public bool IsInUniqueIdResetPool => _owner == ElementOwner.UniqueIdResetPool;

    public bool AutoRecycleCandidate { get; set; }

    public bool MustClearDataContext { get; set; }

    // In WinUI, this is a property on UIElement, but we don't have that
    // This is one solution (also could be an attached property, but that
    // would be an extra property read and possible perf issue)
    public bool CanBeScrollAnchor { get; set; }

    public ElementOwner Owner => _owner;

    public bool KeepAlive { get; set; }

    public Rect ArrangeBounds { get; set; }

    public string UniqueId => _uniqueId;

    public object Data => _data == null ? null :
        _data.TryGetTarget(out var target) ? target : null;
 
    internal void UpdatePhasingInfo(object data)
    {
        _data = new WeakReference<object>(data);
    }

    internal void MoveOwnershipToLayoutFromElementFactory(int index, string uniqueId)
    {
        _owner = ElementOwner.Layout;
        _index = index;
        _uniqueId = uniqueId;
    }

    internal void MoveOwnershipToLayoutFromUniqueIdResetPool()
    {
        _owner = ElementOwner.Layout;
    }

    internal void MoveOwnershipToLayoutFromPinnedPool()
    {
        _owner = ElementOwner.Layout;
    }

    internal void MoveOwnershipToElementFactory()
    {
        _owner = ElementOwner.ElementFactory;
        _pinCounter = 0;
        _index = -1;
        _uniqueId = null;
        ArrangeBounds = ItemsRepeater.InvalidRect;
    }

    internal void MoveOwnershipToUniqueIdResetPoolFromLayout()
    {
        _owner = ElementOwner.UniqueIdResetPool;
        // Keep the pinCounter the same. If the container survives the reset
        // it can go on being pinned as if nothing happened.
    }

    internal void MoveOwnershipToAnimator()
    {
        // During a unique id reset, some elements might get removed.
        // Their ownership will go from the UniqueIdResetPool to the Animator.
        // The common path though is for ownership to go from Layout to Animator.
        _owner = ElementOwner.Animator;
        _index = -1;
        _pinCounter = 0;
    }

    internal void MoveOwnershipToPinnedPool()
    {
        _owner = ElementOwner.PinnedPool;
    }

    internal uint AddPin()
    {
        if (!IsRealized)
            throw new InvalidOperationException("You can't pin an unrealized element");

        return ++_pinCounter;
    }

    internal uint RemovePin()
    {
        if (!IsRealized)
            throw new InvalidOperationException("You can't unpin an unrealized element");

        if (!IsPinned)
            throw new InvalidOperationException("UnpinElement was called more often than PinElement");

        return --_pinCounter;
    }

    internal void UpdateIndex(int newIndex)
    {
        _index = newIndex;
    }

    private uint _pinCounter;
    private int _index = -1;
    private string _uniqueId;
    private ElementOwner _owner;

    private WeakReference<object> _data;    

    internal const int PhaseReachedEnd = -1;

    public enum ElementOwner
    {
        ElementFactory,
        Layout,
        PinnedPool,
        UniqueIdResetPool,
        Animator
    }
}

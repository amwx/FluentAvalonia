using Avalonia.Controls;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public class ContainerContentChangingEventArgs : EventArgs
{
    internal ContainerContentChangingEventArgs(int index, object item,
        Control container, VirtualizationInfo virtInfo)
    {
        ItemIndex = index;
        Item = item;
        ItemContainer = container;
        _virtInfo = virtInfo;
    }

    internal ContainerContentChangingEventArgs(int index, object item,
        Control container, VirtualizationInfo virtInfo, int phase)
    {
        ItemIndex = index;
        Item = item;
        ItemContainer = container;
        _virtInfo = virtInfo;
        Phase = phase;
    }

    //public bool Handled { get; set; }

    //public bool InRecycleQueue { get; internal set; }

    public object Item { get; internal set; }

    public Control ItemContainer { get; internal set; }

    public int ItemIndex { get; internal set; }

    public int Phase { get; private set; }

    public void RegisterUpdateCallback(
        TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> callback)
    {
        _virtInfo.UpdatePhasingInfo(Phase + 1, Item, callback);
    }

    private VirtualizationInfo _virtInfo;
}

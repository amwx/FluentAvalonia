using Avalonia.Controls;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FAItemsRepeater.ContainerContentChanging"/> event.
/// </summary>
public class FAContainerContentChangingEventArgs : EventArgs
{
    internal FAContainerContentChangingEventArgs(int index, object item,
        Control container, VirtualizationInfo virtInfo)
    {
        ItemIndex = index;
        Item = item;
        ItemContainer = container;
        _virtInfo = virtInfo;
    }

    internal FAContainerContentChangingEventArgs(int index, object item,
        Control container, VirtualizationInfo virtInfo, int phase, Phaser phaser)
    {
        ItemIndex = index;
        Item = item;
        ItemContainer = container;
        _virtInfo = virtInfo;
        Phase = phase;
        _phaser = phaser;
    }

    //public bool Handled { get; set; }

    //public bool InRecycleQueue { get; internal set; }

    /// <summary>
    /// Gets the data item associated with this container.
    /// </summary>
    public object Item { get; internal set; }

    /// <summary>
    /// Gets the UI container used to display the current data item.
    /// </summary>
    public Control ItemContainer { get; internal set; }

    /// <summary>
    /// Gets the index in the ItemsSource of the data item associated with this container.
    /// </summary>
    public int ItemIndex { get; internal set; }

    // TODO: Remove this, we no longer use it
    /// <summary>
    /// 
    /// </summary>
    public int Phase { get; private set; }

    public void RegisterUpdateCallback(
        TypedEventHandler<FAItemsRepeater, FAContainerContentChangingEventArgs> callback)
    {
        _phaser.PhaseElement(ItemContainer, _virtInfo, new FAContainerContentChangingEventArgs(
            ItemIndex, Item, ItemContainer, _virtInfo, Phase + 1, _phaser)
        { callback = callback });
    }

    internal TypedEventHandler<FAItemsRepeater, FAContainerContentChangingEventArgs> callback;

    private VirtualizationInfo _virtInfo;
    private readonly Phaser _phaser;
}

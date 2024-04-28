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

    public object Item { get; internal set; }

    public Control ItemContainer { get; internal set; }

    public int ItemIndex { get; internal set; }

    public int Phase { get; private set; }

    public void RegisterUpdateCallback(
        TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> callback)
    {
        //_virtInfo.UpdatePhasingInfo(Phase + 1, Item, callback);
        //_callbackInfo = new UpdateCallbackInfo(callback, Phase + 1);
        //nextPhase = Phase + 1;
        //this.callback = callback;

        _phaser.PhaseElement(ItemContainer, _virtInfo, new ContainerContentChangingEventArgs(
            ItemIndex, Item, ItemContainer, _virtInfo, Phase + 1, _phaser)
        { callback = callback });

        //BuildTreeScheduler.RegisterWork(Phase + 1, () => callback.Invoke(null, 
        //    new ContainerContentChangingEventArgs(ItemIndex, Item, ItemContainer, _virtInfo, Phase + 1)));
    }
    private int nextPhase;
    public TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> callback;

    internal bool GetCallbackInfo(out UpdateCallbackInfo info)
    {
        info = _callbackInfo;

        return info.NextPhase != Phase;
    }

    private VirtualizationInfo _virtInfo;
    private UpdateCallbackInfo _callbackInfo;
    private readonly Phaser _phaser;

    internal readonly struct UpdateCallbackInfo
    {
        public UpdateCallbackInfo(TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> callback, int phase)
        {
            NextCallback = callback;
            NextPhase = phase;
        }

        public TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> NextCallback { get; }

        public int NextPhase { get; }
    }
}

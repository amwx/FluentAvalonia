namespace FluentAvalonia.Core;
public class WeakTypedEventHandler<TSender, TEventArgs> where TEventArgs : EventArgs
{
    private readonly WeakReference<TypedEventHandler<TSender, TEventArgs>> _weakHandler;

    public WeakTypedEventHandler(TypedEventHandler<TSender, TEventArgs> handler)
    {
        _weakHandler = new WeakReference<TypedEventHandler<TSender, TEventArgs>>(handler);
    }

    public void Handler(TSender sender, TEventArgs e)
    {
        if (_weakHandler.TryGetTarget(out var handler))
            handler(sender, e);
    }

    public static implicit operator TypedEventHandler<TSender, TEventArgs>(WeakTypedEventHandler<TSender, TEventArgs> weh)
    {
        return weh.Handler;
    }
}


using Avalonia.Controls;

namespace FluentAvalonia.Core
{
    /// <summary>
    /// EventHandler delegate with a explicit Type
    /// </summary>
    public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);

    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args);
}

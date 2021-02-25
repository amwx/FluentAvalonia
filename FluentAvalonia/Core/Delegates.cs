//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia.Controls;

namespace FluentAvalonia.Core
{
    /// <summary>
    /// EventHandler delegate with a explicit Type
    /// </summary>
    public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);

    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args);
}

using Avalonia.Controls;

namespace FluentAvalonia.Core;

/// <summary>
/// EventHandler delegate with a explicit Type
/// </summary>
public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);

/// <summary>
/// Event handler for selection changed events
/// </summary>
public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args);

using System;

namespace FluentAvalonia.Styling;

/// <summary>
/// Data for the <see cref="FluentAvaloniaTheme.RequestedThemeChanged"/> event
/// </summary>
public class RequestedThemeChangedEventArgs : EventArgs
{
    internal RequestedThemeChangedEventArgs(string theme)
    {
        NewTheme = theme;
    }

    /// <summary>
    /// The name of the new theme (Light, Dark, or HighContrast)
    /// </summary>
    public string NewTheme { get; }
}

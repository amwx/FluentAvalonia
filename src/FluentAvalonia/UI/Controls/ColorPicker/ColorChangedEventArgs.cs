using FluentAvalonia.UI.Media;
using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Data for the <see cref="FAColorPicker.ColorChanged"/> event
/// </summary>
public sealed class ColorChangedEventArgs : EventArgs
{
    internal ColorChangedEventArgs(Color2 oldC, Color2 newC)
    {
        OldColor = oldC;
        NewColor = newC;
    }

    /// <summary>
    /// The old Color
    /// </summary>
    public Color2 OldColor { get; }

    /// <summary>
    /// The new Color
    /// </summary>
    public Color2 NewColor { get; }
}

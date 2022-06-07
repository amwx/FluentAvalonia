using System;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls;

public class ColorButtonColorChangedEventArgs : EventArgs
{
    public Color? OldColor { get; }

    public Color? NewColor { get; }

    internal ColorButtonColorChangedEventArgs(Color? oldC, Color? newC)
    {
        OldColor = oldC;
        NewColor = newC;
    }
}

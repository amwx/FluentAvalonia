using Avalonia;

namespace FluentAvalonia.UI.Controls;

public abstract class LayoutContext : AvaloniaObject
{
    public object LayoutState
    {
        get => LayoutStateCore;
        set => LayoutStateCore = value;
    }

    protected internal virtual object LayoutStateCore { get; set; }

#if DEBUG
    internal int Indent { get; set; }
#endif
}

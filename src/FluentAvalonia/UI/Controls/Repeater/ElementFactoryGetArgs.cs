using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class ElementFactoryGetArgs
{
    public object Data { get; set; }

    public Control Parent { get; set; }

    internal int Index { get; set; }
}

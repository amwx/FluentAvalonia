using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class ElementFactoryRecycleArgs
{
    public Control Element { get; set; }

    public Control Parent { get; set; }
}
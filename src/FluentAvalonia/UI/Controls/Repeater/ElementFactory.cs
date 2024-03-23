using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

public abstract class ElementFactory : IElementFactory
{
    public Control GetElement(ElementFactoryGetArgs args) =>
        GetElementCore(args);

    public void RecycleElement(ElementFactoryRecycleArgs args) =>
        RecycleElementCore(args);

    protected abstract Control GetElementCore(ElementFactoryGetArgs args);

    protected abstract void RecycleElementCore(ElementFactoryRecycleArgs args);

    Control ITemplate<object, Control>.Build(object param) => null;

    bool IDataTemplate.Match(object data) => false;
}

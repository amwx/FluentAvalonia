using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a factory used for creating or recycling items for the ItemsRepeater
/// </summary>
public abstract class ElementFactory : IElementFactory
{
    /// <summary>
    /// Gets an element
    /// </summary>
    public Control GetElement(ElementFactoryGetArgs args) =>
        GetElementCore(args);

    /// <summary>
    /// Recycles a specified element
    /// </summary>
    public void RecycleElement(ElementFactoryRecycleArgs args) =>
        RecycleElementCore(args);

    /// <summary>
    /// Gets an element
    /// </summary>
    protected abstract Control GetElementCore(ElementFactoryGetArgs args);

    /// <summary>
    /// Recycles a specified element
    /// </summary>
    protected abstract void RecycleElementCore(ElementFactoryRecycleArgs args);

    Control ITemplate<object, Control>.Build(object param) => null;

    bool IDataTemplate.Match(object data) => false;
}

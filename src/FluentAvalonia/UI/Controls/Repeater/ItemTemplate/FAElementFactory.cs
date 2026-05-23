using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a factory used for creating or recycling items for the ItemsRepeater
/// </summary>
public abstract class FAElementFactory : IFAElementFactory
{
    /// <summary>
    /// Gets an element
    /// </summary>
    public Control GetElement(FAElementFactoryGetArgs args) =>
        GetElementCore(args);

    /// <summary>
    /// Recycles a specified element
    /// </summary>
    public void RecycleElement(FAElementFactoryRecycleArgs args) =>
        RecycleElementCore(args);

    /// <summary>
    /// Gets an element
    /// </summary>
    protected abstract Control GetElementCore(FAElementFactoryGetArgs args);

    /// <summary>
    /// Recycles a specified element
    /// </summary>
    protected abstract void RecycleElementCore(FAElementFactoryRecycleArgs args);

    Control ITemplate<object, Control>.Build(object param) => null;

    bool IDataTemplate.Match(object data) => false;
}

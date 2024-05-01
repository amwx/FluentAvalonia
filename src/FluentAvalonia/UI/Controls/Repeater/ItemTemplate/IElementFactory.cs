using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Supports the creation and recycling of <see cref="Control"/> objects
/// </summary>
public interface IElementFactory : IDataTemplate
{
    /// <summary>
    /// Gets a <see cref="Control"/> object
    /// </summary>
    Control GetElement(ElementFactoryGetArgs args);

    /// <summary>
    /// Recycles a <see cref="Control"/> that was previously retreived using <see cref="GetElement(ElementFactoryGetArgs)"/>
    /// </summary>
    void RecycleElement(ElementFactoryRecycleArgs args);
}

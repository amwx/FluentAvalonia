using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Supports the creation and recycling of <see cref="Control"/> objects
/// </summary>
public interface IFAElementFactory : IDataTemplate
{
    /// <summary>
    /// Gets a <see cref="Control"/> object
    /// </summary>
    Control GetElement(FAElementFactoryGetArgs args);

    /// <summary>
    /// Recycles a <see cref="Control"/> that was previously retreived using <see cref="GetElement(FAElementFactoryGetArgs)"/>
    /// </summary>
    void RecycleElement(FAElementFactoryRecycleArgs args);
}

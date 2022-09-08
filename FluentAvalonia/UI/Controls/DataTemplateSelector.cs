using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Enables custom template selection logic at the application level.
/// </summary>
/// <remarks>
/// This class should generally not be used and you should use typical Avalonia way of
/// managing data-specific templates. This was added for the NavigationView to ensure
/// recycling of templates works correctly and this class may be removed in a future release
/// </remarks>
public class DataTemplateSelector
{
    /// <summary>
    /// Returns a specific DataTemplate for a given item.
    /// </summary>
    public IDataTemplate SelectTemplate(object item) => SelectTemplateCore(item);

    /// <summary>
    /// Returns a specific DataTemplate for a given item and container.
    /// </summary>
    public IDataTemplate SelectTemplate(object item, IControl container) => SelectTemplateCore(item, container);

    /// <summary>
    /// When implemented by a derived class, returns a specific DataTemplate for a given item or container.
    /// </summary>
    protected virtual IDataTemplate SelectTemplateCore(object item) => null;

    /// <summary>
    /// When implemented by a derived class, returns a specific DataTemplate for a given item or container.
    /// </summary>
    protected virtual IDataTemplate SelectTemplateCore(object item, IControl container) => null;
}

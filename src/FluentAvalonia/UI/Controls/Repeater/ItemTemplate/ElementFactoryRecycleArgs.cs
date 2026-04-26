using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the optional arguments to use when calling an implementation of the
/// <see cref="IElementFactory"/>'s <see cref="IElementFactory.RecycleElement(ElementFactoryRecycleArgs)"/>
/// </summary>
public class ElementFactoryRecycleArgs
{
    /// <summary>
    /// Gets or sets the <see cref="Control"/> object to recycle when calling
    /// <see cref="IElementFactory.RecycleElement(ElementFactoryRecycleArgs)"/>
    /// </summary>
    public Control Element { get; set; }

    /// <summary>
    /// Gets or sets a reference to the current parent <see cref="Control"/> of the element being recycled
    /// </summary>
    public Control Parent { get; set; }
}

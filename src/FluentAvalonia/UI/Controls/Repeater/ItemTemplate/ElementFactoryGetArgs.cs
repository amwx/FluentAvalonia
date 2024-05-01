using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the optional arguments to use when calling an implementation of the 
/// <see cref="IElementFactory"/>'s <see cref="IElementFactory.GetElement(ElementFactoryGetArgs)"/>
/// </summary>
public class ElementFactoryGetArgs
{
    /// <summary>
    /// Gets or sets teh data item for which an appropriate element tree should be realized when calling 
    /// <see cref="IElementFactory.GetElement(ElementFactoryGetArgs)"/>
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Control"/> that is expected to be the parent of the realized element from
    /// <see cref="IElementFactory.GetElement(ElementFactoryGetArgs)"/>
    /// </summary>
    public Control Parent { get; set; }

    internal int Index { get; set; }
}

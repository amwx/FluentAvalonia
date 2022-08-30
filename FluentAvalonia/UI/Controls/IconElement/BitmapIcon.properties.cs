using Avalonia;
using System;

namespace FluentAvalonia.UI.Controls;

public partial class BitmapIcon : FAIconElement
{
    /// <summary>
    /// Defines the <see cref="UriSource"/> property
    /// </summary>
    public static readonly StyledProperty<Uri> UriSourceProperty =
        AvaloniaProperty.Register<BitmapIcon, Uri>(nameof(UriSource));

    /// <summary>
    /// Defines the <see cref="ShowAsMonochrome"/> property
    /// </summary>
    public static readonly StyledProperty<bool> ShowAsMonochromeProperty =
        AvaloniaProperty.Register<BitmapIcon, bool>(nameof(ShowAsMonochrome));

    /// <summary>
    /// Gets or sets the Uniform Resource Identifier (URI) of the bitmap to use as the icon content.
    /// </summary>
    public Uri UriSource
    {
        get => GetValue(UriSourceProperty);
        set => SetValue(UriSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the bitmap is shown in a single color.
    /// </summary>
    public bool ShowAsMonochrome
    {
        get => GetValue(ShowAsMonochromeProperty);
        set => SetValue(ShowAsMonochromeProperty, value);
    }
}

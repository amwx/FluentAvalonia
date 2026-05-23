using Avalonia;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Provides settings used in the template of a <see cref="FANavigationViewItemPresenter"/>
/// </summary>
public class FANavigationViewItemPresenterTemplateSettings : AvaloniaObject
{
    internal FANavigationViewItemPresenterTemplateSettings() { }

    /// <summary>
    /// Defines the <see cref="IconWidth"/> property
    /// </summary>
    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<FANavigationViewItemPresenterTemplateSettings, double>(nameof(IconWidth));

    /// <summary>
    /// Defines the <see cref="SmallerIconWidth"/> property
    /// </summary>
    public static readonly StyledProperty<double> SmallerIconWidthProperty =
        AvaloniaProperty.Register<FANavigationViewItemPresenterTemplateSettings, double>(nameof(SmallerIconWidth));

    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconProperty =
        FAMenuFlyoutItemTemplateSettings.IconProperty.AddOwner<FANavigationViewItemPresenterTemplateSettings>();

    /// <summary>
    /// TODO: Get docs from MS - relatively new setting
    /// </summary>
    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        internal set => SetValue(IconWidthProperty, value);
    }

    /// <summary>
    /// TODO: Get docs from MS - relatively new setting
    /// </summary>
    public double SmallerIconWidth
    {
        get => GetValue(SmallerIconWidthProperty);
        internal set => SetValue(SmallerIconWidthProperty, value);
    }

    /// <summary>
    /// Gets the <see cref="FAIconElement"/> used in the NavigationViewItem
    /// </summary>
    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        internal set => SetValue(IconProperty, value);
    }
}

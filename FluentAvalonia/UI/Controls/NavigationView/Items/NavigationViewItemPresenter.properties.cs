using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls.Primitives;

public partial class NavigationViewItemPresenter
{
    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<NavigationViewItemPresenter, FAIconElement>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<NavigationViewItemPresenterTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<NavigationViewItemPresenter, NavigationViewItemPresenterTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Defines the <see cref="InfoBadge"/> property
    /// </summary>
    public static readonly StyledProperty<InfoBadge> InfoBadgeProperty =
        NavigationViewItem.InfoBadgeProperty.AddOwner<NavigationViewItemPresenter>();

    /// <summary>
    /// Gets or sets the icon in a NavigationView item.
    /// </summary>
    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets the template settings used in the NavigationViewItemPresenter
    /// </summary>
    public NavigationViewItemPresenterTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        internal set => SetValue(TemplateSettingsProperty, value);
    }

    /// <summary>
    /// Gets or sets the InfoBadge used in the NavigationViewItemPresenter
    /// </summary>
    public InfoBadge InfoBadge
    {
        get => GetValue(InfoBadgeProperty);
        set => SetValue(InfoBadgeProperty, value);
    }

    internal NavigationViewItem GetNVI
    {
        get
        {
            return this.FindAncestorOfType<NavigationViewItem>();
        }
    }

    internal IControl SelectionIndicator => _selectionIndicator;
}

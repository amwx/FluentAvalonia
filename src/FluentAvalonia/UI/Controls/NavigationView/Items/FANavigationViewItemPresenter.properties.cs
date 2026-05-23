using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.VisualTree;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls.Primitives;

[PseudoClasses(s_pcExpanded)]
[PseudoClasses(s_pcClosedCompactTop, s_pcNotClosedCompactTop)]
[PseudoClasses(FASharedPseudoclasses.s_pcLeftNav, FASharedPseudoclasses.s_pcTopNav, FASharedPseudoclasses.s_pcTopOverflow)]
[PseudoClasses(FASharedPseudoclasses.s_pcChevronOpen, FASharedPseudoclasses.s_pcChevronClosed, FASharedPseudoclasses.s_pcChevronHidden)]
[PseudoClasses(FASharedPseudoclasses.s_pcIconLeft, FASharedPseudoclasses.s_pcIconOnly, FASharedPseudoclasses.s_pcContentOnly)]
[PseudoClasses(FASharedPseudoclasses.s_pcPressed)]
public partial class FANavigationViewItemPresenter
{
    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<FANavigationViewItemPresenter>();

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<FANavigationViewItemPresenterTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<FANavigationViewItemPresenter, FANavigationViewItemPresenterTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Defines the <see cref="InfoBadge"/> property
    /// </summary>
    public static readonly StyledProperty<FAInfoBadge> InfoBadgeProperty =
        FANavigationViewItem.InfoBadgeProperty.AddOwner<FANavigationViewItemPresenter>();

    /// <summary>
    /// Gets or sets the icon in a NavigationView item.
    /// </summary>
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets the template settings used in the NavigationViewItemPresenter
    /// </summary>
    public FANavigationViewItemPresenterTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        internal set => SetValue(TemplateSettingsProperty, value);
    }

    /// <summary>
    /// Gets or sets the InfoBadge used in the NavigationViewItemPresenter
    /// </summary>
    public FAInfoBadge InfoBadge
    {
        get => GetValue(InfoBadgeProperty);
        set => SetValue(InfoBadgeProperty, value);
    }

    internal FANavigationViewItem GetNVI
    {
        get
        {
            return this.FindAncestorOfType<FANavigationViewItem>();
        }
    }

    internal Control SelectionIndicator => _selectionIndicator;

    private const string s_tpSelectionIndicator = "SelectionIndicator";
    private const string s_tpPresenterContentRootGrid = "PresenterContentRootGrid";
    private const string s_tpInfoBadgePresenter = "InfoBadgePresenter";
    private const string s_tpExpandCollapseChevron = "ExpandCollapseChevron";

    private const string s_pcClosedCompactTop = ":closedcompacttop";
    private const string s_pcNotClosedCompactTop = ":notclosedcompacttop";
    private const string s_pcExpanded = ":expanded";
}

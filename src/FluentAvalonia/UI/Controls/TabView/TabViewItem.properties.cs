using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia;
using FluentAvalonia.Core;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(FASharedPseudoclasses.s_pcIcon, FASharedPseudoclasses.s_pcCompact, s_pcCloseCollapsed, s_pcForeground)]
[PseudoClasses(FASharedPseudoclasses.s_pcBorderRight, FASharedPseudoclasses.s_pcBorderLeft, FASharedPseudoclasses.s_pcNoBorder)]
[TemplatePart(s_tpTabSeparator, typeof(Visual))]
[TemplatePart(s_tpContentPresenter, typeof(ContentPresenter))]
[TemplatePart(s_tpCloseButton, typeof(Button))]
[TemplatePart(s_tpSelectedBackgroundPathName, typeof(Avalonia.Controls.Shapes.Path))]
public partial class TabViewItem
{
    /// <summary>
    /// Defines the <see cref="Header"/> property
    /// </summary>
    public static readonly StyledProperty<object> HeaderProperty =
        HeaderedContentControl.HeaderProperty.AddOwner<TabViewItem>();

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
        HeaderedContentControl.HeaderTemplateProperty.AddOwner<TabViewItem>();

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<TabViewItem>();

    /// <summary>
    /// Defines the <see cref="IsClosable"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<TabViewItem, bool>(nameof(IsClosable), defaultValue: true);

    /// <summary>
    /// Defines the <see cref="TabViewTemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<TabViewItemTemplateSettings> TabViewTemplateSettingsProperty =
        AvaloniaProperty.Register<TabViewItem, TabViewItemTemplateSettings>(nameof(TabViewTemplateSettings));

    /// <summary>
    /// Gets or sets the content that appears inside the tabstrip to represent the tab
    /// </summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the IDataTemplate used to display the <see cref="Header"/> content
    /// </summary>
    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value for the IconSource to be displayed within the tab
    /// </summary>
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that determines if the tab shows a close button (default is true)
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Gets an object that provides calculated values that can be referenced as {TemplateBinding}
    /// markup extension sources when definign templates for a TabViewItem control
    /// </summary>
    public TabViewItemTemplateSettings TabViewTemplateSettings
    {
        get => GetValue(TabViewTemplateSettingsProperty);
        private set => SetValue(TabViewTemplateSettingsProperty, value);
    }

    /// <summary>
    /// Raised when the user attempts to close the TabViewItem via clicking the x-to-close
    /// button
    /// </summary>
    public event TypedEventHandler<TabViewItem, TabViewTabCloseRequestedEventArgs> CloseRequested;

    internal bool IsContainerFromTemplate { get; set; }


    private const string s_pcCloseCollapsed = ":closeCollapsed";
    private const string s_pcDragging = ":dragging";

    private const string s_tpSelectedBackgroundPathName = "SelectedBackgroundPath";
    private const string s_tpTabSeparator = "TabSeparator";
    private const string s_tpContentPresenter = "ContentPresenter";
    internal const string s_tpCloseButton = "CloseButton";
}

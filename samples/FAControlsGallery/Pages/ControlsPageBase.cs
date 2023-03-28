using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FAControlsGallery.Controls;
using FAControlsGallery.Services;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public class ControlsPageBase : UserControl, IStyleable
{
    public ControlsPageBase()
    {        
        SizeChanged += ControlsPageBaseSizeChanged;
    }

    public static readonly StyledProperty<string> ControlNameProperty =
        AvaloniaProperty.Register<ControlsPageBase, string>(nameof(ControlName));

    public static readonly StyledProperty<string> ControlNamespaceProperty =
        AvaloniaProperty.Register<ControlsPageBase, string>(nameof(ControlNamespace));

    public static readonly StyledProperty<Type> TargetTypeProperty =
        ControlExample.TargetTypeProperty.AddOwner<ControlsPageBase>();

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<ControlsPageBase, string>(nameof(Description));

    public static readonly StyledProperty<IconSource> PreviewImageProperty =
        AvaloniaProperty.Register<ControlsPageBase, IconSource>(nameof(PreviewImage));

    public static readonly StyledProperty<string> WinUINamespaceProperty =
        AvaloniaProperty.Register<ControlsPageBase, string>(nameof(WinUINamespace));

    public static readonly StyledProperty<Uri> WinUIDocsLinkProperty =
        AvaloniaProperty.Register<ControlsPageBase, Uri>(nameof(WinUIDocsLink));

    public static readonly StyledProperty<Uri> WinUIGuidelinesLinkProperty =
        AvaloniaProperty.Register<ControlsPageBase, Uri>(nameof(WinUIGuidelinesLink));

    public static readonly StyledProperty<Uri> PageXamlSourceLinkProperty =
        AvaloniaProperty.Register<ControlsPageBase, Uri>(nameof(PageXamlSourceLink));

    public static readonly StyledProperty<Uri> PageCSharpSourceLinkProperty =
        AvaloniaProperty.Register<ControlsPageBase, Uri>(nameof(PageCSharpSourceLink));

    public string ControlName
    {
        get => GetValue(ControlNameProperty);
        set => SetValue(ControlNameProperty, value);
    }

    public string ControlNamespace
    {
        get => GetValue(ControlNamespaceProperty);
        set => SetValue(ControlNamespaceProperty, value);
    }

    public Type TargetType
    {
        get => GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconSource PreviewImage
    {
        get => GetValue(PreviewImageProperty);
        set => SetValue(PreviewImageProperty, value);
    }

    public string WinUINamespace
    {
        get => GetValue(WinUINamespaceProperty);
        set => SetValue(WinUINamespaceProperty, value);
    }

    public Uri WinUIDocsLink
    {
        get => GetValue(WinUIDocsLinkProperty);
        set => SetValue(WinUIDocsLinkProperty, value);
    }

    public Uri WinUIGuidelinesLink
    {
        get => GetValue(WinUIGuidelinesLinkProperty);
        set => SetValue(WinUIGuidelinesLinkProperty, value);
    }

    public Uri PageXamlSourceLink
    {
        get => GetValue(PageXamlSourceLinkProperty);
        set => SetValue(PageXamlSourceLinkProperty, value);
    }

    public Uri PageCSharpSourceLink
    {
        get => GetValue(PageCSharpSourceLinkProperty);
        set => SetValue(PageCSharpSourceLinkProperty, value);
    }

    public string GithubPrefixString
    {
        set
        {
            PageXamlSourceLink = new Uri($"{value}/{GetType().Name}.axaml");
            PageCSharpSourceLink = new Uri($"{value}/{GetType().Name}.axaml.cs");
        }
    }

    Type IStyleable.StyleKey => typeof(ControlsPageBase);

    protected ThemeVariantScope ThemeScopeProvider { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        PseudoClasses.Set(":namespace", ControlNamespace != null);
        PseudoClasses.Set(":winuiNamespace", WinUINamespace != null);

        ThemeScopeProvider = e.NameScope.Find<ThemeVariantScope>("ThemeScopeProvider");

        _optionsHost = e.NameScope.Find<StackPanel>("OptionsRegion");
        _detailsPanel = e.NameScope.Find<Panel>("PageDetails");

        _toggleThemeButton = e.NameScope.Find<Button>("ToggleThemeButton");
        _toggleThemeButton.Click += ToggleThemeButtonClick;

        _winUIDocsItem = e.NameScope.Find<MenuFlyoutItem>("WinUIDocsItem");
        _winUIGuidelinesItem = e.NameScope.Find<MenuFlyoutItem>("WinUIGuidelinesItem");
        _xamlSourceItem = e.NameScope.Find<MenuFlyoutItem>("XamlSourceItem");
        _cSharpSourceItem = e.NameScope.Find<MenuFlyoutItem>("CSharpSourceItem");
        _showDefItem = e.NameScope.Find<MenuFlyoutItem>("ShowDefItem");
        _sep1 = e.NameScope.Find<MenuFlyoutSeparator>("Sep1");
        _sep2 = e.NameScope.Find<MenuFlyoutSeparator>("Sep2");

        var winUIDocs = WinUIDocsLink;
        var winUIGuidelines = WinUIGuidelinesLink;
        var type = TargetType;

        if (winUIDocs == null)
            _winUIDocsItem.IsVisible = false;
        else
            _winUIDocsItem.Click += MoreOptionsItemClick;

        if (winUIGuidelines == null)
            _winUIGuidelinesItem.IsVisible = false;
        else
            _winUIGuidelinesItem.Click += MoreOptionsItemClick;

        if (type == null)
            _showDefItem.IsVisible = false;
        else
            _showDefItem.Click += MoreOptionsItemClick;

        _xamlSourceItem.Click += MoreOptionsItemClick;
        _cSharpSourceItem.Click += MoreOptionsItemClick;

        _sep1.IsVisible = _winUIDocsItem.IsVisible && _winUIGuidelinesItem.IsVisible;
        _sep2.IsVisible = _showDefItem.IsVisible;
    }

    private void MoreOptionsItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            switch (mfi.Name)
            {
                case "WinUIDocsItem":
                    LaunchLink(WinUIDocsLink);
                    break;

                case "WinUIGuidelinesItem":
                    LaunchLink(WinUIGuidelinesLink);
                    break;

                case "XamlSourceItem":
                    LaunchLink(PageXamlSourceLink);
                    break;

                case "CSharpSourceItem":
                    LaunchLink(PageCSharpSourceLink);
                    break;

                case "ShowDefItem":
                    NavigationService.Instance.ShowControlDefinitionOverlay(TargetType);
                    break;
            }
        }
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        _hasLoaded = true;
        SetDetailsAnimation();
    }

    protected override void OnUnloaded()
    {
        base.OnUnloaded();
        _hasLoaded = false;
    }

    private void ControlsPageBaseSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var sz = e.NewSize.Width;

        bool isSmallWidth2 = sz < 580;

        PseudoClasses.Set(":smallWidth", sz < 710);
        PseudoClasses.Set(":smallWidth2", isSmallWidth2);

        if (isSmallWidth2 && !_isSmallWidth2)
        {
            AnimateOptions(true);
            _isSmallWidth2 = true;
        }
        else if (!isSmallWidth2 && _isSmallWidth2)
        {
            AnimateOptions(false);
            _isSmallWidth2 = false;
        }
    }

    private async void AnimateOptions(bool toSmall)
    {
        if (!_hasLoaded)
            return;

        _cts?.Cancel();

        _cts = new CancellationTokenSource();
        double x = toSmall ? 70 : -70;
        double y = toSmall ? -30 : 30;
        var ani = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.25),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, x),
                        new Setter(TranslateTransform.YProperty, y),
                        new Setter(OpacityProperty, 0d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, 0d),
                        new Setter(TranslateTransform.YProperty, 0d),
                        new Setter(OpacityProperty, 1d)
                    },
                    KeySpline = new KeySpline(0, 0, 0, 1)
                }
            }
        };

        await ani.RunAsync(_optionsHost, null, _cts.Token);

        _cts = null;
    }

    private void SetDetailsAnimation()
    {
        var ec = ElementComposition.GetElementVisual(_detailsPanel);
        var compositor = ec.Compositor;
               
        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

        var ani = compositor.CreateImplicitAnimationCollection();
        ani["Offset"] = offsetAnimation;

        ec.ImplicitAnimations = ani;
    }

    protected virtual void ToggleThemeButtonClick(object sender, RoutedEventArgs e)
    {
        var examples = this.GetVisualDescendants()
            .OfType<ControlExample>();

        foreach (var ex in examples)
        {
            ex.SetExampleTheme();
        }
    }

    private async void LaunchLink(Uri link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link.ToString()) { UseShellExecute = true, Verb = "open" });
        }
        catch
        {
            await DialogHelper.ShowUnableToOpenLinkDialog(link);
        }
    }

    private bool _isSmallWidth2;
    private CancellationTokenSource _cts;
    private bool _hasLoaded;

    private Button _toggleThemeButton;
    private Panel _detailsPanel;
    private StackPanel _optionsHost;

    private MenuFlyoutItem _winUIDocsItem;
    private MenuFlyoutItem _winUIGuidelinesItem;
    private MenuFlyoutItem _xamlSourceItem;
    private MenuFlyoutItem _cSharpSourceItem;
    private MenuFlyoutItem _showDefItem;
    private MenuFlyoutSeparator _sep1, _sep2;
}

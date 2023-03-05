using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using FluentAvalonia.UI.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System.Diagnostics;
using FAControlsGallery.Services;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace FAControlsGallery.Controls;

public class ControlExample : HeaderedContentControl
{
    public ControlExample()
    {
        _substitutions = new List<ControlExampleSubstitution>();

        PseudoClasses.Add(":optionsfull");
    }

    public static readonly StyledProperty<Type> TargetTypeProperty =
        AvaloniaProperty.Register<ControlExample, Type>(nameof(TargetType));

    public static readonly StyledProperty<string> XamlSourceProperty =
        AvaloniaProperty.Register<ControlExample, string>(nameof(XamlSource));

    public static readonly StyledProperty<string> CSharpSourceProperty =
        AvaloniaProperty.Register<ControlExample, string>(nameof(CSharpSource));

    public static readonly StyledProperty<string> UsageNotesProperty =
        AvaloniaProperty.Register<ControlExample, string>(nameof(UsageNotes));

    public static readonly StyledProperty<Control> OptionsProperty =
        AvaloniaProperty.Register<ControlExample, Control>(nameof(Options));

    public static readonly DirectProperty<ControlExample, IList<ControlExampleSubstitution>> SubstitutionsProperty =
        AvaloniaProperty.RegisterDirect<ControlExample, IList<ControlExampleSubstitution>>(nameof(Substitutions),
            x => x.Substitutions);

    public static readonly StyledProperty<bool> IsOptionsExpandedProperty =
        AvaloniaProperty.Register<ControlExample, bool>(nameof(IsOptionsExpanded), true);

    public Type TargetType
    {
        get => GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }

    public string XamlSource
    {
        get => GetValue(XamlSourceProperty);
        set => SetValue(XamlSourceProperty, value);
    }

    public string CSharpSource
    {
        get => GetValue(CSharpSourceProperty);
        set => SetValue(CSharpSourceProperty, value);
    }

    public Control Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public string UsageNotes
    {
        get => GetValue(UsageNotesProperty);
        set => SetValue(UsageNotesProperty, value);
    }

    public IList<ControlExampleSubstitution> Substitutions
    {
        get => _substitutions;
    }

    public bool IsOptionsExpanded
    {
        get => GetValue(IsOptionsExpandedProperty);
        set => SetValue(IsOptionsExpandedProperty, value);
    }

    public bool EnableShowDocsLink { get; set; }

    public bool EnableShowDefinitionLink { get; set; }

    public string DocsLinkHeader { get; set; } = "Avalonia Docs";

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_expandOptionsButton != null)
            _expandOptionsButton.Click -= OnExpandOptionsClick;

        base.OnApplyTemplate(e);

        _exampleThemeScopeProvider = e.NameScope.Find<ThemeVariantScope>("ThemeScopeProvider");

        _expandOptionsButton = e.NameScope.Find<Button>("ShowHideOptionsButton");
        _expandOptionsButton.Click += OnExpandOptionsClick;

        _previewAreaHost = e.NameScope.Find<Border>("ControlPreviewAreaHost");

        _moreButton = e.NameScope.Find<Button>("MoreButton");
        if (_moreButton != null)
        {
            BuildMoreButtonMenu();
        }        

        bool hasXaml = XamlSource != null;
        bool hasCSharp = CSharpSource != null;
        bool hasNotes = UsageNotes != null;
        PseudoClasses.Set(":codepreview", hasXaml || hasCSharp || hasNotes);

        PseudoClasses.Set(":xamlsource", hasXaml);
        PseudoClasses.Set(":csharpsource", hasCSharp);
        PseudoClasses.Set(":usagenotes", hasNotes);
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        // Do this here rather than OnApplyTemplate, otherwise this will animate
        // on load and that isn't desired
        AttachOptionsHostAnimation();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OptionsProperty)
        {
            PseudoClasses.Set(":options", change.NewValue != null);
        }
        else if (change.Property == BoundsProperty)
        {
            var wid = change.GetNewValue<Rect>().Width;

            PseudoClasses.Set(":mediumWidth", wid < 725);
            PseudoClasses.Set(":smallWidth", wid < 500);
        }
        else if (change.Property == IsOptionsExpandedProperty)
        {
            PseudoClasses.Set(":optionsfull", change.GetNewValue<bool>());
        }
    }

    private void OnExpandOptionsClick(object sender, RoutedEventArgs e)
    {
        IsOptionsExpanded = !IsOptionsExpanded;
    }

    private void BuildMoreButtonMenu()
    {
        if (!EnableShowDefinitionLink && !EnableShowDocsLink)
        {
            _moreButton.IsVisible = false;
        }
        else
        {
            var l = new List<MenuFlyoutItem>();

            if (EnableShowDocsLink)
            {
                var docsItem = new MenuFlyoutItem
                {
                    Text = DocsLinkHeader,
                    IconSource = new SymbolIconSource { Symbol = Symbol.Link }
                };

                docsItem.Click += LaunchAvaloniaDocs;

                l.Add(docsItem);
            }

            if (EnableShowDefinitionLink)
            {
                var defItem = new MenuFlyoutItem
                {
                    Text = "Show Definition",
                    IconSource = new SymbolIconSource { Symbol = Symbol.CodeFilled }
                };

                defItem.Click += ShowControlDefintion;

                l.Add(defItem);
            }

            _moreButton.Flyout = new FAMenuFlyout
            {
                Items = l,
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
            };
        }
    }

    private void AttachOptionsHostAnimation()
    {
        if (_optionsHost == null)
            return;

        var host = _optionsHost;
        var ec = ElementComposition.GetElementVisual(host);

        if (_optionsHostAnimation == null)
        {
            var comp = ec.Compositor;

            var offsetAni = comp.CreateVector3KeyFrameAnimation();
            offsetAni.InsertExpressionKeyFrame(1f, "this.FinalValue");
            offsetAni.Target = "Offset";
            offsetAni.Duration = TimeSpan.FromMilliseconds(250);

            var scaleAni = comp.CreateVector3KeyFrameAnimation();
            scaleAni.InsertExpressionKeyFrame(1f, "this.FinalValue");
            scaleAni.Target = "Scale";
            scaleAni.Duration = offsetAni.Duration;

            var group = comp.CreateAnimationGroup();
            group.Add(offsetAni);
            group.Add(scaleAni);

            _optionsHostAnimation = comp.CreateImplicitAnimationCollection();
            _optionsHostAnimation["Offset"] = group;
        }

        ec.ImplicitAnimations = _optionsHostAnimation;
    }

    public void SetExampleTheme()
    {
        var theme = _exampleThemeScopeProvider.ActualThemeVariant;

        if (theme == ThemeVariant.Light)
        {
            _exampleThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Dark;
            // Hack force resource invalidation as that doesn't seem to want to happen
            // the first time toggle theme is set
            NotifyChildResourcesChanged(ResourcesChangedEventArgs.Empty);
        }
        else
        {
            _exampleThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Light;
            NotifyChildResourcesChanged(ResourcesChangedEventArgs.Empty);
        }
    }

    public async void LaunchAvaloniaDocs(object sender, RoutedEventArgs e)
    {
        var link = $"https://docs.avaloniaui.net/docs/controls/{TargetType.Name.ToLower()}";
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true, Verb = "open" });
        }
        catch
        {
            await DialogHelper.ShowUnableToOpenLinkDialog(new Uri(link));
        }
    }

    public void ShowControlDefintion(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.ShowControlDefinitionOverlay(TargetType);
    }


    private IList<ControlExampleSubstitution> _substitutions;

    public static Flyout _copiedNoticeFlyout;
    private Border _previewAreaHost;
    private Button _expandOptionsButton;
    private ThemeVariantScope _exampleThemeScopeProvider;
    private Border _optionsHost;

    private Button _moreButton;

    private static ImplicitAnimationCollection _optionsHostAnimation;
}

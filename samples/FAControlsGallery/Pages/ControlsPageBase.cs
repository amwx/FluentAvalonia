using System.Numerics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using FAControlsGallery.Controls;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public class ControlsPageBase : UserControl, IStyleable
{
    public ControlsPageBase()
    {
        PageXamlSourceLink = new Uri($"{GithubPrefixString}/{GetType().Name}.axaml");
        PageCSharpSourceLink = new Uri($"{GithubPrefixString}/{GetType().Name}.axaml.cs");
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

    public static string GithubPrefixString => "https://github.com/amwx/FluentAvalonia/tree/master/FluentAvaloniaSamples/Pages/FAControlPages";

    Type IStyleable.StyleKey => typeof(ControlsPageBase);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        PseudoClasses.Set(":namespace", ControlNamespace != null);

        _optionsHost = e.NameScope.Find<StackPanel>("OptionsRegion");
        _detailsPanel = e.NameScope.Find<Panel>("PageDetails");
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

        //// Weirdness, wrap panel doesn't work right if we only have the offset animation
        //// Copied from ControlCatalog page, but changed the rotation to 0 (filed issue for this)
        //var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
        //rotationAnimation.Target = "RotationAngle";
        //rotationAnimation.InsertKeyFrame(.5f, 0f);
        //rotationAnimation.InsertKeyFrame(1f, 0f);
        //rotationAnimation.Duration = TimeSpan.FromMilliseconds(400);

        //var animationGroup = compositor.CreateAnimationGroup();
        //animationGroup.Add(offsetAnimation);
        //animationGroup.Add(rotationAnimation);

        var ani = compositor.CreateImplicitAnimationCollection();
        ani["Offset"] = offsetAnimation;

        ec.ImplicitAnimations = ani;
    }

    private bool _isSmallWidth2;
    private CancellationTokenSource _cts;
    private bool _hasLoaded;

    private Panel _detailsPanel;
    private StackPanel _optionsHost;
}

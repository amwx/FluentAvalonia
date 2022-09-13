using System;
using Avalonia;
using System.Windows.Input;
using Avalonia.Controls;
using FluentAvalonia.Core;
using Avalonia.Styling;

namespace FluentAvalonia.UI.Controls;

public partial class TeachingTip : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty =
           AvaloniaProperty.Register<TeachingTip, string>(nameof(Title));

    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<TeachingTip, string>(nameof(Subtitle));

    public static readonly DirectProperty<TeachingTip, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<TeachingTip, bool>(nameof(IsOpen),
            x => x.IsOpen, (x, v) => x.IsOpen = v);

    public static readonly StyledProperty<Control> TargetProperty =
        AvaloniaProperty.Register<TeachingTip, Control>(nameof(Target));

    public static readonly StyledProperty<TeachingTipTailVisibility> TailVisibilityProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipTailVisibility>(nameof(TailVisibility));

    public static readonly StyledProperty<object> ActionButtonContentProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(ActionButtonContent));

    public static readonly StyledProperty<ControlTheme> ActionButtonStyleProperty =
        AvaloniaProperty.Register<TeachingTip, ControlTheme>(nameof(ActionButtonStyle));

    public static readonly StyledProperty<ICommand> ActionButtonCommandProperty =
        AvaloniaProperty.Register<TeachingTip, ICommand>(nameof(ActionButtonCommand));

    public static readonly StyledProperty<object> ActionButtonCommandParameterProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(ActionButtonCommandParameter));

    public static readonly StyledProperty<object> CloseButtonContentProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(CloseButtonContent));

    public static readonly StyledProperty<ControlTheme> CloseButtonStyleProperty =
        AvaloniaProperty.Register<TeachingTip, ControlTheme>(nameof(CloseButtonStyle));

    public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
        AvaloniaProperty.Register<TeachingTip, ICommand>(nameof(CloseButtonCommand));

    public static readonly StyledProperty<object> CloseButtonCommandParameterProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(CloseButtonCommandParameter));

    public static readonly StyledProperty<Thickness> PlacementMarginProperty =
        AvaloniaProperty.Register<TeachingTip, Thickness>(nameof(PlacementMargin));

    public static readonly StyledProperty<bool> ShouldConstrainToRootBoundsProperty =
        AvaloniaProperty.Register<TeachingTip, bool>(nameof(ShouldConstrainToRootBounds));

    public static readonly StyledProperty<bool> IsLightDismissEnabledProperty =
        AvaloniaProperty.Register<TeachingTip, bool>(nameof(IsLightDismissEnabled));

    public static readonly StyledProperty<TeachingTipPlacementMode> PreferredPlacementProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipPlacementMode>(nameof(PreferredPlacement));

    public static readonly StyledProperty<TeachingTipHeroContentPlacementMode> HeroContentPlacementProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipHeroContentPlacementMode>(nameof(HeroContentPlacement));

    public static readonly StyledProperty<IControl> HeroContentProperty =
        AvaloniaProperty.Register<TeachingTip, IControl>(nameof(HeroContent));

    public static readonly StyledProperty<IconSource> IconSourceProperty =
        AvaloniaProperty.Register<TeachingTip, IconSource>(nameof(IconSource));

    public static readonly StyledProperty<TeachingTipTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipTemplateSettings>(nameof(TemplateSettings));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
    public bool IsOpen
    {
        get => _isOpen;
        set => SetAndRaise(IsOpenProperty, ref _isOpen, value);
    }
    public Control Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    public TeachingTipTailVisibility TailVisibility
    {
        get => GetValue(TailVisibilityProperty);
        set => SetValue(TailVisibilityProperty, value);
    }
    public object ActionButtonContent
    {
        get => GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    public ControlTheme ActionButtonStyle
    {
        get => GetValue(ActionButtonStyleProperty);
        set => SetValue(ActionButtonStyleProperty, value);
    }

    public ICommand ActionButtonCommand
    {
        get => GetValue(ActionButtonCommandProperty);
        set => SetValue(ActionButtonCommandProperty, value);
    }
    public object ActionButtonCommandParameter
    {
        get => GetValue(ActionButtonCommandParameterProperty);
        set => SetValue(ActionButtonCommandParameterProperty, value);
    }
    public object CloseButtonContent
    {
        get => GetValue(CloseButtonContentProperty);
        set => SetValue(CloseButtonContentProperty, value);
    }

    public ControlTheme CloseButtonStyle
    {
        get => GetValue(CloseButtonStyleProperty);
        set => SetValue(CloseButtonStyleProperty, value);
    }

    public ICommand CloseButtonCommand
    {
        get => GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }
    public object CloseButtonCommandParameter
    {
        get => GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }
    public Thickness PlacementMargin
    {
        get => GetValue(PlacementMarginProperty);
        set => SetValue(PlacementMarginProperty, value);
    }
    public bool ShouldConstrainToRootBounds
    {
        get => GetValue(ShouldConstrainToRootBoundsProperty);
        set => SetValue(ShouldConstrainToRootBoundsProperty, value);
    }
    public bool IsLightDismissEnabled
    {
        get => GetValue(IsLightDismissEnabledProperty);
        set => SetValue(IsLightDismissEnabledProperty, value);
    }
    public TeachingTipPlacementMode PreferredPlacement
    {
        get => GetValue(PreferredPlacementProperty);
        set => SetValue(PreferredPlacementProperty, value);
    }
    public TeachingTipHeroContentPlacementMode HeroContentPlacement
    {
        get => GetValue(HeroContentPlacementProperty);
        set => SetValue(HeroContentPlacementProperty, value);
    }
    public IControl HeroContent
    {
        get => GetValue(HeroContentProperty);
        set => SetValue(HeroContentProperty, value);
    }
    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }
    public TeachingTipTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        set => SetValue(TemplateSettingsProperty, value);
    }

    public event TypedEventHandler<TeachingTip, EventArgs> ActionButtonClick;

    public event TypedEventHandler<TeachingTip, EventArgs> CloseButtonClick;

    public event TypedEventHandler<TeachingTip, TeachingTipClosingEventArgs> Closing;

    public event TypedEventHandler<TeachingTip, TeachingTipClosedEventArgs> Closed;

    private bool _isOpen;
}

using System;
using Avalonia;
using System.Windows.Input;
using Avalonia.Controls;
using FluentAvalonia.Core;
using Avalonia.Styling;
using FluentAvalonia.Core.Attributes;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// A teaching tip is a notification flyout used to provide contextually relevant information. 
/// It supports rich content (including titles, subtitles, icons, images, and text) and can be 
/// configured for either explicit or light-dismiss.
/// </summary>
public partial class TeachingTip : ContentControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> property
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
           AvaloniaProperty.Register<TeachingTip, string>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Subtitle"/> property
    /// </summary>
    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<TeachingTip, string>(nameof(Subtitle));

    /// <summary>
    /// Defines the <see cref="IsOpen"/> property
    /// </summary>
    public static readonly DirectProperty<TeachingTip, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<TeachingTip, bool>(nameof(IsOpen),
            x => x.IsOpen, (x, v) => x.IsOpen = v);

    /// <summary>
    /// Defines the <see cref="Target"/> property
    /// </summary>
    public static readonly StyledProperty<Control> TargetProperty =
        AvaloniaProperty.Register<TeachingTip, Control>(nameof(Target));

    /// <summary>
    /// Defines the <see cref="TailVisibility"/> property
    /// </summary>
    public static readonly StyledProperty<TeachingTipTailVisibility> TailVisibilityProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipTailVisibility>(nameof(TailVisibility));

    /// <summary>
    /// Defines the <see cref="ActionButtonContent"/> property
    /// </summary>
    public static readonly StyledProperty<object> ActionButtonContentProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(ActionButtonContent));

    /// <summary>
    /// Defines the <see cref="ActionButtonStyle"/> property
    /// </summary>
    public static readonly StyledProperty<ControlTheme> ActionButtonStyleProperty =
        AvaloniaProperty.Register<TeachingTip, ControlTheme>(nameof(ActionButtonStyle));

    /// <summary>
    /// Defines the <see cref="ActionButtonCommand"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> ActionButtonCommandProperty =
        AvaloniaProperty.Register<TeachingTip, ICommand>(nameof(ActionButtonCommand));

    /// <summary>
    /// Defines the <see cref="ActionButtonCommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> ActionButtonCommandParameterProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(ActionButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="CloseButtonContent"/> property
    /// </summary>
    public static readonly StyledProperty<object> CloseButtonContentProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(CloseButtonContent));

    /// <summary>
    /// Defines the <see cref="CloseButtonStyle"/> property
    /// </summary>
    public static readonly StyledProperty<ControlTheme> CloseButtonStyleProperty =
        AvaloniaProperty.Register<TeachingTip, ControlTheme>(nameof(CloseButtonStyle));

    /// <summary>
    /// Defines the <see cref="CloseButtonCommand"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
        AvaloniaProperty.Register<TeachingTip, ICommand>(nameof(CloseButtonCommand));

    /// <summary>
    /// Defines the <see cref="CloseButtonCommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> CloseButtonCommandParameterProperty =
        AvaloniaProperty.Register<TeachingTip, object>(nameof(CloseButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="PlacementMargin"/> property
    /// </summary>
    public static readonly StyledProperty<Thickness> PlacementMarginProperty =
        AvaloniaProperty.Register<TeachingTip, Thickness>(nameof(PlacementMargin));

    /// <summary>
    /// Defines the <see cref="ShouldConstrainToRootBounds"/> property
    /// </summary>
    [NotImplemented]
    public static readonly StyledProperty<bool> ShouldConstrainToRootBoundsProperty =
        AvaloniaProperty.Register<TeachingTip, bool>(nameof(ShouldConstrainToRootBounds));

    /// <summary>
    /// Defines the <see cref="IsLightDismissEnabled" /> property
    /// </summary>
    public static readonly StyledProperty<bool> IsLightDismissEnabledProperty =
        AvaloniaProperty.Register<TeachingTip, bool>(nameof(IsLightDismissEnabled));

    /// <summary>
    /// Defines the <see cref="PreferredPlacement"/> property
    /// </summary>
    public static readonly StyledProperty<TeachingTipPlacementMode> PreferredPlacementProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipPlacementMode>(nameof(PreferredPlacement));

    /// <summary>
    /// Defines the <see cref="HeroContentPlacement"/> property
    /// </summary>
    public static readonly StyledProperty<TeachingTipHeroContentPlacementMode> HeroContentPlacementProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipHeroContentPlacementMode>(nameof(HeroContentPlacement));

    /// <summary>
    /// Defines the <see cref="HeroContent"/> property
    /// </summary>
    public static readonly StyledProperty<IControl> HeroContentProperty =
        AvaloniaProperty.Register<TeachingTip, IControl>(nameof(HeroContent));

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<IconSource> IconSourceProperty =
        AvaloniaProperty.Register<TeachingTip, IconSource>(nameof(IconSource));

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<TeachingTipTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<TeachingTip, TeachingTipTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Gets or sets the title of the teaching tip.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the subtitle of the teaching tip.
    /// </summary>
    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the teaching tip is open.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        set => SetAndRaise(IsOpenProperty, ref _isOpen, value);
    }

    /// <summary>
    /// Gets or sets the target for a teaching tip to position itself relative to and point at with its tail.
    /// </summary>
    public Control Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Toggles collapse of a teaching tip's tail. Can be used to override auto behavior 
    /// to make a tail visible on a non-targeted teaching tip and hidden on a targeted teaching tip.
    /// </summary>
    public TeachingTipTailVisibility TailVisibility
    {
        get => GetValue(TailVisibilityProperty);
        set => SetValue(TailVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the text of the teaching tip's action button.
    /// </summary>
    public object ActionButtonContent
    {
        get => GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Style (ControlTheme) to apply to the action button.
    /// </summary>
    public ControlTheme ActionButtonStyle
    {
        get => GetValue(ActionButtonStyleProperty);
        set => SetValue(ActionButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to invoke when the action button is clicked.
    /// </summary>
    public ICommand ActionButtonCommand
    {
        get => GetValue(ActionButtonCommandProperty);
        set => SetValue(ActionButtonCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command for the action button.
    /// </summary>
    public object ActionButtonCommandParameter
    {
        get => GetValue(ActionButtonCommandParameterProperty);
        set => SetValue(ActionButtonCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the teaching tip's close button.
    /// </summary>
    public object CloseButtonContent
    {
        get => GetValue(CloseButtonContentProperty);
        set => SetValue(CloseButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the Style (ControlTheme) to apply to the teaching tip's close button.
    /// </summary>
    public ControlTheme CloseButtonStyle
    {
        get => GetValue(CloseButtonStyleProperty);
        set => SetValue(CloseButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to invoke when the close button is clicked.
    /// </summary>
    public ICommand CloseButtonCommand
    {
        get => GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command for the close button.
    /// </summary>
    public object CloseButtonCommandParameter
    {
        get => GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }

    /// <summary>
    /// Adds a margin between a targeted teaching tip and its target or between a non-targeted teaching tip and the xaml root.
    /// </summary>
    public Thickness PlacementMargin
    {
        get => GetValue(PlacementMarginProperty);
        set => SetValue(PlacementMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the teaching tip will constrain to the bounds of its xaml root.
    /// </summary>
    [NotImplemented]
    public bool ShouldConstrainToRootBounds
    {
        get => GetValue(ShouldConstrainToRootBoundsProperty);
        set => SetValue(ShouldConstrainToRootBoundsProperty, value);
    }

    /// <summary>
    /// Enables light-dismiss functionality so that a teaching tip will dismiss when a user scrolls or 
    /// interacts with other elements of the application.
    /// </summary>
    public bool IsLightDismissEnabled
    {
        get => GetValue(IsLightDismissEnabledProperty);
        set => SetValue(IsLightDismissEnabledProperty, value);
    }

    /// <summary>
    /// Preferred placement to be used for the teaching tip. If there is not enough space 
    /// to show at the preferred placement, a new placement will be automatically chosen. 
    /// Placement is relative to its target if Target is non-null or to the parent window 
    /// of the teaching tip if Target is null.
    /// </summary>
    public TeachingTipPlacementMode PreferredPlacement
    {
        get => GetValue(PreferredPlacementProperty);
        set => SetValue(PreferredPlacementProperty, value);
    }

    /// <summary>
    /// Placement of the hero content within the teaching tip.
    /// </summary>
    public TeachingTipHeroContentPlacementMode HeroContentPlacement
    {
        get => GetValue(HeroContentPlacementProperty);
        set => SetValue(HeroContentPlacementProperty, value);
    }

    /// <summary>
    /// Border-to-border graphic content displayed in the header or footer
    /// of the teaching tip. Will appear opposite of the tail in targeted teaching tips unless otherwise set.
    /// </summary>
    public IControl HeroContent
    {
        get => GetValue(HeroContentProperty);
        set => SetValue(HeroContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the graphic content to appear alongside the title and subtitle.
    /// </summary>
    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Provides calculated values that can be referenced as TemplatedParent sources when defining 
    /// templates for a TeachingTip. Not intended for general use.
    /// </summary>
    public TeachingTipTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    /// <summary>
    /// Occurs after the action button is clicked.
    /// </summary>
    public event TypedEventHandler<TeachingTip, EventArgs> ActionButtonClick;

    /// <summary>
    /// Occurs after the close button is clicked.
    /// </summary>
    public event TypedEventHandler<TeachingTip, EventArgs> CloseButtonClick;

    /// <summary>
    /// Occurs after the tip is closed.
    /// </summary>
    public event TypedEventHandler<TeachingTip, TeachingTipClosingEventArgs> Closing;

    /// <summary>
    /// Occurs just before the tip begins to close.
    /// </summary>
    public event TypedEventHandler<TeachingTip, TeachingTipClosedEventArgs> Closed;

    private bool _isOpen;
}

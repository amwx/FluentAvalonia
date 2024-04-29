using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvalonia.UI.Controls;

[TemplatePart(_tpAnimatedVisual, typeof(ProgressRingAnimatedVisual))]
public class ProgressRing : RangeBase
{
    /// <summary>
    /// Defines the <see cref="IsActive"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsActive), true);

    /// <summary>
    /// Defines the <see cref="IsIndeterminate"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>(
            new StyledPropertyMetadata<bool>(defaultValue: true));

    /// <summary>
    /// Gets or sets a value that indicates whether the ProgressRing is showing progress
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the progress ring reports generic progress 
    /// with a repeating pattern or reports progress based on the Value property.
    /// </summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _animatedVisualSource = e.NameScope.Get<ProgressRingAnimatedVisual>(_tpAnimatedVisual);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            _animatedVisualSource?.SetValue(change.GetNewValue<double>());
        }
        else if (change.Property == MinimumProperty)
        {
            _animatedVisualSource?.SetMinimum(change.GetNewValue<double>());
        }
        else if (change.Property == MaximumProperty)
        {
            _animatedVisualSource?.SetMaximum(change.GetNewValue<double>());
        }
        else if (change.Property == IsIndeterminateProperty)
        {
            _animatedVisualSource?.SetIndeterminate(change.GetNewValue<bool>());
        }
        else if (change.Property == IsActiveProperty)
        {
            _animatedVisualSource?.SetActive(change.GetNewValue<bool>());
        }
        else if (change.Property == ForegroundProperty)
        {
            _animatedVisualSource?.SetForeground((IBrush)change.NewValue);
        }
        else if (change.Property == BackgroundProperty)
        {
            _animatedVisualSource?.SetBackground((IBrush)change.NewValue);
        }
    }

    private ProgressRingAnimatedVisual _animatedVisualSource;

    private const string _tpAnimatedVisual = "AnimatedVisual";
}

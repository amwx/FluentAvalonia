using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;

namespace FluentAvalonia.UI.Controls;

// See /Internal/BorderRenderHelper.cs for a note on some changes made compared to upstream
// Border and BorderRenderHelper

public class FABorder : Border
{
    /// <summary>
    /// Defines the <see cref="BackgroundSizing" property
    /// </summary>
    public static readonly StyledProperty<BackgroundSizing> BackgroundSizingProperty =
        AvaloniaProperty.Register<FABorder, BackgroundSizing>(nameof(BackgroundSizing));

    /// <summary>
    /// Gets or sets a value that indicates how far the background extends in relation to this element's border.
    /// </summary>
    public BackgroundSizing BackgroundSizing
    {
        get => GetValue(BackgroundSizingProperty);
        set => SetValue(BackgroundSizingProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_helper != null)
        {
            if (change.Property == BackgroundProperty)
            {
                _helper.Background = change.GetNewValue<IBrush>();
            }
            else if (change.Property == BorderBrushProperty)
            {
                _helper.BorderBrush = change.GetNewValue<IBrush>();
            }
            else if (change.Property == BorderThicknessProperty)
            {
                _helper.BorderThickness = change.GetNewValue<Thickness>();
            }
            else if (change.Property == CornerRadiusProperty)
            {
                _helper.CornerRadius = change.GetNewValue<CornerRadius>();
            }
            else if (change.Property == BoxShadowProperty)
            {
                _helper.BoxShadow = change.GetNewValue<BoxShadows>();
            }
            else if (change.Property == BackgroundSizingProperty)
            {
                _helper.BackgroundSizing = change.GetNewValue<BackgroundSizing>();
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        _helper ??= new BorderRenderHelper(Background, BorderBrush, BorderThickness, CornerRadius, BoxShadow, BackgroundSizing);

        _helper.Render(context, Bounds.Size);
    }

    private BorderRenderHelper _helper;    
}

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Utilities;

namespace FluentAvalonia.UI.Controls;

// See /Internal/BorderRenderHelper.cs for a note on some changes made compared to upstream
// Border and BorderRenderHelper

/// <summary>
/// Border control that allows specifying how the border and background align when rendered
/// </summary>
public class FABorder : Decorator
{
    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(Background));

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BorderBrushProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(BorderBrush));

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<Border, Thickness>(nameof(BorderThickness));

    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<Border, CornerRadius>(nameof(CornerRadius));

    /// <summary>
    /// Defines the <see cref="BoxShadow"/> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<Border, BoxShadows>(nameof(BoxShadow));



    /// <summary>
    /// Defines the <see cref="BackgroundSizing"/> property
    /// </summary>
    public static readonly StyledProperty<BackgroundSizing> BackgroundSizingProperty =
        AvaloniaProperty.Register<FABorder, BackgroundSizing>(nameof(BackgroundSizing));

    /// <summary>
    /// Gets or sets a brush with which to paint the background.
    /// </summary>
    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
    set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush with which to paint the border.
    /// </summary>
    public IBrush BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the border.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty); 
        set => SetValue(BorderThicknessProperty, value); 
    }

    /// <summary>
    /// Gets or sets the radius of the border rounded corners.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value); 
    }

    /// <summary>
    /// Gets or sets the box shadow effect parameters
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how far the background extends in relation to this element's border.
    /// </summary>
    public BackgroundSizing BackgroundSizing
    {
        get => GetValue(BackgroundSizingProperty);
        set => SetValue(BackgroundSizingProperty, value);
    }
    
    private Thickness LayoutThickness
    {
        get
        {
            VerifyScale();

            if (_layoutThickness == null)
            {
                var borderThickness = BorderThickness;

                if (UseLayoutRounding)
                    borderThickness = LayoutHelper.RoundLayoutThickness(borderThickness, _scale, _scale);

                _layoutThickness = borderThickness;
            }

            return _layoutThickness.Value;
        }
    }

    private void VerifyScale()
    {
        var currentScale = LayoutHelper.GetLayoutScale(this);
        if (MathUtilities.AreClose(currentScale, _scale))
            return;

        _scale = currentScale;
        _layoutThickness = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_helper != null)
        {
            if (change.Property == BackgroundProperty)
            {
                _helper.Background = change.GetNewValue<IBrush>();
                InvalidateVisual();
            }
            else if (change.Property == BorderBrushProperty)
            {
                _helper.BorderBrush = change.GetNewValue<IBrush>();
                InvalidateVisual();
            }
            else if (change.Property == BorderThicknessProperty)
            {
                _helper.BorderThickness = change.GetNewValue<Thickness>();
                InvalidateMeasure();
            }
            else if (change.Property == CornerRadiusProperty)
            {
                _helper.CornerRadius = change.GetNewValue<CornerRadius>();
                InvalidateVisual();
            }
            else if (change.Property == BoxShadowProperty)
            {
                _helper.BoxShadow = change.GetNewValue<BoxShadows>();
                InvalidateVisual();
            }
            else if (change.Property == BackgroundSizingProperty)
            {
                _helper.BackgroundSizing = change.GetNewValue<BackgroundSizing>();
                InvalidateVisual();
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
    }

    public override void Render(DrawingContext context)
    {
        _helper ??= new BorderRenderHelper(Background, BorderBrush, BorderThickness, 
            CornerRadius, BoxShadow, BackgroundSizing);

        _helper.Render(context, Bounds.Size);
    }

    private BorderRenderHelper _helper;
    private Thickness? _layoutThickness;
    private double _scale;
}

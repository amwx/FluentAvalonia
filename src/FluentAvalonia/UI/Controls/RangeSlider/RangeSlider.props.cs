using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;

namespace FluentAvalonia.UI.Controls;

[TemplatePart(s_tpActiveRectangle, typeof(Rectangle))]
[TemplatePart(s_tpMinThumb, typeof(Thumb))]
[TemplatePart(s_tpMaxThumb, typeof(Thumb))]
[TemplatePart(s_tpContainerCanvas, typeof(Canvas))]
[TemplatePart(s_tpToolTipText, typeof(TextBlock))]
public partial class RangeSlider
{
    /// <summary>
    /// Defines the <see cref="Minimum"/> property
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty = 
        RangeBase.MinimumProperty.AddOwner<RangeSlider>(
            new StyledPropertyMetadata<double>(0d));

    /// <summary>
    /// Defines the <see cref="Maximum"/> property
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty = 
        RangeBase.MaximumProperty.AddOwner<RangeSlider>(
            new StyledPropertyMetadata<double>(100d));

    /// <summary>
    /// Defines the <see cref="RangeStart"/> property
    /// </summary>
    public static readonly StyledProperty<double> RangeStartProperty = 
        AvaloniaProperty.Register<RangeSlider, double>(nameof(RangeStart),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="RangeEnd"/> property
    /// </summary>
    public static readonly StyledProperty<double> RangeEndProperty = 
        AvaloniaProperty.Register<RangeSlider, double>(nameof(RangeEnd), 
            defaultValue: 100, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="StepFrequency"/> property
    /// </summary>
    public static readonly StyledProperty<double> StepFrequencyProperty = 
        AvaloniaProperty.Register<RangeSlider, double>(nameof(StepFrequency), 
            defaultValue: 1);

    public static readonly StyledProperty<string> ToolTipStringFormatProperty =
        AvaloniaProperty.Register<RangeSlider, string>(nameof(ToolTipStringFormat));

    /// <summary>
    /// Gets or sets the minimum allowed value for the RangeSlider
    /// </summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum allowed value for the RangeSlider
    /// </summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Gets or sets the start of the selected range
    /// </summary>
    public double RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    /// <summary>
    /// Gets or sets the end of the selected range
    /// </summary>
    public double RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    /// <summary>
    /// Gets or sets the frequency of ticks when dragging the slider
    /// </summary>
    public double StepFrequency
    {
        get => GetValue(StepFrequencyProperty);
        set => SetValue(StepFrequencyProperty, value);
    }

    /// <summary>
    /// Gets or sets the string format used in the value ToolTip when dragging
    /// </summary>
    public string ToolTipStringFormat
    {
        get => GetValue(ToolTipStringFormatProperty);
        set => SetValue(ToolTipStringFormatProperty, value);
    }

    // Internal for UnitTests
    internal double DragWidth => _containerCanvas.Bounds.Width - _maxThumb.Bounds.Width;

    /// <summary>
    /// Fired when a thumb drag begins
    /// </summary>
    public event EventHandler<VectorEventArgs> ThumbDragStarted;

    /// <summary>
    /// Fired when a thumb drag completes
    /// </summary>
    public event EventHandler<VectorEventArgs> ThumbDragCompleted;

    /// <summary>
    /// Fired when either RangeStart or RangeEnd is changed
    /// </summary>
    public event EventHandler<RangeChangedEventArgs> ValueChanged;

    private const string s_tpActiveRectangle = "ActiveRectangle";
    private const string s_tpMinThumb = "MinThumb";
    private const string s_tpMaxThumb = "MaxThumb";
    private const string s_tpContainerCanvas = "ContainerCanvas";
    private const string s_tpToolTipText = "ToolTipText";
}


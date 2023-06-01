using Avalonia.Data;
using Avalonia.Media;
using Avalonia;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media;

namespace FluentAvalonia.UI.Controls;

public partial class FAColorPicker
{
    /// <summary>
    /// Defines the <see cref="PreviousColor"/> property
    /// </summary>
    public static readonly StyledProperty<Color2> PreviousColorProperty =
        AvaloniaProperty.Register<FAColorPicker, Color2>(nameof(PreviousColor),
            Colors.Red, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="Color"/> property
    /// </summary>
    public static readonly StyledProperty<Color2> ColorProperty =
        AvaloniaProperty.Register<FAColorPicker, Color2>(nameof(Color),
            Colors.Red, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="ColorTextType"/> property
    /// </summary>
    public static readonly StyledProperty<ColorTextType> ColorTextTypeProperty =
        AvaloniaProperty.Register<FAColorPicker, ColorTextType>(nameof(ColorTextType));

    /// <summary>
    /// Defines the <see cref="Component"/> property
    /// </summary>
    public static readonly StyledProperty<ColorSpectrumComponents> ComponentProperty =
        AvaloniaProperty.Register<FAColorPicker, ColorSpectrumComponents>(nameof(Component),
            defaultValue: ColorSpectrumComponents.SaturationValue);

    /// <summary>
    /// Defines the <see cref="IsMoreButtonVisible"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsMoreButtonVisibleProperty =
        AvaloniaProperty.Register<FAColorPicker, bool>(nameof(IsMoreButtonVisible));

    /// <summary>
    /// Defines the <see cref="IsCompact"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<FAColorPicker, bool>(nameof(IsCompact));

    /// <summary>
    /// Defines the <see cref="IsAlphaEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AvaloniaProperty.Register<FAColorPicker, bool>(nameof(IsAlphaEnabled),
            defaultValue: true);

    /// <summary>
    /// Defines the <see cref="UseSpectrum"/> property
    /// </summary>
    public static readonly StyledProperty<bool> UseSpectrumProperty =
        AvaloniaProperty.Register<FAColorPicker, bool>(nameof(UseSpectrum), 
            defaultValue: true);

    /// <summary>
    /// Defines the <see cref="UseColorWheel"/> property
    /// </summary>
    public static readonly StyledProperty<bool> UseColorWheelProperty =
        AvaloniaProperty.Register<FAColorPicker, bool>(nameof(UseColorWheel),
            defaultValue: false);

    /// <summary>
    /// Defines the <see cref="UseColorTriangle"/> property
    /// </summary>
    public static readonly StyledProperty<bool> UseColorTriangleProperty =
        AvaloniaProperty.Register<FAColorPicker, bool>(nameof(UseColorTriangle),
            defaultValue: false);

    /// <summary>
    /// Defines the <see cref="UseColorPalette"/> property
    /// </summary>
    public static readonly StyledProperty<bool> UseColorPaletteProperty =
        AvaloniaProperty.Register<FAColorPicker, bool>(nameof(UseColorPalette));

    /// <summary>
    /// Defines the <see cref="CustomPaletteColors"/> property
    /// </summary>
    public static readonly DirectProperty<FAColorPicker, IEnumerable<Color>> CustomPaletteColorsProperty =
        AvaloniaProperty.RegisterDirect<FAColorPicker, IEnumerable<Color>>(nameof(CustomPaletteColors),
            x => x.CustomPaletteColors, (x, v) => x.CustomPaletteColors = v);

    /// <summary>
    /// Defines the <see cref="PaletteColumnCount"/> property
    /// </summary>
    public static readonly StyledProperty<int> PaletteColumnCountProperty =
        AvaloniaProperty.Register<FAColorPicker, int>(nameof(PaletteColumnCount), 
            defaultValue: 10);

    /// <summary>
    /// Gets or sets the color used as the previous color
    /// </summary>
    public Color2 PreviousColor
    {
        get => GetValue(PreviousColorProperty);
        set => SetValue(PreviousColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the current color of this ColorPicker
    /// </summary>
    public Color2 Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Gets or sets how the color should be represented in string form
    /// </summary>
    /// <remarks>
    /// Options: [Hex: #RRGGBB], [HexAlpha: #AARRGGBB], [RGB: rgb (R,G,B)], 
    /// or [RGBA: rgba (R, G, B, A)]
    /// </remarks>
    public ColorTextType ColorTextType
    {
        get => GetValue(ColorTextTypeProperty);
        set => SetValue(ColorTextTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets which components the color spectrum should display
    /// </summary>
    public ColorSpectrumComponents Component
    {
        get => GetValue(ComponentProperty);
        set => SetValue(ComponentProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the More Button is visible on this ColorPicker
    /// </summary>
    public bool IsMoreButtonVisible
    {
        get => GetValue(IsMoreButtonVisibleProperty);
        set => SetValue(IsMoreButtonVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this ColorPicker is in a compact state
    /// </summary>
    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can change the alpha value of the color
    /// </summary>
    public bool IsAlphaEnabled
    {
        get => GetValue(IsAlphaEnabledProperty); 
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can change the color using the spectrum display
    /// </summary>
    public bool UseSpectrum
    {
        get => GetValue(UseSpectrumProperty);
        set => SetValue(UseSpectrumProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can change the color using the color wheel display
    /// </summary>
    public bool UseColorWheel
    {
        get => GetValue(UseColorWheelProperty);
        set => SetValue(UseColorWheelProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can change the color using the HSV color triangle display
    /// </summary>
    public bool UseColorTriangle
    {
        get => GetValue(UseColorTriangleProperty);
        set => SetValue(UseColorTriangleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a color palette of defined color is available for use
    /// </summary>
    public bool UseColorPalette
    {
        get => GetValue(UseColorPaletteProperty);
        set => SetValue(UseColorPaletteProperty, value);
    }

    /// <summary>
    /// Gets or sets the set of colors to display in the color palette
    /// </summary>
    public IEnumerable<Color> CustomPaletteColors
    {
        get => _customPaletteColors;
        set => SetAndRaise(CustomPaletteColorsProperty, ref _customPaletteColors, value);
    }

    /// <summary>
    /// Gets or sets the number of columns to use in the color palette display
    /// </summary>
    public int PaletteColumnCount
    {
        get => GetValue(PaletteColumnCountProperty);
        set => SetValue(PaletteColumnCountProperty, value);
    }

    /// <summary>
    /// Event raised when the <see cref="Color"/> property changes
    /// </summary>
    public event TypedEventHandler<FAColorPicker, ColorChangedEventArgs> ColorChanged;


    private IEnumerable<Color> _customPaletteColors;
}

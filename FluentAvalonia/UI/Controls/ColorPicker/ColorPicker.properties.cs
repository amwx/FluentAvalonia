using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls
{
	public partial class ColorPicker
	{
		/// <summary>
		/// Defines the <see cref="PreviousColor"/> property
		/// </summary>
		public static readonly StyledProperty<Color2> PreviousColorProperty =
			AvaloniaProperty.Register<ColorPicker, Color2>(nameof(PreviousColor),
				Colors.Red, defaultBindingMode: BindingMode.TwoWay);

		/// <summary>
		/// Defines the <see cref="Color"/> property
		/// </summary>
		public static readonly StyledProperty<Color2> ColorProperty =
			AvaloniaProperty.Register<ColorPicker, Color2>(nameof(Color),
				Colors.Red, defaultBindingMode: BindingMode.TwoWay);

		/// <summary>
		/// Defines the <see cref="ColorTextType"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPicker, ColorTextType> ColorTextTypeProperty =
			AvaloniaProperty.RegisterDirect<ColorPicker, ColorTextType>(nameof(ColorTextType),
				x => x.ColorTextType, (x, v) => x.ColorTextType = v);

		/// <summary>
		/// Defines the <see cref="Component"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPicker, ColorSpectrumComponents> ComponentProperty =
			AvaloniaProperty.RegisterDirect<ColorPicker, ColorSpectrumComponents>(nameof(Component),
				x => x.Component, (x, v) => x.Component = v);

		/// <summary>
		/// Defines the <see cref="IsMoreButtonVisible"/> property
		/// </summary>
		public static readonly StyledProperty<bool> IsMoreButtonVisibleProperty =
			AvaloniaProperty.Register<ColorPicker, bool>(nameof(IsMoreButtonVisible));

		/// <summary>
		/// Defines the <see cref="IsCompact"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPicker, bool> IsCompactProperty =
			AvaloniaProperty.RegisterDirect<ColorPicker, bool>(nameof(IsCompact),
				x => x.IsCompact, (x, v) => x.IsCompact = v);

		/// <summary>
		/// Defines the <see cref="IsAlphaEnabled"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPicker, bool> IsAlphaEnabledProperty =
			AvaloniaProperty.RegisterDirect<ColorPicker, bool>(nameof(IsAlphaEnabled),
				x => x.IsAlphaEnabled, (x, v) => x.IsAlphaEnabled = v);

		/// <summary>
		/// Defines the <see cref="UseSpectrum"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseSpectrumProperty =
			AvaloniaProperty.Register<ColorPicker, bool>(nameof(UseSpectrum), defaultValue: true);

		/// <summary>
		/// Defines the <see cref="UseColorWheel"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseColorWheelProperty =
			AvaloniaProperty.Register<ColorPicker, bool>(nameof(UseColorWheel));

		/// <summary>
		/// Defines the <see cref="UseColorTriangle"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseColorTriangleProperty =
			AvaloniaProperty.Register<ColorPicker, bool>(nameof(UseColorTriangle));

		/// <summary>
		/// Defines the <see cref="UseColorPalette"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseColorPaletteProperty =
			AvaloniaProperty.Register<ColorPicker, bool>(nameof(UseColorPalette));

		/// <summary>
		/// Defines the <see cref="CustomPaletteColors"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPicker, IEnumerable<Color>> CustomPaletteColorsProperty =
			AvaloniaProperty.RegisterDirect<ColorPicker, IEnumerable<Color>>(nameof(CustomPaletteColors),
				x => x.CustomPaletteColors, (x, v) => x.CustomPaletteColors = v);

		/// <summary>
		/// Defines the <see cref="PaletteColumnCount"/> property
		/// </summary>
		public static readonly StyledProperty<int> PaletteColumnCountProperty =
			AvaloniaProperty.Register<ColorPicker, int>(nameof(PaletteColumnCount), defaultValue: 10);

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
			get => _textType;
			set
			{
				if (SetAndRaise(ColorTextTypeProperty, ref _textType, value))
				{
					SetHexBoxHeader();
					UpdateHexBox(Color);
				}
			}
		}

		/// <summary>
		/// Gets or sets which components the color spectrum should display
		/// </summary>
		public ColorSpectrumComponents Component
		{
			get => _component;
			set
			{
				if (SetAndRaise(ComponentProperty, ref _component, value))
				{
					UpdatePickerComponents();
				}
			}
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
			get => _isCompact;
			set
			{
				if (SetAndRaise(IsCompactProperty, ref _isCompact, value))
				{
					PseudoClasses.Set(":compact", value);
					SetAsCompactMode();
				}
			}
		}

		/// <summary>
		/// Gets or sets whether the user can change the alpha value of the color
		/// </summary>
		public bool IsAlphaEnabled
		{
			get => _isAlphaEnabled;
			set
			{
				if (SetAndRaise(IsAlphaEnabledProperty, ref _isAlphaEnabled, value))
				{
					PseudoClasses.Set(":alpha", value);
				}
			}
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
		public event TypedEventHandler<ColorPicker, ColorChangedEventArgs> ColorChanged;


		private ColorTextType _textType;
		private ColorSpectrumComponents _component = ColorSpectrumComponents.SaturationValue;
		private bool _isCompact = false;
		private bool _isAlphaEnabled = true;
		private IEnumerable<Color> _customPaletteColors;
	}
}

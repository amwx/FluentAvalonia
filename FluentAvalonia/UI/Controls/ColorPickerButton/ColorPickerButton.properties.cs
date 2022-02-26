using System;
using Avalonia.Data;
using Avalonia;
using Avalonia.Media;
using FluentAvalonia.UI.Media;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls
{
	public partial class ColorPickerButton
	{
		/// <summary>
		/// Defines the <see cref="Color"/> property
		/// </summary>
		public static readonly StyledProperty<Color2> ColorProperty =
			AvaloniaProperty.Register<ColorPickerButton, Color2>(nameof(Color),
				defaultBindingMode: BindingMode.TwoWay);

		/// <summary>
		/// Defines the <see cref="IsMoreButtonVisible"/> property
		/// </summary>
		public static readonly StyledProperty<bool> IsMoreButtonVisibleProperty =
			ColorPicker.IsMoreButtonVisibleProperty.AddOwner<ColorPickerButton>();

		/// <summary>
		/// Defines the <see cref="IsCompact"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPickerButton, bool> IsCompactProperty =
			ColorPicker.IsCompactProperty.AddOwner<ColorPickerButton>(x => x.IsCompact,
				(x, v) => x.IsCompact = v);

		/// <summary>
		/// Defines the <see cref="IsAlphaEnabled"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPickerButton, bool> IsAlphaEnabledProperty =
			ColorPicker.IsCompactProperty.AddOwner<ColorPickerButton>(
				x => x.IsAlphaEnabled, (x, v) => x.IsAlphaEnabled = v);

		/// <summary>
		/// Defines the <see cref="UseSpectrum"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseSpectrumProperty =
			ColorPicker.UseSpectrumProperty.AddOwner<ColorPickerButton>();

		/// <summary>
		/// Defines the <see cref="UseColorWheel"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseColorWheelProperty =
			ColorPicker.UseColorWheelProperty.AddOwner<ColorPickerButton>();

		/// <summary>
		/// Defines the <see cref="UseColorTriangle"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseColorTriangleProperty =
			ColorPicker.UseColorTriangleProperty.AddOwner<ColorPickerButton>();

		/// <summary>
		/// Defines the <see cref="UseColorPalette"/> property
		/// </summary>
		public static readonly StyledProperty<bool> UseColorPaletteProperty =
			ColorPicker.UseColorPaletteProperty.AddOwner<ColorPickerButton>();

		/// <summary>
		/// Defines the <see cref="CustomPaletteColors"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPickerButton, IEnumerable<Color>> CustomPaletteColorsProperty =
			ColorPicker.CustomPaletteColorsProperty.AddOwner<ColorPickerButton>(x => x.CustomPaletteColors,
				(x, v) => x.CustomPaletteColors = v);

		/// <summary>
		/// Define sthe <see cref="PaletteColumnCount"/> property
		/// </summary>
		public static readonly StyledProperty<int> PaletteColumnCountProperty =
			ColorPicker.PaletteColumnCountProperty.AddOwner<ColorPickerButton>();

		/// <summary>
		/// Defines the <see cref="ShowAcceptDismissButtons"/> property
		/// </summary>
		public static readonly StyledProperty<bool> ShowAcceptDismissButtonsProperty =
			AvaloniaProperty.Register<ColorPickerButton, bool>(nameof(ShowAcceptDismissButtons), defaultValue: true);


		/// <summary>
		/// Gets or sets the current <see cref="Avalonia.Media.Color"/> of the ColorPickerButton
		/// </summary>
		public Color Color
		{
			get => GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <summary>
		/// Gets or sets whether the More button is visible in the <see cref="ColorPicker"/>
		/// </summary>
		public bool IsMoreButtonVisible
		{
			get => GetValue(IsMoreButtonVisibleProperty);
			set => SetValue(IsMoreButtonVisibleProperty, value);
		}

		/// <summary>
		/// Gets or sets whether the <see cref="ColorPicker"/> is in a compact state
		/// </summary>
		public bool IsCompact
		{
			get => _isCompact;
			set
			{
				if (SetAndRaise(IsCompactProperty, ref _isCompact, value))
				{
					PseudoClasses.Set(":compact", value);
				}
			}
		}

		/// <summary>
		/// Gets or sets whether the user can edit the alpha in the <see cref="ColorPicker"/>
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
		/// Gets or sets whether the <see cref="ColorPicker"/> should allow using the 
		/// Color Spectrum display for selecting a color
		/// </summary>
		public bool UseSpectrum
		{
			get => GetValue(UseSpectrumProperty);
			set => SetValue(UseSpectrumProperty, value);
		}

		/// <summary>
		/// Gets or sets whether the <see cref="ColorPicker"/> should allow using the
		/// Color Wheel display for selecting a color
		/// </summary>
		public bool UseColorWheel
		{
			get => GetValue(UseColorWheelProperty);
			set => SetValue(UseColorWheelProperty, value);
		}

		/// <summary>
		/// Gets or sets whether the <see cref="ColorPicker"/> should allow using the 
		/// HSV Color Triangle display for selecting a color
		/// </summary>
		public bool UseColorTriangle
		{
			get => GetValue(UseColorTriangleProperty);
			set => SetValue(UseColorTriangleProperty, value);
		}

		/// <summary>
		/// Gets or sets whether the <see cref="ColorPicker"/> should allow showing the
		/// color palette of pre-defined colors
		/// </summary>
		public bool UseColorPalette
		{
			get => GetValue(UseColorPaletteProperty);
			set => SetValue(UseColorPaletteProperty, value);
		}

		/// <summary>
		/// Gets or sets a collection of colors to be used in the Custom Color Palette display
		/// of the <see cref="ColorPicker"/>
		/// </summary>
		public IEnumerable<Color> CustomPaletteColors
		{
			get => _customPaletteColors ?? (CustomPaletteColors = new AvaloniaList<Color>());
			set => SetAndRaise(CustomPaletteColorsProperty, ref _customPaletteColors, value);
		}

		/// <summary>
		/// Gets or sets the number of columns to use in the Custom Color Palette display 
		/// of the <see cref="ColorPicker"/>
		/// </summary>
		public int PaletteColumnCount
		{
			get => GetValue(PaletteColumnCountProperty);
			set => SetValue(PaletteColumnCountProperty, value);
		}

		/// <summary>
		/// Gets or sets whether the Flyout should show the Accept and Dismiss buttons. If true,
		/// changes to the color are only applied if accept is pressed. If false, changes to the 
		/// color apply immediately.
		/// </summary>
		public bool ShowAcceptDismissButtons
		{
			get => GetValue(ShowAcceptDismissButtonsProperty);
			set => SetValue(ShowAcceptDismissButtonsProperty, value);
		}

        /// <summary>
        /// Raised when the color change was confirmed and the flyout closes.
        /// </summary>
        public event TypedEventHandler<ColorPickerButton, ColorChangedEventArgs> FlyoutConfirmed;

        /// <summary>
        /// Raised when the color change was dismissed and the flyout closes.
        /// </summary>
        public event TypedEventHandler<ColorPickerButton, EventArgs> FlyoutDismissed;

        /// <summary> Raised when the flyout opens.
        /// </summary>
        public event TypedEventHandler<ColorPickerButton, EventArgs> FlyoutOpened;

        /// <summary>
        /// Raised when the flyout closes regardless of confirmation or dismissal.
        /// </summary>
        public event TypedEventHandler<ColorPickerButton, EventArgs> FlyoutClosed;

        private bool _isCompact = true;
		private bool _isAlphaEnabled = true;
		private IEnumerable<Color> _customPaletteColors;
	}
}

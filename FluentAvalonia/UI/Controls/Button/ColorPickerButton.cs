using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AvColor = Avalonia.Media.Color;

namespace FluentAvalonia.UI.Controls
{
    public class ColorPickerButton : TemplatedControl, IStyleable
    {
		static ColorPickerButton()
		{
			IsMoreButtonVisibleProperty.OverrideDefaultValue<ColorPickerButton>(true);
			UseSpectrumProperty.OverrideDefaultValue<ColorPickerButton>(true);
			UseColorWheelProperty.OverrideDefaultValue<ColorPickerButton>(true);
			UseColorTriangleProperty.OverrideDefaultValue<ColorPickerButton>(true);
		}

        public static readonly StyledProperty<Color2> ColorProperty =
            AvaloniaProperty.Register<ColorPickerButton, Color2>(nameof(Color), defaultBindingMode: BindingMode.TwoWay);

		public static readonly StyledProperty<bool> IsMoreButtonVisibleProperty =
			ColorPicker.IsMoreButtonVisibleProperty.AddOwner<ColorPickerButton>();

		public static readonly DirectProperty<ColorPickerButton, bool> IsCompactProperty =
			ColorPicker.IsCompactProperty.AddOwner<ColorPickerButton>(x => x.IsCompact, 
				(x, v) => x.IsCompact = v);

		public static readonly DirectProperty<ColorPickerButton, bool> IsAlphaEnabledProperty =
			ColorPicker.IsCompactProperty.AddOwner<ColorPickerButton>(
				x => x.IsAlphaEnabled, (x, v) => x.IsAlphaEnabled = v);

		public static readonly StyledProperty<bool> UseSpectrumProperty =
			ColorPicker.UseSpectrumProperty.AddOwner<ColorPickerButton>();

		public static readonly StyledProperty<bool> UseColorWheelProperty =
			ColorPicker.UseColorWheelProperty.AddOwner<ColorPickerButton>();

		public static readonly StyledProperty<bool> UseColorTriangleProperty =
			ColorPicker.UseColorTriangleProperty.AddOwner<ColorPickerButton>();

		public static readonly StyledProperty<bool> UseColorPaletteProperty =
			ColorPicker.UseColorPaletteProperty.AddOwner<ColorPickerButton>();

		public static readonly DirectProperty<ColorPickerButton, IEnumerable<AvColor>> CustomPaletteColorsProperty =
			ColorPicker.CustomPaletteColorsProperty.AddOwner<ColorPickerButton>(x => x.CustomPaletteColors, 
				(x, v) => x.CustomPaletteColors = v);

		public static readonly StyledProperty<int> PaletteColumnCountProperty =
			ColorPicker.PaletteColumnCountProperty.AddOwner<ColorPickerButton>();

		public static readonly StyledProperty<bool> ShowAcceptDismissButtonsProperty =
			AvaloniaProperty.Register<ColorPickerButton, bool>(nameof(ShowAcceptDismissButtons), defaultValue: true);


		Type IStyleable.StyleKey => typeof(ColorPickerButton);

        public AvColor Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
		public bool IsMoreButtonVisible
		{
			get => GetValue(IsMoreButtonVisibleProperty);
			set => SetValue(IsMoreButtonVisibleProperty, value);
		}

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

		public bool UseSpectrum
		{
			get => GetValue(UseSpectrumProperty);
			set => SetValue(UseSpectrumProperty, value);
		}

		public bool UseColorWheel
		{
			get => GetValue(UseColorWheelProperty);
			set => SetValue(UseColorWheelProperty, value);
		}

		public bool UseColorTriangle
		{
			get => GetValue(UseColorTriangleProperty);
			set => SetValue(UseColorTriangleProperty, value);
		}

		public bool UseColorPalette
		{
			get => GetValue(UseColorPaletteProperty);
			set => SetValue(UseColorPaletteProperty, value);
		}

		public IEnumerable<AvColor> CustomPaletteColors
		{
			get => _customPaletteColors ?? (CustomPaletteColors = new AvaloniaList<AvColor>());
			set => SetAndRaise(CustomPaletteColorsProperty, ref _customPaletteColors, value);
		}

		public int PaletteColumnCount
		{
			get => GetValue(PaletteColumnCountProperty);
			set => SetValue(PaletteColumnCountProperty, value);
		}

		public bool ShowAcceptDismissButtons
		{
			get => GetValue(ShowAcceptDismissButtonsProperty);
			set => SetValue(ShowAcceptDismissButtonsProperty, value);
		}


		public event TypedEventHandler<ColorPickerButton, ColorChangedEventArgs> ColorChanged;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (_button != null)
			{
				_button.Click -= OnButtonClick;
			}

            base.OnApplyTemplate(e);

			_button = e.NameScope.Find<Button>("MainButton");
			if (_button != null)
			{
				_button.Click += OnButtonClick;
			}
        }

		protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnAttachedToVisualTree(e);
			if(_flyout == null)
			{
				_flyout = new ColorPickerFlyout();
			}
			_flyout.Closed += OnFlyoutClosed;
			_flyout.Confirmed += OnFlyoutConfirmed;
			_flyout.Dismissed += OnFlyoutDismissed;
		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);
			_flyout.Closed -= OnFlyoutClosed;
			_flyout.Confirmed -= OnFlyoutConfirmed;
			_flyout.Dismissed -= OnFlyoutDismissed;
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == ColorProperty)
			{
				ColorChanged?.Invoke(this, new ColorChangedEventArgs(change.OldValue.GetValueOrDefault<Color2>(),
					change.NewValue.GetValueOrDefault<Color2>()));
			}
		}

		private void OnButtonClick(object sender, RoutedEventArgs e)
		{
			// ColorPicker is a large control, so the flyout is shared among all the ColorButton instances
			// So we need to make sure the ColorPicker is properly set for this button
			_flyout.ColorPicker.PreviousColor = Color;
						
			_flyout.ColorPicker.Color = Color;

			_flyout.ShowHideButtons(ShowAcceptDismissButtons);
			
			// If not showing the buttons, we'll update the color in real time
			if (!ShowAcceptDismissButtons)
			{
				_flyout.ColorPicker.ColorChanged += OnColorPickerColorChanged;
			}

			_flyout.ColorPicker.IsMoreButtonVisible = IsMoreButtonVisible;
			_flyout.ColorPicker.IsCompact = IsCompact;
			_flyout.ColorPicker.IsAlphaEnabled = IsAlphaEnabled;
			_flyout.ColorPicker.UseSpectrum = UseSpectrum;
			_flyout.ColorPicker.UseColorWheel = UseColorWheel;
			_flyout.ColorPicker.UseColorTriangle = UseColorTriangle;
			_flyout.ColorPicker.UseColorPalette = UseColorPalette;
			_flyout.ColorPicker.CustomPaletteColors = CustomPaletteColors;
			_flyout.ColorPicker.PaletteColumnCount = PaletteColumnCount;
			_flyout.ShowAt(this);

			// Keep track of which button the flyout is active on
			_flyoutActive = true;
		}

		private void OnColorPickerColorChanged(ColorPicker sender, ColorChangedEventArgs args)
		{
			Color = args.NewColor;
		}

		private void OnFlyoutDismissed(ColorPickerFlyout sender, object args)
		{
			if (_flyoutActive)
			{
				_flyoutActive = false;				
			}
		}

		private void OnFlyoutConfirmed(ColorPickerFlyout sender, object args)
		{
			if (_flyoutActive)
			{
				_flyoutActive = false;

				Color = _flyout.ColorPicker.Color;
			}
		}

		private void OnFlyoutClosed(object sender, EventArgs e)
		{
			if (!ShowAcceptDismissButtons)
			{
				_flyout.ColorPicker.ColorChanged -= OnColorPickerColorChanged;
			}
		}

		private static ColorPickerFlyout _flyout;

		private bool _flyoutActive;
		private Button _button;
		private bool _isCompact = true;
		private bool _isAlphaEnabled = true;
		private IEnumerable<AvColor> _customPaletteColors;
	}
}

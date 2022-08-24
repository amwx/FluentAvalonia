using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using System;
using FluentAvalonia.UI.Media;
using FluentAvalonia.Core;
using Avalonia.Interactivity;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Defines a control for selecting a color
	/// </summary>
	public partial class ColorPicker : TemplatedControl
    {
        public ColorPicker()
        {

		}
		        
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            //Disconnect event handlers...
            if (_templateApplied)
            {
				UnhookEvents();
                _templateApplied = false;
            }

            base.OnApplyTemplate(e);

			_displayItemTabControl = e.NameScope.Find<TabControl>("DisplayItemTabControl");
			if (_displayItemTabControl != null)
			{
				_displayItemTabControl.SelectionChanged += OnDisplayItemChanged;
			}

			_textEntryTabHost = e.NameScope.Find<Panel>("TextEntryTabHost");
			_rootGrid = e.NameScope.Find<Grid>("Root");
			_textEntryArea = e.NameScope.Find<StackPanel>("TextEntryArea");
						
			_spectrum = e.NameScope.Find<ColorSpectrum>("Spectrum");
			if (_spectrum != null)
			{
				_spectrum.ColorChanged += OnSpectrumColorChanged;
			}

			_thirdComponentSlider = e.NameScope.Find<ColorRamp>("ThirdComponentRamp");
			if (_thirdComponentSlider != null)
			{
				_thirdComponentSlider.ColorChanged += OnThirdComponentColorChanged;
			}

			_opacityComponentSlider = e.NameScope.Find<ColorRamp>("SpectrumAlphaRamp");
			if (_opacityComponentSlider != null)
			{
				_opacityComponentSlider.ColorChanged += OnSpectrumAlphaChanged;
			}

			_hueButton = e.NameScope.Find<RadioButton>("HueRadio");
			if (_hueButton != null)
			{
				_hueButton.Checked += OnComponentRBChecked;
			}

			_satButton = e.NameScope.Find<RadioButton>("SatRadio");
			if (_satButton != null)
			{
				_satButton.Checked += OnComponentRBChecked;
			}

			_valButton = e.NameScope.Find<RadioButton>("ValRadio");
			if (_valButton != null)
			{
				_valButton.Checked += OnComponentRBChecked;
			}

			_redButton = e.NameScope.Find<RadioButton>("RedRadio");
			if (_redButton != null)
			{
				_redButton.Checked += OnComponentRBChecked;
			}

			_greenButton = e.NameScope.Find<RadioButton>("GreenRadio");
			if (_greenButton != null)
			{
				_greenButton.Checked += OnComponentRBChecked;
			}

			_blueButton = e.NameScope.Find<RadioButton>("BlueRadio");
			if (_blueButton != null)
			{
				_blueButton.Checked += OnComponentRBChecked;
			}

			_hueBox = e.NameScope.Find<NumberBox>("HueBox");
			if (_hueBox != null)
			{
				_hueBox.ValueChanged += OnComponentBoxValueChanged;
			}

			_satBox = e.NameScope.Find<NumberBox>("SatBox");
			if (_satBox != null)
			{
				_satBox.ValueChanged += OnComponentBoxValueChanged;
			}

			_valBox = e.NameScope.Find<NumberBox>("ValBox");
			if (_valBox != null)
			{
				_valBox.ValueChanged += OnComponentBoxValueChanged;
			}

			_redBox = e.NameScope.Find<NumberBox>("RedBox");
			if (_redBox != null)
			{
				_redBox.ValueChanged += OnComponentBoxValueChanged;
			}

			_greenBox = e.NameScope.Find<NumberBox>("GreenBox");
			if (_greenBox != null)
			{
				_greenBox.ValueChanged += OnComponentBoxValueChanged;
			}

			_blueBox = e.NameScope.Find<NumberBox>("BlueBox");
			if (_blueBox != null)
			{
				_blueBox.ValueChanged += OnComponentBoxValueChanged;
			}

			_alphaBox = e.NameScope.Find<NumberBox>("AlphaBox");
			if (_alphaBox != null)
			{
				_alphaBox.ValueChanged += OnComponentBoxValueChanged;
			}

			_hueRamp = e.NameScope.Find<ColorRamp>("HueRamp");
			if (_hueRamp != null)
			{
				_hueRamp.ColorChanged += OnComponentRampColorChanged;
			}

			_satRamp = e.NameScope.Find<ColorRamp>("SatRamp");
			if (_satRamp != null)
			{
				_satRamp.ColorChanged += OnComponentRampColorChanged;
			}

			_valRamp = e.NameScope.Find<ColorRamp>("ValRamp");
			if (_valRamp != null)
			{
				_valRamp.ColorChanged += OnComponentRampColorChanged;
			}
			_redRamp = e.NameScope.Find<ColorRamp>("RedRamp");
			if (_redRamp != null)
			{
				_redRamp.ColorChanged += OnComponentRampColorChanged;
			}

			_greenRamp = e.NameScope.Find<ColorRamp>("GreenRamp");
			if (_greenRamp != null)
			{
				_greenRamp.ColorChanged += OnComponentRampColorChanged;
			}

			_blueRamp = e.NameScope.Find<ColorRamp>("BlueRamp");
			if (_blueRamp != null)
			{
				_blueRamp.ColorChanged += OnComponentRampColorChanged;
			}

			_alphaRamp = e.NameScope.Find<ColorRamp>("AlphaRamp");
			if (_alphaRamp != null)
			{
				_alphaRamp.ColorChanged += OnComponentRampColorChanged;
			}

			_hexBox = e.NameScope.Find<TextBox>("HexBox");
			if (_hexBox != null)
			{
				_hexBox.KeyDown += OnHexBoxKeyDown;
			}

			_rgbButton = e.NameScope.Find<ToggleButton>("RGBButton");
			if (_rgbButton != null)
			{
				_rgbButton.Checked += OnColorTypeRBChecked;
			}
			_hsvButton = e.NameScope.Find<ToggleButton>("HSVButton");
			if (_hsvButton != null)
			{
				_hsvButton.Checked += OnColorTypeRBChecked;
			}

			PseudoClasses.Set(":alpha", IsAlphaEnabled);
			PseudoClasses.Set(":compact", IsCompact);
			
			_templateApplied = true;

			if (IsCompact)
			{
				SetAsCompactMode();
			}

			UpdatePickerComponents();
			OnDisplayItemChanged(null, null);

			UpdateColorAndControls(Color, ColorUpdateReason.Initial);
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == ColorProperty)
			{
				if (!_ignoreColorChange)
				{
					UpdateColorAndControls(change.GetNewValue<Color2>(), ColorUpdateReason.Programmatic);
				}
			}
		}

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
			base.OnPointerReleased(e);
			if (!e.Handled && e.InitialPressMouseButton == MouseButton.Left 
				&& e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
			{
				if (e.Source is Border b)
				{
					if (b.Name == "Dark2PreviewBorder" || b.Name == "Dark1PreviewBorder"
						 || b.Name == "Light1PreviewBorder" || b.Name == "Light2PreviewBorder")
					{
						Color = (b.Background as ISolidColorBrush)?.Color ?? Color;
					}
				}
			}
		}

		private void OnDisplayItemChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_templateApplied)
				return;

			int selIndex = _displayItemTabControl.SelectedIndex;
			if (selIndex == 0)
			{
				_spectrum.Shape = ColorSpectrumShape.Spectrum;
				UpdatePickerComponents();

				PseudoClasses.Set(":spectrum", true);
				PseudoClasses.Set(":wheel", false);
				PseudoClasses.Set(":triangle", false);
				PseudoClasses.Set(":palette", false);
				PseudoClasses.Set(":textentry", false);
			}
			else if (selIndex == 1)
			{
				_spectrum.Shape = ColorSpectrumShape.Wheel;

				if (_thirdComponentSlider != null)
				{
					_thirdComponentSlider.Component = ColorComponent.Value;
				}

				PseudoClasses.Set(":spectrum", false);
				PseudoClasses.Set(":wheel", true);
				PseudoClasses.Set(":triangle", false);
				PseudoClasses.Set(":palette", false);
				PseudoClasses.Set(":textentry", false);
			}
			else if (selIndex == 2)
			{
				_spectrum.Shape = ColorSpectrumShape.Triangle;
				PseudoClasses.Set(":spectrum", false);
				PseudoClasses.Set(":wheel", false);
				PseudoClasses.Set(":triangle", true);
				PseudoClasses.Set(":palette", false);
				PseudoClasses.Set(":textentry", false);
			}
			else if (selIndex == 3)
			{
				PseudoClasses.Set(":spectrum", false);
				PseudoClasses.Set(":wheel", false);
				PseudoClasses.Set(":triangle", false);
				PseudoClasses.Set(":palette", true);
				PseudoClasses.Set(":textentry", false);
			}
			else if (selIndex == 4)
			{
				PseudoClasses.Set(":spectrum", false);
				PseudoClasses.Set(":wheel", false);
				PseudoClasses.Set(":triangle", false);
				PseudoClasses.Set(":palette", false);
				PseudoClasses.Set(":textentry", true);
			}
		}

		private void UpdatePickerComponents()
		{
			if (!_templateApplied || _spectrum == null || _thirdComponentSlider == null || !UseSpectrum)
				return;

			if (IsCompact)
			{
				//In Compact mode, we limit the Spectrum to Sat/Val, with the third component showing hue
				//The radiobuttons are hidden, to keep the minimal UI, and I like this display mode best
				//But don't override the actual setting, so we can restore if compact mode is turned off
				_spectrum.Component = ColorComponent.Hue;
				_thirdComponentSlider.Component = ColorComponent.Hue;
				return;
			}
			
			try
			{
				_ignoreRadioChange = true;

				switch (_component)
				{
					case ColorSpectrumComponents.SaturationValue:
						_spectrum.Component = ColorComponent.Hue;
						_thirdComponentSlider.Component = ColorComponent.Hue;
						
						_hueButton.IsChecked = true;
						break;
					case ColorSpectrumComponents.ValueHue:
						_spectrum.Component = ColorComponent.Saturation;
						_thirdComponentSlider.Component = ColorComponent.Saturation;
						_satButton.IsChecked = true;
						break;
					case ColorSpectrumComponents.SaturationHue:
						_spectrum.Component = ColorComponent.Value;
						_thirdComponentSlider.Component = ColorComponent.Value;
						_valButton.IsChecked = true;
						break;

					case ColorSpectrumComponents.BlueGreen:
						_spectrum.Component = ColorComponent.Red;
						_thirdComponentSlider.Component = ColorComponent.Red;
						_redButton.IsChecked = true;
						break;
					case ColorSpectrumComponents.BlueRed:
						_spectrum.Component = ColorComponent.Green;
						_thirdComponentSlider.Component = ColorComponent.Green;
						_greenButton.IsChecked = true;
						break;
					case ColorSpectrumComponents.GreenRed:
						_spectrum.Component = ColorComponent.Blue;
						_thirdComponentSlider.Component = ColorComponent.Blue;
						_blueButton.IsChecked = true;
						break;
				}
			}
			finally
			{
				_ignoreRadioChange = false;
			}
		}

		private void UpdateColorAndControls(Color2 col, ColorUpdateReason reason)
		{
			if (!_templateApplied)
				return;

			try
			{
				_ignoreColorChange = true;

				var old = Color;
				if (reason != ColorUpdateReason.Programmatic)
					Color = col;

				switch (reason)
				{
					case ColorUpdateReason.Initial:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, true, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.Spectrum:
						UpdateSpectrum(col, false, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.ThirdComponent:
						UpdateSpectrum(col, true, false);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.SpectrumAlphaRamp:
						UpdateSpectrum(col, true, true);
						UpdateRamps(col, true, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, false, true, true);
						break;

					case ColorUpdateReason.RedBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, false, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.HueBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, false, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.GreenBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, false, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.SaturationBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, false, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.BlueBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, false);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.ValueBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, false, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.AlphaBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, false, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.HexBox:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateOpacity(col, true, true, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;

					case ColorUpdateReason.RedRamp:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateRamps(col, true, true, true, false, true, true);
						UpdateOpacity(col, true, false, true);
						UpdateHexBox(col);
						break;

					case ColorUpdateReason.HueSlider:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateRamps(col, false, true, true, true, true, true);
						UpdateOpacity(col, true, false, true);
						UpdateHexBox(col);
						break;

					case ColorUpdateReason.GreenRamp:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateRamps(col, true, true, true, true, false, true);
						UpdateOpacity(col, true, false, true);
						UpdateHexBox(col);
						break;

					case ColorUpdateReason.SaturationSlider:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateRamps(col, true, false, true, true, true, true);
						UpdateOpacity(col, true, false, true);
						UpdateHexBox(col);
						break;

					case ColorUpdateReason.BlueRamp:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateRamps(col, true, true, true, true, true, false);
						UpdateOpacity(col, true, false, true);
						UpdateHexBox(col);
						break;

					case ColorUpdateReason.ValueSlider:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateRamps(col, true, true, false, true, true, true);
						UpdateOpacity(col, true, false, true);
						UpdateHexBox(col);
						break;

					case ColorUpdateReason.AlphaRamp:
						UpdateSpectrum(col, true, true);
						UpdateRamps(col, true, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, true, false);
						break;

					case ColorUpdateReason.Programmatic:
						UpdateSpectrum(col, true, true);
						UpdateBoxes(col, true, true, true, true, true, true);
						UpdateHexBox(col);
						UpdateOpacity(col, true, true, true);
						UpdateRamps(col, true, true, true, true, true, true);
						break;
				}

				RaiseColorChangedEvent(old, col);
			}
			finally
			{
				_ignoreColorChange = false;
			}
		}

		private void UpdateSpectrum(Color2 col, bool spectrum, bool third)
		{
			if (spectrum && _spectrum != null)
				_spectrum.Color = col;

			if (third && _thirdComponentSlider != null)
				_thirdComponentSlider.Color = col;
		}

		private void UpdateBoxes(Color2 col, bool hue, bool sat, bool val, bool red, bool green, bool blue)
		{
			if (hue && _hueBox != null)
				_hueBox.Value = col.Hue;

			if (sat && _satBox != null)
				_satBox.Value = col.Saturation;

			if (val && _valBox != null)
				_valBox.Value = col.Value;

			if (red && _redBox != null)
				_redBox.Value = col.R;

			if (green && _greenBox != null)
				_greenBox.Value = col.G;

			if (blue && _blueBox != null)
				_blueBox.Value = col.B;
		}

		private void UpdateRamps(Color2 col, bool hue, bool sat, bool val, bool red, bool green, bool blue)
		{
			if (hue && _hueBox != null)
				_hueRamp.Color = col;

			if (sat && _satBox != null)
				_satRamp.Color = col;

			if (val && _valBox != null)
				_valRamp.Color = col;

			if (red && _redBox != null)
				_redRamp.Color = col;

			if (green && _greenBox != null)
				_greenRamp.Color = col;

			if (blue && _blueBox != null)
				_blueRamp.Color = col;
		}

		private void UpdateHexBox(Color2 col)
		{
			if (_hexBox == null)
				return;

			switch (_textType)
			{
				case ColorTextType.Hex:
					_hexBox.Text = col.ToHexString(false);
					break;
				case ColorTextType.HexAlpha:
					_hexBox.Text = col.ToHexString();
					break;

				case ColorTextType.RGB:
					_hexBox.Text = col.ToHTML(false);
					break;
				case ColorTextType.RGBA:
					_hexBox.Text = col.ToHTML();
					break;
			}
		}

		private void UpdateOpacity(Color2 col, bool spectrumRamp, bool box, bool ramp)
		{
			if (box && _alphaBox != null)
				_alphaBox.Value = col.A;

			if (ramp && _alphaRamp != null)
				_alphaRamp.Color = col;

			if (spectrumRamp && _isCompact && _opacityComponentSlider != null)
				_opacityComponentSlider.Color = col;
		}

		private void OnHexBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (Color2.TryParse(_hexBox.Text.AsSpan(), out Color2 c))
				{
					UpdateColorAndControls(c, ColorUpdateReason.HexBox);
					DataValidationErrors.SetError(_hexBox, null);
				}
				else
				{
					DataValidationErrors.SetError(_hexBox, new Exception("Invalid input"));
				}
				e.Handled = true;
			}
		}

		private void OnComponentRampColorChanged(ColorPickerComponent sender, ColorChangedEventArgs args)
		{
			if (!_templateApplied || _ignoreColorChange)
				return;

			if (sender == _hueRamp)
				UpdateColorAndControls(args.NewColor, ColorUpdateReason.HueSlider);
			else if (sender == _satRamp)
				UpdateColorAndControls(args.NewColor, ColorUpdateReason.SaturationSlider);
			else if (sender == _valRamp)
				UpdateColorAndControls(args.NewColor, ColorUpdateReason.ValueSlider);
			else if (sender == _redRamp)
				UpdateColorAndControls(args.NewColor, ColorUpdateReason.RedRamp);
			else if (sender == _greenRamp)
				UpdateColorAndControls(args.NewColor, ColorUpdateReason.GreenRamp);
			else if (sender == _blueRamp)
				UpdateColorAndControls(args.NewColor, ColorUpdateReason.BlueRamp);
			else if (sender == _alphaRamp)
				UpdateColorAndControls(args.NewColor, ColorUpdateReason.AlphaRamp);
		}

		private void OnComponentBoxValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
		{
			if (!_templateApplied || _ignoreColorChange)
				return;

			if (sender == _hueBox)
			{
				UpdateColorAndControls(Color.WithHuef((float)args.NewValue), ColorUpdateReason.HueBox);
			}
			else if (sender == _satBox)
			{
				UpdateColorAndControls(Color.WithSatf((float)args.NewValue / 100), ColorUpdateReason.SaturationBox);
			}
			else if (sender == _valBox)
			{
				UpdateColorAndControls(Color.WithValf((float)args.NewValue / 100), ColorUpdateReason.ValueBox);
			}
			else if (sender == _redBox)
			{
				UpdateColorAndControls(Color.WithRedf((float)args.NewValue / 255), ColorUpdateReason.RedBox);
			}
			else if (sender == _greenBox)
			{
				UpdateColorAndControls(Color.WithGreenf((float)args.NewValue / 255), ColorUpdateReason.GreenBox);
			}
			else if (sender == _blueBox)
			{
				UpdateColorAndControls(Color.WithBluef((float)args.NewValue / 255), ColorUpdateReason.BlueBox);
			}
			else if (sender == _alphaBox)
			{
				UpdateColorAndControls(Color.WithAlphaf((float)args.NewValue / 255), ColorUpdateReason.AlphaBox);
			}
		}

		private void OnComponentRBChecked(object sender, RoutedEventArgs e)
		{
			if (!_templateApplied || _ignoreRadioChange)
				return;

			if (sender == _hueButton)
			{
				Component = ColorSpectrumComponents.SaturationValue;
			}
			else if (sender == _satButton)
			{
				Component = ColorSpectrumComponents.ValueHue;
			}
			else if (sender == _valButton)
			{
				Component = ColorSpectrumComponents.SaturationHue;
			}
			else if (sender == _redButton)
			{
				Component = ColorSpectrumComponents.BlueGreen;
			}
			else if (sender == _greenButton)
			{
				Component = ColorSpectrumComponents.BlueRed;
			}
			else if (sender == _blueButton)
			{
				Component = ColorSpectrumComponents.GreenRed;
			}
		}

		private void OnSpectrumAlphaChanged(ColorPickerComponent sender, ColorChangedEventArgs args)
		{
			if (!_templateApplied || _ignoreColorChange)
				return;

			UpdateColorAndControls(args.NewColor, ColorUpdateReason.SpectrumAlphaRamp);
		}

		private void OnThirdComponentColorChanged(ColorPickerComponent sender, ColorChangedEventArgs args)
		{
			if (!_templateApplied || _ignoreColorChange)
				return;

			UpdateColorAndControls(args.NewColor, ColorUpdateReason.ThirdComponent);
		}

		private void OnSpectrumColorChanged(ColorPickerComponent sender, ColorChangedEventArgs args)
		{
			if (!_templateApplied || _ignoreColorChange)
				return;

			UpdateColorAndControls(args.NewColor, ColorUpdateReason.Spectrum);
		}

		private void RaiseColorChangedEvent(Color2 oldColor, Color2 newColor)
        {
            ColorChanged?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
        }

		private void SetAsCompactMode()
		{
			if (!_templateApplied || _rootGrid == null || _textEntryTabHost == null || _textEntryArea == null)
				return;

			if (_isCompact)
			{
				_rootGrid.Children.Remove(_textEntryArea);
				_textEntryTabHost.Children.Add(_textEntryArea);
			}
			else
			{
				_textEntryTabHost.Children.Remove(_textEntryArea);
				_rootGrid.Children.Add(_textEntryArea);

                // If we expand the ColorPicker and we were in the Text entry area while compact
                // make sure we find another tab to switch to so we don't end up with a blank
                // space from the text area being moved
                if (_displayItemTabControl.SelectedIndex == 4)
                {
                    for (int i = 3; i >= 0; i--)
                    {
                        if (_displayItemTabControl.Items.ElementAt(i) is TabItem ti && ti.IsVisible)
                        {
                            _displayItemTabControl.SelectedIndex = i;
                            break;
                        }
                    }
                }
			}

			UpdatePickerComponents();

			if (_rgbButton == null || _hsvButton == null)
				return;

			if (_rgbButton.IsChecked == true)
			{
				PseudoClasses.Set(":rgb", true);
				PseudoClasses.Set(":hsv", false);
			}
			else if (_hsvButton.IsChecked == true)
			{
				PseudoClasses.Set(":rgb", false);
				PseudoClasses.Set(":hsv", true);
			}
			else
			{
				PseudoClasses.Set(":rgb", true);
				PseudoClasses.Set(":hsv", false);
			}
		}

		private void OnColorTypeRBChecked(object sender, RoutedEventArgs e)
		{
			if (sender == _rgbButton)
			{
				PseudoClasses.Set(":rgb", true);
				PseudoClasses.Set(":hsv", false);
			}
			else if (sender == _hsvButton)
			{
				PseudoClasses.Set(":rgb", false);
				PseudoClasses.Set(":hsv", true);
			}
		}

		public void OnHexTextContextMenuItemClick(object param)
		{
			if (param != null)
			{
				if (param.Equals("#AARRGGBB"))
				{
					ColorTextType = ColorTextType.HexAlpha;
				}
				else if (param.Equals("#RRGGBB"))
				{
					ColorTextType = ColorTextType.Hex;
				}
				else if (param.ToString().Contains("rgba"))
				{
					ColorTextType = ColorTextType.RGBA;
				}
				else if (param.ToString().Contains("rgb"))
				{
					ColorTextType = ColorTextType.RGB;
				}
			}
		}

		private void SetHexBoxHeader()
		{
			string val = "";
			switch (ColorTextType)
			{
				case ColorTextType.Hex:
				case ColorTextType.HexAlpha:
					val = "#";
					break;

				case ColorTextType.RGB:
					val = "rgb";
					break;

				case ColorTextType.RGBA:
					val = "rgba";
					break;
			}

			if (_hexBox != null)
			{
				if (_hexBox.InnerLeftContent is Border b && b.Child is TextBlock tb)
				{
					tb.Text = val;
				}
			}
		}

		private void UnhookEvents()
		{
			if (_displayItemTabControl != null)
				_displayItemTabControl.SelectionChanged -= OnDisplayItemChanged;

			if (_spectrum != null)
				_spectrum.ColorChanged -= OnSpectrumColorChanged;

			if (_thirdComponentSlider != null)
				_thirdComponentSlider.ColorChanged -= OnThirdComponentColorChanged;

			if (_opacityComponentSlider != null)
				_opacityComponentSlider.ColorChanged -= OnSpectrumAlphaChanged;

			if (_hueButton != null)
				_hueButton.Checked -= OnComponentRBChecked;

			if (_satButton != null)
				_satButton.Checked -= OnComponentRBChecked;

			if (_valButton != null)
				_valButton.Checked -= OnComponentRBChecked;

			if (_redButton != null)
				_redButton.Checked -= OnComponentRBChecked;

			if (_greenButton != null)
				_greenButton.Checked -= OnComponentRBChecked;

			if (_blueButton != null)
				_blueButton.Checked -= OnComponentRBChecked;

			if (_hueBox != null)
				_hueBox.ValueChanged -= OnComponentBoxValueChanged;

			if (_satBox != null)
				_satBox.ValueChanged -= OnComponentBoxValueChanged;

			if (_valBox != null)
				_valBox.ValueChanged -= OnComponentBoxValueChanged;

			if (_redBox != null)
				_redBox.ValueChanged -= OnComponentBoxValueChanged;

			if (_greenBox != null)
				_greenBox.ValueChanged -= OnComponentBoxValueChanged;

			if (_blueBox != null)
				_blueBox.ValueChanged -= OnComponentBoxValueChanged;

			if (_alphaBox != null)
				_alphaBox.ValueChanged -= OnComponentBoxValueChanged;

			if (_hueRamp != null)
				_hueRamp.ColorChanged -= OnComponentRampColorChanged;

			if (_satRamp != null)
				_satRamp.ColorChanged -= OnComponentRampColorChanged;

			if (_valRamp != null)
				_valRamp.ColorChanged -= OnComponentRampColorChanged;

			if (_redRamp != null)
				_redRamp.ColorChanged -= OnComponentRampColorChanged;

			if (_greenRamp != null)
				_greenRamp.ColorChanged -= OnComponentRampColorChanged;

			if (_blueRamp != null)
				_blueRamp.ColorChanged -= OnComponentRampColorChanged;

			if (_alphaRamp != null)
				_alphaRamp.ColorChanged -= OnComponentRampColorChanged;

			if (_hexBox != null)
				_hexBox.KeyDown += OnHexBoxKeyDown;

			if (_rgbButton != null)
				_rgbButton.Checked -= OnColorTypeRBChecked;

			if (_hsvButton != null)
				_hsvButton.Checked -= OnColorTypeRBChecked;
		}


		//Template Items
		private TabControl _displayItemTabControl;
		
		private ColorSpectrum _spectrum;
        private ColorRamp _thirdComponentSlider;
        private ColorRamp _opacityComponentSlider;

		private RadioButton _hueButton;
		private RadioButton _satButton;
		private RadioButton _valButton;
		private RadioButton _redButton;
		private RadioButton _greenButton;
		private RadioButton _blueButton;

		private ColorRamp _hueRamp;
		private ColorRamp _satRamp;
		private ColorRamp _valRamp;
		private ColorRamp _redRamp;
		private ColorRamp _greenRamp;
		private ColorRamp _blueRamp;
		private ColorRamp _alphaRamp;

		private NumberBox _hueBox;
		private NumberBox _satBox;
		private NumberBox _valBox;
		private NumberBox _redBox;
		private NumberBox _greenBox;
		private NumberBox _blueBox;
		private NumberBox _alphaBox;

        private TextBox _hexBox;

		private Panel _textEntryTabHost;
		private Grid _rootGrid;
		private StackPanel _textEntryArea;

		private ToggleButton _rgbButton;
		private ToggleButton _hsvButton;

		private bool _ignoreRadioChange;
        
        private bool _templateApplied;
        private bool _ignoreColorChange;
	}
}

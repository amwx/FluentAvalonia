using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Runtime.InteropServices;
using FluentAvalonia.UI.Media;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls
{
    public class ColorPicker : TemplatedControl
    {
        public ColorPicker()
        {
            Color = Colors.Red;
        }

        public static readonly StyledProperty<Color2> ColorProperty =
            AvaloniaProperty.Register<ColorPicker, Color2>("Color", Colors.Red, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public Color2 Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DirectProperty<ColorPicker, ColorTextType> ColorTextTypeProperty =
            AvaloniaProperty.RegisterDirect<ColorPicker, ColorTextType>("ColorTextType",
                x => x.ColorTextType, (x,v) => x.ColorTextType = v);

        public static readonly DirectProperty<ColorPicker, ColorSpectrumShape> SpectrumShapeProperty =
            AvaloniaProperty.RegisterDirect<ColorPicker, ColorSpectrumShape>("SpectrumShape",
                x => x.SpectrumShape, (x, v) => x.SpectrumShape = v);

        public static readonly DirectProperty<ColorPicker, ColorSpectrumComponents> ComponentProperty =
            AvaloniaProperty.RegisterDirect<ColorPicker, ColorSpectrumComponents>("Component",
                x => x.Component, (x, v) => x.Component = v);

        public static readonly DirectProperty<ColorPicker, bool> IsMoreButtonVisibleProperty =
            AvaloniaProperty.RegisterDirect<ColorPicker, bool>("IsMoreButtonVisible",
                x => x.IsMoreButtonVisible, (x, v) => x.IsMoreButtonVisible = v);

        public static readonly DirectProperty<ColorPicker, bool> IsTextInputVisibleProperty =
            AvaloniaProperty.RegisterDirect<ColorPicker, bool>("IsTextInputVisible",
                x => x.IsTextInputVisible, (x, v) => x.IsTextInputVisible = v);

        public static readonly DirectProperty<ColorPicker, bool> IsAlphaEnabledProperty =
            AvaloniaProperty.RegisterDirect<ColorPicker, bool>("IsAlphaEnabled",
                x => x.IsAlphaEnabled, (x, v) => x.IsAlphaEnabled = v);

        public ColorTextType ColorTextType
        {
            get => _textType;
            set
            {
                if(SetAndRaise(ColorTextTypeProperty, ref _textType, value))
                {
                    UpdateHexBox(Color);
                }
            }
        }

        public ColorSpectrumShape SpectrumShape
        {
            get => _shape;
            set
            {
                if (SetAndRaise(SpectrumShapeProperty, ref _shape, value))
                {
                    UpdateSpectrumShape();
                }
            }
        }

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

        public bool IsMoreButtonVisible
        {
            get => _moreVisible;
            set 
            {
                if (SetAndRaise(IsMoreButtonVisibleProperty, ref _moreVisible, value))
                {
                    UpdateInputDisplays();
                }
            }
        }

        public bool IsTextInputVisible
        {
            get => _isTextInputVisible;
            set
            {
                if (SetAndRaise(IsTextInputVisibleProperty, ref _isTextInputVisible, value))
                {
                    UpdateInputDisplays();
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
                    UpdateInputDisplays();
                }
            }
        }

        public event TypedEventHandler<ColorPicker, ColorChangedEventArgs> ColorChanged;
        

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            //Disconnect event handlers...
            if (_templateApplied)
            {
                _templateApplied = false;
            }

            base.OnApplyTemplate(e);

            _spectrum = e.NameScope.Find<ColorSpectrum>("ColorSpectrum");
            _spectrum.ColorChanged += OnSpectrumColorChanged; ;

            _thirdComponentSlider = e.NameScope.Find<ColorRamp>("ThirdComponentSlider");
            _thirdComponentSlider.ColorChanged += OnThirdComponentColorChanged; ;

            _opacityComponentSlider = e.NameScope.Find<ColorRamp>("AlphaSlider");
            _opacityComponentSlider.ColorChanged += OnOpacitySliderColorChanged;

            _colorTypeSelector = e.NameScope.Find<Avalonia.Controls.ComboBox>("ColorTypeSelector");
            _colorTypeSelector.SelectionChanged += OnColorTypeChanged;

            _moreButton = e.NameScope.Find<ToggleButton>("MoreButton");
            _moreButton.Checked += MoreButtonChecked;
            _moreButton.Unchecked += MoreButtonChecked;

            var t = e.NameScope.Find<Border>("TransparentBackground");
            if (t != null)
            {
                t.Background = CreateTransparentBackground();
            }

            _comp1Box = e.NameScope.Find<TextBox>("Comp1TB");
            _comp1Label = e.NameScope.Find<TextBlock>("Comp1Label");

            _comp2Box = e.NameScope.Find<TextBox>("Comp2TB");
            _comp2Label = e.NameScope.Find<TextBlock>("Comp2Label");

            _comp3Box = e.NameScope.Find<TextBox>("Comp3TB");
            _comp3Label = e.NameScope.Find<TextBlock>("Comp3Label");

            _opacityBox = e.NameScope.Find<TextBox>("AlphaTB");
            _opacityLabel = e.NameScope.Find<TextBlock>("AlphaLabel");

            _comp1Box.AddHandler(TextBox.TextInputEvent, OnCompBoxInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            _comp2Box.AddHandler(TextBox.TextInputEvent, OnCompBoxInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            _comp3Box.AddHandler(TextBox.TextInputEvent, OnCompBoxInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            _opacityBox.AddHandler(TextBox.TextInputEvent, OnCompBoxInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);

            _hexBox = e.NameScope.Find<TextBox>("HexBox");
            _hexBox.KeyDown += OnHexBoxInput;

            _newColorPreview = e.NameScope.Find<Border>("CurrentColorPreview");

            _textEntryGrid = e.NameScope.Find<Grid>("TextEntryGrid");

            _templateApplied = true;
            var col = Color;
            UpdateColorAndControls(col, ColorUpdateReason.Initial);
                       
            if (!_previousColor.HasValue)
            {
                _previousColor = col;

                e.NameScope.Find<Border>("PreviousColorPreview").Background = new Avalonia.Media.SolidColorBrush(col);
            }

            
            UpdateInputDisplays();
            UpdateSpectrumShape();
            UpdatePickerComponents();
        }


        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ColorProperty)
            {
                if (!_ignoreColorChange)
                {
                    UpdateColorAndControls(change.NewValue.GetValueOrDefault<Color2>(), ColorUpdateReason.Programmatic);                    
                }
            }
        }

        private void UpdateColorAndControls(Color2 col, ColorUpdateReason reason)
        {
            if (!_templateApplied)
                return;

            _ignoreColorChange = true;

            var old = Color;
            if (reason != ColorUpdateReason.Programmatic)
                Color = col;

            switch (reason)
            {
                case ColorUpdateReason.Initial:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.Spectrum:
                    UpdateSpectrum(col, false, true);
                    UpdateBoxes(col, true, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.ThirdComponent:
                    UpdateSpectrum(col, true, false);

                    UpdateBoxes(col, true, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.AlphaSlider:
                    UpdateSpectrum(col, true, true);
                    //UpdateBoxes(true, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, false, true);
                    break;

                case ColorUpdateReason.RedBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, false, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.HueBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, false, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.GreenBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, false, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.SaturationBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, false, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.BlueBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, true, false);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.ValueBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, true, false);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.AlphaBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, false);
                    break;

                case ColorUpdateReason.HexBox:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, true, true);
                    UpdateOpacity(col, true, true);
                    break;

                case ColorUpdateReason.Programmatic:
                    UpdateSpectrum(col, true, true);
                    UpdateBoxes(col, true, true, true);
                    UpdateHexBox(col);
                    UpdateOpacity(col, true, true);
                    break;
            }

            _newColorPreview.Background = new SolidColorBrush(col);
            RaiseColorChangedEvent(old, col);
            _ignoreColorChange = false;
        }

        private void OnOpacitySliderColorChanged(ColorPickerComponent sender, ColorChangedEventArgs args)
        {
            if (!_templateApplied || _ignoreColorChange)
                return;

            UpdateColorAndControls(args.NewColor, ColorUpdateReason.AlphaSlider);
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

        private void OnCompBoxInput(object sender, TextInputEventArgs e)
        {
            if (!_templateApplied || _ignoreColorChange)
                return;
            if (sender == _comp1Box)
            {
                var colorType = _colorTypeSelector.SelectedIndex;
                if (colorType == 0 && float.TryParse(_comp1Box.Text, out float res) && (res >= 0 && res <= 255))
                {
                    UpdateColorAndControls(Color.WithRedf(res / 255), ColorUpdateReason.RedBox);
                }
                else if(colorType == 1 && float.TryParse(_comp1Box.Text, out float res1) && (res1 >= 0 && res1 <= 100))
                {
                    UpdateColorAndControls(Color.WithHuef(res1), ColorUpdateReason.HueBox);
                }
            }
            else if (sender == _comp2Box)
            {
                var colorType = _colorTypeSelector.SelectedIndex;
                if (colorType == 0 && float.TryParse(_comp2Box.Text, out float res) && (res >= 0 && res <= 255))
                {
                    UpdateColorAndControls(Color.WithGreenf(res / 255), ColorUpdateReason.GreenBox);
                }
                else if (colorType == 1 && float.TryParse(_comp2Box.Text, out float res1) && (res1 >= 0 && res1 <= 100))
                {
                    UpdateColorAndControls(Color.WithSatf(res1 / 100), ColorUpdateReason.SaturationBox);
                }
            }
            else if (sender == _comp3Box)
            {
                var colorType = _colorTypeSelector.SelectedIndex;
                if (colorType == 0 && float.TryParse(_comp3Box.Text, out float res) && (res >= 0 && res <= 255))
                {
                    UpdateColorAndControls(Color.WithBluef(res / 255), ColorUpdateReason.BlueBox);
                }
                else if (colorType == 1 && float.TryParse(_comp3Box.Text, out float res1) && (res1 >= 0 && res1 <= 100))
                {
                    UpdateColorAndControls(Color.WithValf(res1 / 100), ColorUpdateReason.ValueBox);
                }
            }
            else if (sender == _opacityBox)
            {
                var colorType = _colorTypeSelector.SelectedIndex;
                if (float.TryParse(_opacityBox.Text, out float res) && (res >= 0 && res <= 255))
                {
                    UpdateColorAndControls(Color.WithAlphaf(res / 255), ColorUpdateReason.AlphaBox);
                }
            }
        }

        private void OnColorTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            _colorSpace = _colorTypeSelector.SelectedIndex;
            if (_colorTypeSelector.SelectedIndex == 1) //HSV
            {
                _comp1Label.Text = "Hue";
                _comp2Label.Text = "Saturation";
                _comp3Label.Text = "Value";
            }
            else if (_colorTypeSelector.SelectedIndex == 0) //RGB
            {
                _comp1Label.Text = "Red";
                _comp2Label.Text = "Green";
                _comp3Label.Text = "Blue";
            }

            //We need the boxes to update, spectrum is most expensive to invalidate, so
            //call that as update method
            UpdateColorAndControls(Color, ColorUpdateReason.Spectrum);
        }

        private void UpdateSpectrum(Color2 col, bool spectrum, bool third)
        {
            if (spectrum)
                _spectrum.Color = col;

            if(third && _spectrum.Shape != ColorSpectrumShape.Triangle)
                _thirdComponentSlider.Color = col;
        }

        private void UpdateBoxes(Color2 col, bool first, bool second, bool third)
        {
            if (_colorSpace == 0)
            {
                _comp1Box.Text = col.R.ToString();
                _comp2Box.Text = col.G.ToString();
                _comp3Box.Text = col.B.ToString();
            }
            else if (_colorSpace == 1)
            {
                _comp1Box.Text = col.Hue.ToString();
                _comp2Box.Text = col.Saturation.ToString();
                _comp3Box.Text = col.Value.ToString();
            }
        }

        private void UpdateHexBox(Color2 col)
        {
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

        private void UpdateOpacity(Color2 col, bool slider, bool box)
        {
            if (slider)
                _opacityComponentSlider.Color = col;

            if (box)
                _opacityBox.Text = col.A.ToString();
        }

        private void OnHexBoxInput(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                if (Color2.TryParse(_hexBox.Text, out Color2 c))
                {
                    UpdateColorAndControls(c, ColorUpdateReason.HexBox);
                    DataValidationErrors.SetError(_hexBox, null);
                }
                else
                {
                    DataValidationErrors.SetError(_hexBox, new Exception("Invalid input"));
                }

                args.Handled = true;
            }
        }

        private void MoreButtonChecked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            UpdateInputDisplays();
        }

        private IBrush CreateTransparentBackground()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(new PixelSize(50, 50));
            using (var context = rtb.CreateDrawingContext(null))
            {
                bool white = true;
                for (int i = 0; i < 50; i += 5)
                {

                    for (int j = 0; j < 50; j += 5)
                    {
                        if (white)
                        {
                            context.DrawRectangle(Brushes.White, null, new Rect(i, j, 5, 5));
                        }
                        else
                        {
                            context.DrawRectangle(Brushes.LightGray, null, new Rect(i, j, 5, 5));
                        }
                        white = !white;
                    }
                    white = !white;
                }
            }
            var b = new ImageBrush(rtb);
            b.TileMode = TileMode.Tile;
            b.BitmapInterpolationMode = Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality;
            b.DestinationRect = new RelativeRect(0, 0, 50, 50, RelativeUnit.Absolute);
            return b.ToImmutable();
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

        private void UpdateSpectrumShape()
        {
            if (!_templateApplied)
                return;

            if (_shape == ColorSpectrumShape.Spectrum || _shape == ColorSpectrumShape.Wheel)
            {
                _thirdComponentSlider.IsVisible = true;
            }
            else
            {
                _thirdComponentSlider.IsVisible = false;
            }
            _spectrum.Shape = _shape;
        }

        private void UpdatePickerComponents()
        {
            if (!_templateApplied || _shape == ColorSpectrumShape.Triangle)
                return;

            if(_shape == ColorSpectrumShape.Wheel) //Wheel is Hue/Saturation
            {
                _thirdComponentSlider.Component = ColorComponent.Value;
                return;
            }

            switch (_component)
            {
                case ColorSpectrumComponents.SaturationValue:
                    _spectrum.Component = ColorComponent.Hue;
                    _thirdComponentSlider.Component = ColorComponent.Hue;
                    break;
                case ColorSpectrumComponents.SaturationHue:
                    _spectrum.Component = ColorComponent.Value;
                    _thirdComponentSlider.Component = ColorComponent.Value;
                    break;
                case ColorSpectrumComponents.ValueHue:
                    _spectrum.Component = ColorComponent.Saturation;
                    _thirdComponentSlider.Component = ColorComponent.Saturation;
                    break;

                case ColorSpectrumComponents.BlueGreen:
                    _spectrum.Component = ColorComponent.Red;
                    _thirdComponentSlider.Component = ColorComponent.Red;
                    break;
                case ColorSpectrumComponents.BlueRed:
                    _spectrum.Component = ColorComponent.Green;
                    _thirdComponentSlider.Component = ColorComponent.Green;
                    break;
                case ColorSpectrumComponents.GreenRed:
                    _spectrum.Component = ColorComponent.Blue;
                    _thirdComponentSlider.Component = ColorComponent.Blue;
                    break;
            }

        }

        private void UpdateInputDisplays()
        {
            if (!_templateApplied)
                return;

            _moreButton.IsVisible = _moreVisible;

            _textEntryGrid.IsVisible = _isTextInputVisible && ((_moreVisible && _moreButton.IsChecked.Value) || !_moreVisible);

            _opacityComponentSlider.IsVisible = _isAlphaEnabled;
            _opacityBox.IsVisible = _isAlphaEnabled;
            _opacityLabel.IsVisible = _isAlphaEnabled;
        }

        private void RaiseColorChangedEvent(Color2 oldColor, Color2 newColor)
        {
            ColorChanged?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
        }



        private ColorSpectrum _spectrum;
        private ColorRamp _thirdComponentSlider;
        private ColorRamp _opacityComponentSlider;

        private TextBlock _comp1Label;
        private TextBlock _comp2Label;
        private TextBlock _comp3Label;
        private TextBlock _opacityLabel;

        private TextBox _comp1Box;
        private TextBox _comp2Box;
        private TextBox _comp3Box;
        private TextBox _opacityBox;

        //private Border _oldColorPreview;
        private Border _newColorPreview;

        private TextBox _hexBox;

        private Avalonia.Controls.ComboBox _colorTypeSelector;

        private ToggleButton _moreButton;
        private Grid _textEntryGrid;

        private int _colorSpace = 0; //0=RGB, 1=HSV
                
        //fields
        private bool _templateApplied;
        private bool _ignoreColorChange;
        private Color2? _previousColor;
        private ColorTextType _textType;
        private ColorSpectrumShape _shape = ColorSpectrumShape.Spectrum;
        private ColorSpectrumComponents _component = ColorSpectrumComponents.SaturationValue;
        private bool _moreVisible = true;
        private bool _isTextInputVisible = true;
        private bool _isAlphaEnabled = true;

    }

    public enum ColorUpdateReason
    {
        Initial,
        Programmatic,
        Spectrum,
        ThirdComponent,
        HueSlider,
        HueBox,
        SaturationSlider,
        SaturationBox,
        ValueSlider,
        ValueBox,
        RedSlider,
        RedBox,
        GreenSlider,
        GreenBox,
        BlueSlider,
        BlueBox,
        AlphaSlider,
        AlphaBox,
        HexBox
    }

    public enum ColorTextType
    {
        Hex, //#FFFFFF
        HexAlpha, //#FFFFFFFF
        RGB, //rgb( 0, 0, 0 )
        RGBA //rgba( 0, 0, 0, 0 )
    }
}

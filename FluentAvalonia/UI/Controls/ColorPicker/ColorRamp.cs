using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Media;
using AvColor = Avalonia.Media.Color;

namespace FluentAvalonia.UI.Controls
{
    public class ColorRamp : ColorPickerComponent
    {
        public ColorRamp()
        {
            ClipToBounds = false;
        }

        static ColorRamp()
        {
            FocusableProperty.OverrideDefaultValue<ColorRamp>(true);
        }

        public static readonly DirectProperty<ColorRamp, Orientation> OrientationProperty =
            AvaloniaProperty.RegisterDirect<ColorRamp, Orientation>("Orientation",
                x => x.Orientation, (x, v) => x.Orientation = v);


        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if(SetAndRaise(OrientationProperty, ref _orientation, value))
                {
                    InvalidateVisual();
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return Orientation == Orientation.Horizontal ? new Size(150, 12) : new Size(12, 150);
        }

        public override void Render(DrawingContext context)
        {
            var b = new LinearGradientBrush();
            //b.SpreadMethod = GradientSpreadMethod.Repeat;

            Rect rect = new Rect(Orientation == Orientation.Horizontal ? _handleRadius : 1,
                Orientation == Orientation.Vertical ? _handleRadius : 1,
                Orientation == Orientation.Horizontal ? Bounds.Width - (_handleRadius * 2) : Bounds.Height - 2,
                Orientation == Orientation.Vertical ? Bounds.Height - (_handleRadius * 2) : Bounds.Height - 2);

            if (Orientation == Orientation.Horizontal)
            {
                b.StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
                b.EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative);
                //b.StartPoint = new RelativePoint(1, 1, RelativeUnit.Absolute);
                //b.EndPoint = new RelativePoint(Bounds.Width - 2, 1, RelativeUnit.Absolute);
            }
            else
            {
                b.StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative);
                b.EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
                //b.StartPoint = new RelativePoint(1, 1, RelativeUnit.Absolute);
                //b.EndPoint = new RelativePoint(1, Bounds.Height - 2, RelativeUnit.Absolute);
            }

            var comp = Component;
            if(comp != ColorComponent.Alpha)
            {
                static AvColor ColorFromHSV(float hue, float sat, float val)
                {
                    var sk = Color2.FromHSV(hue, sat, val);
                    return new AvColor(255, sk.R, sk.G, sk.B);
                }

                switch (comp)
                {
                    case ColorComponent.Hue:
                        for(int i = 0; i <= 360; i += 60)
                        {
                            b.GradientStops.Add(new GradientStop
                            {
                                Color = ColorFromHSV(i, Color.Saturationf, Color.Valuef),
                                Offset = i / 360.0
                            });
                        }
                        break;
                    case ColorComponent.Saturation:
                        b.GradientStops = new GradientStops
                        {
                            new GradientStop
                            {
                                Color = ColorFromHSV(Color.Huef, 0, Color.Valuef),
                                Offset = 0
                            },
                            new GradientStop
                            {
                                Color = ColorFromHSV(Color.Huef, 1, Color.Valuef),
                                Offset = 1
                            }
                        };
                        break;
                    case ColorComponent.Value:
                        b.GradientStops = new GradientStops
                        {
                            new GradientStop
                            {
                                Color = ColorFromHSV(Color.Huef, Color.Saturationf, 0),
                                Offset = 0
                            },
                            new GradientStop
                            {
                                Color = ColorFromHSV(Color.Huef, Color.Saturationf, 1),
                                Offset = 1
                            }
                        };
                        break;


                    case ColorComponent.Red:
                        b.GradientStops = new GradientStops
                        {
                            new GradientStop
                            {
                                Color = AvColor.FromRgb(0, Color.G, Color.B),
                                Offset = 0
                            },
                            new GradientStop
                            {
                                Color = AvColor.FromRgb(255, Color.G, Color.B),
                                Offset = 1
                            }
                        };
                        break;
                    case ColorComponent.Green:
                        b.GradientStops = new GradientStops
                        {
                            new GradientStop
                            {
                                Color = AvColor.FromRgb(Color.R, 0, Color.B),
                                Offset = 0
                            },
                            new GradientStop
                            {
                                Color = AvColor.FromRgb(Color.R, 255, Color.B),
                                Offset = 1
                            }
                        };
                        break;
                    case ColorComponent.Blue:
                        b.GradientStops = new GradientStops
                        {
                            new GradientStop
                            {
                                Color = AvColor.FromRgb(Color.R, Color.G, 0),
                                Offset = 0
                            },
                            new GradientStop
                            {
                                Color = AvColor.FromRgb(Color.R, Color.G, 255),
                                Offset = 1
                            }
                        };
                        break;
                }

                context.FillRectangle(b, rect);
            }
            else
            {
                if(_checkBrush == null)
                {
                    CreateCheckBrush();
                }
                _checkBrush.DestinationRect = new RelativeRect(0, 0, 50, 50, RelativeUnit.Absolute);
                context.FillRectangle(_checkBrush, rect);

                b.GradientStops = new GradientStops
                {
                    new GradientStop(AvColor.FromArgb(0, Color.R, Color.G, Color.B), 0),
                    new GradientStop(AvColor.FromArgb(255, Color.R, Color.G, Color.B), 1)
                };

                context.FillRectangle(b, rect);
            }

            //Render border
            using (context.PushClip(new Rect(0, 0, Bounds.Width, Bounds.Height)))
                context.DrawRectangle(new Pen(Brushes.Gray), rect);

            //Render markers
            if(Orientation == Orientation.Horizontal)
            {
                var x = GetMarkerPosition(comp, rect.Width, false);
                var y = rect.Height / 2 - _handleRadius+1;
                context.FillRectangle(Brushes.Black, new Rect(x - _handleRadius, y, _handleRadius*2, _handleRadius*2), _handleRadius*2);
            }
            else
            {
                var y = GetMarkerPosition(comp, rect.Height, true);
                var x = rect.Width / 2 - _handleRadius+0.5;
                context.FillRectangle(Brushes.Black, new Rect(x, y - _handleRadius, _handleRadius * 2, _handleRadius * 2), _handleRadius * 2);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var pos = e.GetCurrentPoint(this);
            if (pos.Properties.IsLeftButtonPressed)
            {
                _isDown = true;
                SetComponentFromPosition(pos.Position);
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_isDown)
            {
                SetComponentFromPosition(e.GetPosition(this));
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_isDown)
            {
                _isDown = false;
                SetComponentFromPosition(e.GetPosition(this));
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    if(Orientation == Orientation.Vertical)
                    {
                        SetComponentFromKeyPress(e.Key == Key.Up);
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                case Key.Right:
                    if(Orientation == Orientation.Horizontal)
                    {
                        SetComponentFromKeyPress(e.Key == Key.Right);
                        e.Handled = true;
                    }
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnColorChanged(Color2 oldColor, Color2 newColor)
        {
            base.OnColorChanged(oldColor, newColor);
            InvalidateVisual();
        }

        private void SetComponentFromPosition(Point pt)
        {
            double perc = 0;
            if (Orientation == Orientation.Horizontal)
            {
                perc = pt.X / (Bounds.Width - 2);
            }
            else
            {
                perc = 1 - (pt.Y / (Bounds.Height - 2));
            }

            if (perc > 1)
                perc = 1;
            if (perc < 0)
                perc = 0;

            switch (Component)
            {
                case ColorComponent.Hue:
                    Color = Color2.FromHSV(359 * (float)perc, Color.Saturationf, Color.Valuef);
                    break;
                case ColorComponent.Saturation:
                    Color = Color2.FromHSV(Hue, (float)perc, Color.Valuef);
                    break;
                case ColorComponent.Value:
                    Color = Color2.FromHSV(Hue, Color.Saturationf, (float)perc);
                    break;

                case ColorComponent.Red:
                    Color = Color2.FromRGB((float)perc, Color.Gf, Color.Bf, Color.Af);
                    break;
                case ColorComponent.Green:
                    Color = Color2.FromRGB(Color.Rf, (float)perc, Color.Bf, Color.Af);
                    break;
                case ColorComponent.Blue:
                    Color = Color2.FromRGB(Color.Rf, Color.Gf, (float)perc, Color.Af);
                    break;
                case ColorComponent.Alpha:
                    Color = Color2.FromRGB(Color.Rf, Color.Gf, Color.Bf, (float)perc);
                    break;
            }

        }

        private void SetComponentFromKeyPress(bool increment)
        {
            switch (Component)
            {
                case ColorComponent.Hue:
                    Color = Color2.FromHSV(Hue + (increment ? 1 : -1), Color.Saturation, Color.Value);
                    break;
                case ColorComponent.Saturation:
                    Color = Color2.FromHSV(Hue, Color.Saturation + (increment ? 0.01f : -0.01f), Color.Value);
                    break;
                case ColorComponent.Value:
                    Color = Color2.FromHSV(Hue, Color.Saturation, Color.Value + (increment ? 0.01f : -0.01f));
                    break;

                case ColorComponent.Red:
                    //Color = new EliteColor(Color.A, increment ? (byte)(Color.R + 1) : (byte)(Color.R - 1), Color.G, Color.B);
                    break;
                case ColorComponent.Green:
                    //Color = new EliteColor(Color.A, Color.R, increment ? (byte)(Color.G + 1) : (byte)(Color.G - 1), Color.B);
                    break;
                case ColorComponent.Blue:
                    //Color = new EliteColor(Color.A, Color.R, Color.G, increment ? (byte)(Color.B + 1) : (byte)(Color.B - 1));
                    break;
                case ColorComponent.Alpha:
                    //Color = new EliteColor(increment ? (byte)(Color.A + 1) : (byte)(Color.A - 1), Color.R, Color.G, Color.B);
                    break;
            }

        }

        private void CreateCheckBrush()
        {
            //this is only created once

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
            //rtb.Save("C:/Users/Andrew/Desktop/Check.png");
            var b = new ImageBrush(rtb);
            b.TileMode = TileMode.Tile;
           // b.SourceRect = new RelativeRect(0, 0, 50, 50, RelativeUnit.Absolute);
            
            b.BitmapInterpolationMode = Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality;
            _checkBrush = b;//.ToImmutable();

        }

        private double GetMarkerPosition(ColorComponent comp, double length, bool vertical = false)
        {
            switch (comp)
            {
                case ColorComponent.Hue:
                    return vertical ? _handleRadius + (1 - (Hue / 360f)) * length : _handleRadius + (Hue / 360f) * length;
                case ColorComponent.Saturation:
                    return vertical ? _handleRadius + (1 - Color.Saturationf) * length : _handleRadius + Color.Saturationf * length;
                case ColorComponent.Value:
                    return vertical ? _handleRadius + (1 - Color.Valuef) * length : _handleRadius + Color.Valuef * length;

                case ColorComponent.Red:
                    return vertical ? _handleRadius + (1 - Color.Rf) * length : _handleRadius + Color.Rf * length;
                case ColorComponent.Green:
                    return vertical ? _handleRadius + (1 - Color.Gf) * length : _handleRadius + Color.Gf * length;
                case ColorComponent.Blue:
                    return vertical ? _handleRadius + (1 - Color.Bf) * length : _handleRadius + Color.Bf * length;
                case ColorComponent.Alpha:
                    return vertical ? _handleRadius + (1 - Color.Af) * length : _handleRadius + Color.Af * length;

                default:
                    return 0.0;
            }
        }

        //private LinearGradientBrush _gradBrush;
        private TileBrush _checkBrush;
        private bool _isDown;
        private readonly int _handleRadius = 9;

        private Orientation _orientation;
    }

    public enum ColorComponent
    {
        Hue,
        Saturation,
        Value,
        Red,
        Green,
        Blue,
        Alpha
    }
}

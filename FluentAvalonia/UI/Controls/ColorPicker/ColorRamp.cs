using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Media;
using System;
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
            AvaloniaProperty.RegisterDirect<ColorRamp, Orientation>(nameof(Orientation),
                x => x.Orientation, (x, v) => x.Orientation = v);

		public static readonly StyledProperty<IBrush> BorderBrushProperty =
			Border.BorderBrushProperty.AddOwner<ColorRamp>();

		public static readonly StyledProperty<double> BorderThicknessProperty =
			AvaloniaProperty.Register<ColorRamp, double>(nameof(BorderThickness), 1d);

		public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
			Border.CornerRadiusProperty.AddOwner<ColorRamp>();
			
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

		public CornerRadius CornerRadius
		{
			get => GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
		}

		public IBrush BorderBrush
		{
			get => GetValue(BorderBrushProperty);
			set => SetValue(BorderBrushProperty, value);
		}

		public double BorderThickness
		{
			get => GetValue(BorderThicknessProperty);
			set => SetValue(BorderThicknessProperty, value);
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == BorderBrushProperty ||
				change.Property == BorderThicknessProperty)
			{
				RecreateBorderPen();
				InvalidateVisual();
			}
		}

		protected override Size MeasureOverride(Size availableSize)
        {
            return Orientation == Orientation.Horizontal ? new Size(150, 12) : new Size(12, 150);
        }

        public override void Render(DrawingContext context)
        {
			Rect rect = new Rect(Bounds.Size).Inflate(-1);

            if (Orientation == Orientation.Horizontal)
            {
                _lgb.StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
				_lgb.EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative);
            }
            else
            {
				_lgb.StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative);
				_lgb.EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
            }

			var radius = (float)CornerRadius.TopLeft;

			if (Component == ColorComponent.Alpha)
				context.FillRectangle(CheckeredBrush, rect, radius);

			context.FillRectangle(_lgb, rect, radius);

			if (_borderPen != null)
				context.DrawRectangle(_borderPen, rect.Inflate(0.5), radius);

			//Render markers
			if (Orientation == Orientation.Horizontal)
            {
				var hHgt = rect.Height - 2;
				var x = GetMarkerPosition(Component, rect.Width - hHgt, false);
				var y = 1;
				context.DrawRectangle(GetLightness(Color) >= 0.5 ? BlackPen : WhitePen,
					new Rect(x, y+1, hHgt, hHgt), radius);
            }
			else 
			{ 
				var hWid = rect.Width - 2;
                var y = GetMarkerPosition(Component, rect.Height-hWid, true);
                var x = 1;
                context.DrawRectangle(GetLightness(Color) >= 0.5 ? BlackPen : WhitePen, 
					new Rect(x+1, y, hWid, hWid), radius);
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
			EnsureGradientBrush();
            InvalidateVisual();
        }

		protected override void OnComponentChanged(ColorComponent newValue)
		{
			base.OnComponentChanged(newValue);
			EnsureGradientBrush();
		}

		private void EnsureGradientBrush()
		{
			if (Component == ColorComponent.Hue)
			{
				if (_lgb.GradientStops.Count != 7)
				{
					_lgb.GradientStops.Clear();
					for (int i = 0; i <= 360; i += 60)
					{
						_lgb.GradientStops.Add(new GradientStop
						{
							Color = Color.WithHue(i).WithAlpha(255),
							Offset = i / 360.0
						});
					}
				}
			}
			else
			{
				if (_lgb.GradientStops.Count > 2)
				{
					_lgb.GradientStops.RemoveRange(2, _lgb.GradientStops.Count - 2);
				}
				else if (_lgb.GradientStops.Count < 2)
				{
					while (_lgb.GradientStops.Count < 2)
					{
						_lgb.GradientStops.Add(new GradientStop());
					}
				}

				switch (Component)
				{
					case ColorComponent.Saturation:
						_lgb.GradientStops[0].Color = Color.WithSat(0).WithAlpha(255);
						_lgb.GradientStops[0].Offset = 0;
						_lgb.GradientStops[1].Color = Color.WithSatf(1).WithAlpha(255);
						_lgb.GradientStops[1].Offset = 1;
						break;

					case ColorComponent.Value:
						_lgb.GradientStops[0].Color = Color.WithValf(0).WithAlpha(255);
						_lgb.GradientStops[0].Offset = 0;
						_lgb.GradientStops[1].Color = Color.WithValf(1).WithAlpha(255);
						_lgb.GradientStops[1].Offset = 1;
						break;

					case ColorComponent.Red:
						_lgb.GradientStops[0].Color = Color.WithRed(0).WithAlpha(255);
						_lgb.GradientStops[0].Offset = 0;
						_lgb.GradientStops[1].Color = Color.WithRed(255).WithAlpha(255);
						_lgb.GradientStops[1].Offset = 1;
						break;

					case ColorComponent.Green:
						_lgb.GradientStops[0].Color = Color.WithGreen(0).WithAlpha(255);
						_lgb.GradientStops[0].Offset = 0;
						_lgb.GradientStops[1].Color = Color.WithGreen(255).WithAlpha(255);
						_lgb.GradientStops[1].Offset = 1;
						break;

					case ColorComponent.Blue:
						_lgb.GradientStops[0].Color = Color.WithBlue(0).WithAlpha(255);
						_lgb.GradientStops[0].Offset = 0;
						_lgb.GradientStops[1].Color = Color.WithBlue(255).WithAlpha(255);
						_lgb.GradientStops[1].Offset = 1;
						break;

					case ColorComponent.Alpha:
						_lgb.GradientStops[0].Color = Color.WithAlpha(0);
						_lgb.GradientStops[0].Offset = 0;
						_lgb.GradientStops[1].Color = Color.WithAlpha(255);
						_lgb.GradientStops[1].Offset = 1;
						break;
				}
			}
		}


        private void SetComponentFromPosition(Point pt)
        {
            double perc = 0;
            if (Orientation == Orientation.Horizontal)
            {
                perc = (pt.X-1) / (Bounds.Width - 2);
            }
            else
            {
                perc = 1 - ((pt.Y-1) / (Bounds.Height - 2));
            }

            if (perc > 1)
                perc = 1;
            if (perc < 0)
                perc = 0;

            switch (Component)
            {
                case ColorComponent.Hue:
					Color = Color.WithHuef(359 * (float)perc);// Color2.FromHSV(359 * (float)perc, Color.Saturationf, Color.Valuef);
                    break;
                case ColorComponent.Saturation:
					Color = Color.WithSatf((float)perc);// Color2.FromHSV(Hue, (float)perc, Color.Valuef);
                    break;
                case ColorComponent.Value:
					Color = Color.WithValf((float)perc);// Color2.FromHSV(Hue, Color.Saturationf, (float)perc);
                    break;

                case ColorComponent.Red:
                    Color = Color.WithRedf((float)perc);// Color2.FromRGB((float)perc, Color.Gf, Color.Bf, Color.Af);
                    break;
                case ColorComponent.Green:
					Color = Color.WithGreenf((float)perc);// Color2.FromRGB(Color.Rf, (float)perc, Color.Bf, Color.Af);
                    break;
                case ColorComponent.Blue:
					Color = Color.WithBluef((float)perc);// Color2.FromRGB(Color.Rf, Color.Gf, (float)perc, Color.Af);
                    break;
                case ColorComponent.Alpha:
					Color = Color.WithAlphaf((float)perc);// Color2.FromRGB(Color.Rf, Color.Gf, Color.Bf, (float)perc);
                    break;
            }

        }

        private void SetComponentFromKeyPress(bool increment)
        {
            switch (Component)
            {
                case ColorComponent.Hue:
					Color = Color.WithHue(Hue + (increment ? 1 : -1));// Color2.FromHSV(Hue + (increment ? 1 : -1), Color.Saturation, Color.Value);
                    break;
                case ColorComponent.Saturation:
					Color = Color.WithSat(Color.Saturation + (increment ? 1 : -1));
					//Color = Color2.FromHSV(Hue, Color.Saturation + (increment ? 0.01f : -0.01f), Color.Value);
                    break;
                case ColorComponent.Value:
					Color = Color.WithVal(Color.Value + (increment ? 1 : -1));
					// Color = Color2.FromHSV(Hue, Color.Saturation, Color.Value + (increment ? 0.01f : -0.01f));
					break;

                case ColorComponent.Red:
					Color = Color.WithRed(Color.R + (increment ? 1 : -1));
					//Color = Color2.FromRGB(Color.R + (increment ? 1 : -1), Color.G, Color.B);
                    break;
                case ColorComponent.Green:
					Color = Color.WithGreen(Color.G + (increment ? 1 : -1));
					//Color = Color2.FromRGB(Color.R, Color.G + (increment ? 1 : -1), Color.B);
					break;
                case ColorComponent.Blue:
					Color = Color.WithBlue(Color.B + (increment ? 1 : -1));
					//Color = Color2.FromRGB(Color.R, Color.G, Color.B + (increment ? 1 : -1));
                    break;
                case ColorComponent.Alpha:
					Color = Color.WithAlpha(Color.A + (increment ? 1 : -1));
					//Color = Color2.FromRGB(Color.R, Color.G, Color.B, Color.A + (increment ? 1 : -1));
					break;
            }

        }

        private double GetMarkerPosition(ColorComponent comp, double length, bool vertical = false)
        {
            switch (comp)
            {
                case ColorComponent.Hue:
					return vertical ? 1 + (1 - (Hue / 360f)) * length : 1 + (Hue / 360f) * length;
					//return vertical ? _handleRadius + (1 - (Hue / 360f)) * length : _handleRadius + (Hue / 360f) * length;
                case ColorComponent.Saturation:
					return vertical ? 1 + (1 - Color.Saturationf) * length : 1 + Color.Saturationf * length;
                    //return vertical ? _handleRadius + (1 - Color.Saturationf) * length : _handleRadius + Color.Saturationf * length;
                case ColorComponent.Value:
					return vertical ? 1 + (1 - Color.Valuef) * length : 1 + Color.Valuef * length;
					//return vertical ? _handleRadius + (1 - Color.Valuef) * length : _handleRadius + Color.Valuef * length;

                case ColorComponent.Red:
					return vertical ? 1 + (1 - Color.Rf) * length : 1 + Color.Rf * length;
                    //return vertical ? _handleRadius + (1 - Color.Rf) * length : _handleRadius + Color.Rf * length;
                case ColorComponent.Green:
					return vertical ? 1 + (1 - Color.Gf) * length : 1 + Color.Gf * length;
					//return vertical ? _handleRadius + (1 - Color.Gf) * length : _handleRadius + Color.Gf * length;
                case ColorComponent.Blue:
					return vertical ? 1 + (1 - Color.Bf) * length : 1 + Color.Bf * length;
					//return vertical ? _handleRadius + (1 - Color.Bf) * length : _handleRadius + Color.Bf * length;
                case ColorComponent.Alpha:
					return vertical ? 1 + (1 - Color.Af) * length : 1 + Color.Af * length;
					//return vertical ? _handleRadius + (1 - Color.Af) * length : _handleRadius + Color.Af * length;

                default:
                    return 0.0;
            }
        }

		private double GetLightness(AvColor col)
		{
			var rg = col.R <= 10 ? col.R / 3294.0 : Math.Pow(col.R / 269.0 + 0.0513, 2.4);
			var gg = col.G <= 10 ? col.G / 3294.0 : Math.Pow(col.G / 269.0 + 0.0513, 2.4);
			var bg = col.B <= 10 ? col.B / 3294.0 : Math.Pow(col.B / 269.0 + 0.0513, 2.4);
			return 0.2126 * rg + 0.7152 * gg + 0.0722 * bg;
		}

		private void RecreateBorderPen()
		{
			if (BorderBrush == null || BorderThickness == 0)
			{
				_borderPen = null;
				return;
			}

			_borderPen = new Pen(BorderBrush, BorderThickness);
		}


		public static IBrush CheckeredBrush { get; } = CreateCheckeredBrush();

		private static IBrush CreateCheckeredBrush()
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

			var b = new ImageBrush(rtb);
			b.TileMode = TileMode.Tile;
			// b.SourceRect = new RelativeRect(0, 0, 50, 50, RelativeUnit.Absolute);
			b.DestinationRect = new RelativeRect(0, 0, 50, 50, RelativeUnit.Absolute);
			b.BitmapInterpolationMode = Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality;
			return b.ToImmutable();
			
			//_checkBrush = b;//.ToImmutable();

		}
		        
		private static readonly Pen BlackPen = new Pen(Brushes.Black, 3);
		private static readonly Pen WhitePen = new Pen(Brushes.White, 3);

		//This is the brush for the background
		//Create once here & recycle
		private LinearGradientBrush _lgb = new LinearGradientBrush();

		private bool _isDown;
		private Orientation _orientation;
		private Pen _borderPen;		
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

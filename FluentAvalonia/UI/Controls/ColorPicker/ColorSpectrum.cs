using Avalonia;
using System;
using SkiaSharp;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Input;
using FluentAvalonia.UI.Media;
using AvColor = Avalonia.Media.Color;

namespace FluentAvalonia.UI.Controls
{
    public class ColorSpectrum : ColorPickerComponent
    {
        public ColorSpectrum()
        {
            MinWidth = 150;
            MinHeight = 150;
        }

        static ColorSpectrum()
        {
            FocusableProperty.OverrideDefaultValue<ColorSpectrum>(true);
        }


        public static readonly DirectProperty<ColorSpectrum, ColorSpectrumShape> ShapeProperty =
            AvaloniaProperty.RegisterDirect<ColorSpectrum, ColorSpectrumShape>("Shape",
                x => x.Shape, (x, v) => x.Shape = v);


        public ColorSpectrumShape Shape
        {
            get => _shape;
            set
            {
                if (SetAndRaise(ShapeProperty, ref _shape, value))
                    OnShapeChanged(value);
            }
        }


        #region Override Methods

        public override void Render(DrawingContext context)
        {
            if (_tempBitmap == null)
                CreateBitmap();

            Rect rect = new Rect(Bounds.Size);

            using (context.PushClip(new Rect(Bounds.Size)))
            {
                if (Shape == ColorSpectrumShape.Spectrum)
                {
                    context.DrawImage(_tempBitmap, new Rect(_tempBitmap.Size), rect, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);

                    RenderSelectorRects(context, rect.Width, rect.Height);

                    context.DrawRectangle(new Pen(Brushes.Gray), rect);
                }
                else if (Shape == ColorSpectrumShape.Wheel)
                {
                    var minD = Math.Min(Bounds.Width, Bounds.Height) - WheelPadding;
                    if (_lastWheelRect.Width != minD)
                    {
                        _lastWheelRect = new Rect(Bounds.Width / 2 - minD / 2, Bounds.Height / 2 - minD / 2,
                            minD, minD);
                    }

                    Rect x = new Rect(_lastWheelRect.X + 1, _lastWheelRect.Y + 1, minD - 2, minD - 2);
                    context.FillRectangle(Brushes.Black, x, (float)minD * 2);

                    //Rather than creating the bitmap everytime the color changes, we can fake the change in
                    //Value by drawing a Black ellipse behind the image and the using the Value as the opacity
                    //to draw the bitmap
                    using (context.PushOpacity(Color.Valuef))
                        context.DrawImage(_tempBitmap, new Rect(_tempBitmap.Size), _lastWheelRect, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
                }
                else if (Shape == ColorSpectrumShape.Triangle)
                {
                    if (_tempBitmap == null)
                        return;

                    var minD = Math.Min(Bounds.Width, Bounds.Height) - WheelPadding;
                    if (_lastWheelRect.Width != minD || _triangleDirty)
                    {
                        _lastWheelRect = new Rect(Bounds.Width / 2 - minD / 2, Bounds.Height / 2 - minD / 2,
                            minD, minD);
                        CreateBitmap();
                    }

                    context.DrawImage(_tempBitmap, new Rect(_tempBitmap.Size), _lastWheelRect, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);

                    RenderTriangleSelector(context);
                }
            }

            if (Shape == ColorSpectrumShape.Wheel)
            {
                //Don't clip this to bounds so draw here
                RenderWheelSelector(context);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var pt = e.GetCurrentPoint(this);
            if (pt.Properties.IsLeftButtonPressed)
            {
                _isDown = true;
                SetColorFromPosition(pt.Position);
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_isDown)
            {
                SetColorFromPosition(e.GetPosition(this));
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_isDown)
            {
                SetColorFromPosition(e.GetPosition(this));
                _isDownWheel = false;
                _isDown = false;
                _isDownTriangle = false;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                    HandleLeftRightKey(e);
                    break;

                case Key.Up:
                case Key.Down:
                    HandleUpDownKey(e);
                    break;
            }

            base.OnKeyDown(e);
        }


        protected override void OnColorChanged(Color2 oldColor, Color2 newColor)
        {
            base.OnColorChanged(oldColor, newColor);

            if (Shape == ColorSpectrumShape.Spectrum)
            {
                switch (Component)
                {
                    case ColorComponent.Hue:
                        if (newColor.Hue >= 0 && Hue != oldColor.Hue)
                            CreateBitmap();
                        break;
                    case ColorComponent.Saturation:
                        if (oldColor.Saturation != oldColor.Saturation)
                            CreateBitmap();
                        break;
                    case ColorComponent.Value:
                        if (oldColor.Value != oldColor.Value)
                            CreateBitmap();
                        break;
                    case ColorComponent.Red:
                        if (oldColor.R != oldColor.R)
                            CreateBitmap();
                        break;
                    case ColorComponent.Green:
                        if (oldColor.G != oldColor.G)
                            CreateBitmap();
                        break;
                    case ColorComponent.Blue:
                        if (oldColor.B != oldColor.B)
                            CreateBitmap();
                        break;
                }
            }
            else if (Shape == ColorSpectrumShape.Triangle)
            {
                //Check _tempBitmap to make sure we've initialized to not trigger this
                //if the color changes before we've init-d
                if (_tempBitmap != null && newColor.Hue >= 0 && Hue != oldColor.Hue)
                    _triangleDirty = true;
            }

            InvalidateVisual();
        }

        protected override void OnComponentChanged(ColorComponent newValue)
        {
            CreateBitmap();
            base.OnComponentChanged(newValue); //this will call invalidate            
        }

        #endregion

        #region Private Methods

        private void OnShapeChanged(ColorSpectrumShape shape)
        {
            CreateBitmap();
            InvalidateVisual();
        }

        private void CreateBitmap()
        {
            //Triangle sizes the bitmap to the control at all times so we have to make sure we skip this
            //if the control has no size
            //Spectrum & wheel can proceed as normal since they use a fixed bitmap...
            if (Shape == ColorSpectrumShape.Triangle && _lastWheelRect == Rect.Empty)
                return;

            _tempBitmap?.Dispose();

            if (Shape == ColorSpectrumShape.Spectrum)
            {
                _tempBitmap = new WriteableBitmap(new PixelSize(500, 500), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul);

                //Component represents the third, non-displayed, color component
                switch (Component)
                {
                    case ColorComponent.Hue:
                        DrawValueSaturationBitmap();
                        break;
                    case ColorComponent.Saturation:
                        DrawValueHueBitmap();
                        break;
                    case ColorComponent.Value:
                        DrawSaturationHueBitmap();
                        break;
                    case ColorComponent.Red:
                        DrawBlueGreenBitmap();
                        break;
                    case ColorComponent.Green:
                        DrawBlueRedBitmap();
                        break;
                    case ColorComponent.Blue:
                        DrawGreenRedBitmap();
                        break;

                    default:
                        DrawValueSaturationBitmap();
                        break;
                }
            }
            else if (Shape == ColorSpectrumShape.Wheel)
            {
                _tempBitmap = new RenderTargetBitmap(new PixelSize(_defBitmapSize, _defBitmapSize));
                DrawWheelBitmap();
            }
            else if (Shape == ColorSpectrumShape.Triangle)
            {
                _tempBitmap = new RenderTargetBitmap(new PixelSize((int)_lastWheelRect.Width, (int)_lastWheelRect.Height));
                DrawTriangleWheelBitmap();
                //DrawTriangleBitmap();

                _triangleDirty = false;
            }
        }

        private void DrawValueSaturationBitmap()
        {
            using (var lok = (_tempBitmap as WriteableBitmap).Lock())
            {
                unsafe
                {
                    var pixels = (uint*)(void*)lok.Address;
                    var count = _defBitmapSize * _defBitmapSize;
                    uint x = 0;
                    uint y = 0;
                    var hue = Hue;
                    float size = _defBitmapSize;
                    for (int i = 0; i < count; i++)
                    {
                        var value = (x / size);
                        var sat = 1 - (y / size);

                        var col = Color2.FromHSV(hue, sat, value);// SKColor.FromHsv(hue, sat, value);
                        pixels[i] = (uint)(0xFF << 24 | col.R << 16 | col.G << 8 | col.B);
                        x++;
                        if (x == _defBitmapSize)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void DrawValueHueBitmap()
        {
            using (var lok = (_tempBitmap as WriteableBitmap).Lock())
            {
                unsafe
                {
                    var pixels = (uint*)(void*)lok.Address;
                    var count = _defBitmapSize * _defBitmapSize;
                    uint x = 0;
                    uint y = 0;
                    float size = _defBitmapSize;
                    var sat = Color.Saturationf * 100;
                    for (int i = 0; i < count; i++)
                    {
                        var value = (x / size);
                        var hue = 360 - ((y / size) * 360);

                        var col = SKColor.FromHsv(hue, sat, value * 100);
                        pixels[i] = (uint)(0xFF << 24 | col.Red << 16 | col.Green << 8 | col.Blue);
                        x++;
                        if (x == _defBitmapSize)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void DrawSaturationHueBitmap()
        {
            using (var lok = (_tempBitmap as WriteableBitmap).Lock())
            {
                unsafe
                {
                    var pixels = (uint*)(void*)lok.Address;
                    var count = _defBitmapSize * _defBitmapSize;
                    uint x = 0;
                    uint y = 0;
                    float size = _defBitmapSize;
                    var value = Color.Valuef;// * 100;
                    for (int i = 0; i < count; i++)
                    {
                        var sat = (x / size);
                        var hue = 360 - ((y / size) * 360);

                        var col = SKColor.FromHsv(hue, sat * 100, value * 100);
                        pixels[i] = (uint)(0xFF << 24 | col.Red << 16 | col.Green << 8 | col.Blue);
                        x++;
                        if (x == _defBitmapSize)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void DrawBlueGreenBitmap()
        {
            using (var lok = (_tempBitmap as WriteableBitmap).Lock())
            {
                unsafe
                {
                    var pixels = (uint*)(void*)lok.Address;
                    var count = _defBitmapSize * _defBitmapSize;
                    uint x = 0;
                    uint y = 0;
                    float size = _defBitmapSize;
                    var red = (uint)Color.R;
                    for (int i = 0; i < count; i++)
                    {
                        var blue = (x / size) * 255;
                        var green = 255 - ((y / size) * 255);

                        pixels[i] = (uint)(0xFF << 24 | red << 16 | (uint)green << 8 | (uint)blue);
                        x++;
                        if (x == _defBitmapSize)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void DrawBlueRedBitmap()
        {
            using (var lok = (_tempBitmap as WriteableBitmap).Lock())
            {
                unsafe
                {
                    var pixels = (uint*)(void*)lok.Address;
                    var count = _defBitmapSize * _defBitmapSize;
                    uint x = 0;
                    uint y = 0;
                    float size = _defBitmapSize;
                    var green = (uint)Color.G;
                    for (int i = 0; i < count; i++)
                    {
                        var blue = (x / size) * 255;
                        var red = 255 - ((y / size) * 255);

                        pixels[i] = (uint)(0xFF << 24 | (uint)red << 16 | green << 8 | (uint)blue);
                        x++;
                        if (x == _defBitmapSize)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void DrawGreenRedBitmap()
        {
            using (var lok = (_tempBitmap as WriteableBitmap).Lock())
            {
                unsafe
                {
                    var pixels = (uint*)(void*)lok.Address;
                    var count = _defBitmapSize * _defBitmapSize;
                    uint x = 0;
                    uint y = 0;
                    float size = _defBitmapSize;
                    var blue = (uint)Color.B;
                    for (int i = 0; i < count; i++)
                    {
                        var green = (x / size) * 255;
                        var red = 255 - ((y / size) * 255);

                        pixels[i] = (uint)(0xFF << 24 | (uint)red << 16 | (uint)green << 8 | blue);
                        x++;
                        if (x == _defBitmapSize)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void DrawWheelBitmap()
        {
            using (var dc = (_tempBitmap as RenderTargetBitmap).CreateDrawingContext(null))
            using (var skDC = (dc as ISkiaDrawingContextImpl).SkCanvas)
            {
                SKRect rect = SKRect.Create(0, 0, _tempBitmap.PixelSize.Width, _tempBitmap.PixelSize.Height);
                SKPoint center = new SKPoint(rect.MidX, rect.MidY);
                float radius = (rect.Width / 2f) - ((float)WheelPadding / 2f);

                SKShader spectrum = SKShader.CreateSweepGradient(center,
                    new SKColor[]
                    {
                        new SKColor(255, 0, 0),
                        new SKColor(255, 0, 255),
                        new SKColor(0, 0, 255),
                        new SKColor(0, 255, 255),
                        new SKColor(0, 255, 0),
                        new SKColor(255, 255, 0),
                        new SKColor(255, 0, 0)
                    });

                SKPaint paint = new SKPaint();
                paint.Shader = spectrum;
                paint.Style = SKPaintStyle.StrokeAndFill;
                paint.StrokeWidth = 1f;
                paint.IsAntialias = true;

                skDC.DrawCircle(center, radius, paint);

                spectrum.Dispose();

                var grad = SKShader.CreateRadialGradient(center, radius,
                    new SKColor[] { SKColor.Parse("#FFFFFFFF"), SKColor.Parse("#00FFFFFF") },
                    new float[] { 0, 1 }, SKShaderTileMode.Clamp);

                paint.Shader = grad;
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = true;

                skDC.DrawCircle(center, radius, paint);

                paint.Dispose();
                grad.Dispose();
            }
        }

        private void DrawTriangleWheelBitmap()
        {
            using (var dc = (_tempBitmap as RenderTargetBitmap).CreateDrawingContext(null))
            using (var skDC = (dc as ISkiaDrawingContextImpl).SkCanvas)
            {
                SKRect rect = SKRect.Create(0, 0, _tempBitmap.PixelSize.Width, _tempBitmap.PixelSize.Height);
                SKPoint center = new SKPoint(rect.MidX, rect.MidY);
                float radius = (rect.Width / 2f) - ((float)WheelPadding / 2f);

                //Apply scale to TriangleWheelThickness
                var wheelThicc = TriangleWheelThickness * (rect.Width / 500);

                SKShader spectrum = SKShader.CreateSweepGradient(center,
                    new SKColor[]
                    {
                        new SKColor(255, 0, 0),
                        new SKColor(255, 0, 255),
                        new SKColor(0, 0, 255),
                        new SKColor(0, 255, 255),
                        new SKColor(0, 255, 0),
                        new SKColor(255, 255, 0),
                        new SKColor(255, 0, 0)
                    });

                SKPaint paint = new SKPaint();
                paint.Shader = spectrum;
                paint.Style = SKPaintStyle.Fill;
                paint.StrokeWidth = 1f;
                paint.IsAntialias = true;

                skDC.DrawCircle(center, radius, paint);

                spectrum.Dispose();

                //var grad = SKShader.CreateRadialGradient(center, radius,
                //    new SKColor[] { SKColor.Parse("#FFFFFFFF"), SKColor.Parse("#00FFFFFF") },
                //    new float[] { 0, 1 }, SKShaderTileMode.Clamp);
                var grad = SKShader.CreateColor(SKColors.Black);
                paint.Shader = grad;
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = true;
                paint.BlendMode = SKBlendMode.DstOut;

                skDC.DrawCircle(center, radius - wheelThicc, paint);

                grad.Dispose();

                // -- Now draw the Triangle
                paint.BlendMode = SKBlendMode.SrcOver; //Restore to default
                paint.Style = SKPaintStyle.StrokeAndFill;
                //paint.IsAntialias = false;
                rect.Inflate(-wheelThicc + (float)WheelPadding / 2,
                    -wheelThicc + (float)WheelPadding / 2);

                radius -= wheelThicc;

                Color.GetAHSVf(out float hue, out float s, out float v, out float _);
                var h = hue * MathF.PI / 180;
                float third = MathF.PI * (2f / 3f);
                var hx = rect.MidX + radius * MathF.Cos(h);
                var hy = rect.MidY - radius * MathF.Sin(h);

                var sx = rect.MidX + radius * MathF.Cos(h - third);
                var sy = rect.MidY - radius * MathF.Sin(h - third);

                var vx = rect.MidX + radius * MathF.Cos(h + third);
                var vy = rect.MidY - radius * MathF.Sin(h + third);

                SKPath path = new SKPath();
                path.MoveTo(hx, hy);
                path.LineTo(sx, sy);
                path.LineTo(vx, vy);
                path.Close();

                //Black Triangle
                //Transparent - Color(Hue)
                //Transparent to White (composited)

                var colShader = SKShader.CreateColor(SKColors.Black);
                var xMid = (sx + vx) / 2;
                var yMid = (sy + vy) / 2;
                var hShader = SKShader.CreateLinearGradient(new SKPoint(xMid, yMid), new SKPoint(hx, hy),
                    new SKColor[]
                    {
                        SKColor.FromHsv(hue,100,100,0),
                        SKColor.FromHsv(hue,100,100)
                    }, SKShaderTileMode.Clamp);


                xMid = (hx + sx) / 2;
                yMid = (hy + sy) / 2;
                var wShader = SKShader.CreateLinearGradient(new SKPoint(vx, vy), new SKPoint(xMid, yMid),
                    new SKColor[]
                    {
                        SKColors.White,
                        SKColor.Parse("#00FFFFFF")
                    }, SKShaderTileMode.Clamp);

                paint.Shader = colShader;
                paint.Style = SKPaintStyle.StrokeAndFill;

                skDC.DrawPath(path, paint);


                paint.Shader = hShader;
                skDC.DrawPath(path, paint);

                paint.Shader = wShader;
                paint.BlendMode = SKBlendMode.Plus;
                skDC.DrawPath(path, paint);

                wShader.Dispose();
                hShader.Dispose();
                colShader.Dispose();


                ///////////////

                //var colShader = SKShader.CreateColor(SKColor.FromHsv(hue, 100, 100));
                //var xMid = (hx + vx) / 2;
                //var yMid = (hy + vy) / 2;
                //var bShader = SKShader.CreateLinearGradient(new SKPoint(sx, sy), new SKPoint(xMid, yMid),
                //    new SKColor[]
                //    {
                //        SKColors.Black,
                //        SKColors.Transparent
                //    }, SKShaderTileMode.Clamp);

                //xMid = (hx + sx) / 2;
                //yMid = (hy + sy) / 2;
                //var wShader = SKShader.CreateLinearGradient(new SKPoint(vx, vy), new SKPoint(xMid, yMid),
                //    new SKColor[]
                //    {
                //        SKColors.White,
                //        SKColor.Parse("#00FFFFFF")
                //    }, SKShaderTileMode.Clamp);

                //paint.Shader = colShader;
                //paint.Style = SKPaintStyle.StrokeAndFill;

                //skDC.DrawPath(path, paint);

                //paint.Shader = bShader;
                //skDC.DrawPath(path, paint);

                //paint.Shader = wShader;
                //paint.BlendMode = SKBlendMode.Lighten;
                //skDC.DrawPath(path, paint);

                //var tmp = SKShader.CreateColor(SKColors.Transparent);
                //paint.Shader = tmp;
                //paint.BlendMode = SKBlendMode.Src;
                //paint.Style = SKPaintStyle.Stroke;

                ////skDC.DrawPath(path, paint);

                //tmp.Dispose();
                //wShader.Dispose();
                //bShader.Dispose();
                //colShader.Dispose();
                path.Dispose();

                paint.Dispose();
            }
        }

        private void RenderSelectorRects(DrawingContext context, double width, double height)
        {
            double x = 0;
            double y = 0;
            Pen p1 = new Pen(Brushes.White, 3);
            Pen p2 = new Pen(Brushes.Black, 1);
            Color2 color = Color;
            switch (Component)
            {
                case ColorComponent.Hue:
                    x = width * color.Valuef;
                    y = height * (1 - color.Saturationf);
                    break;

                case ColorComponent.Saturation:
                    x = width * color.Valuef;
                    y = height * (1 - (Hue / 360d));
                    break;

                case ColorComponent.Value:
                    x = width * color.Saturationf;
                    y = height * (1 - (Hue / 360d));
                    break;

                case ColorComponent.Red:
                    x = width * color.Bf;
                    y = height * (1 - color.Gf);
                    break;

                case ColorComponent.Green:
                    x = width * color.Bf;
                    y = height * (1 - color.Rf);
                    break;

                case ColorComponent.Blue:
                    x = width * color.Gf;
                    y = height * (1 - color.Rf);
                    break;
            }

            var col = Color;
            var rg = col.R <= 10 ? col.R / 3294.0 : Math.Pow(col.R / 269.0 + 0.0513, 2.4);
            var gg = col.G <= 10 ? col.G / 3294.0 : Math.Pow(col.G / 269.0 + 0.0513, 2.4);
            var bg = col.B <= 10 ? col.B / 3294.0 : Math.Pow(col.B / 269.0 + 0.0513, 2.4);
            var light = 0.2126 * rg + 0.7152 * gg + 0.0722 * bg;

            //Alt would be to calculate HSL Lightness from HSV
            //Simpler, but may not work as nicely
            //var light = Color.Valuef * (1 - (Color.Saturationf / 2f));

            context.DrawRectangle(new Pen(light > 0.5 ? Brushes.Black : Brushes.White, 2), new Rect(x - 5, y - 5, 10, 10), 5f);

            //context.DrawLine(p1, new Point(x, 0), new Point(x, height));
            //context.DrawLine(p1, new Point(0, y), new Point(width, y));

            //context.DrawLine(p2, new Point(x, 0), new Point(x, height));
            //context.DrawLine(p2, new Point(0, y), new Point(width, y));
        }

        private void RenderWheelSelector(DrawingContext context)
        {
            var hue = Color.Huef;
            var sat = Color.Saturationf;
            var radius = _lastWheelRect.Width / 2;

            var x = _lastWheelRect.Center.X + (radius * sat) * MathF.Cos(hue * MathF.PI / 180);
            var y = _lastWheelRect.Center.Y - (radius * sat) * MathF.Sin(hue * MathF.PI / 180);

            //From WinUI (ColorSpectrum.cpp - SelectionEllipseShouldBeLight)
            var col = Color;
            var rg = col.R <= 10 ? col.R / 3294.0 : Math.Pow(col.R / 269.0 + 0.0513, 2.4);
            var gg = col.G <= 10 ? col.G / 3294.0 : Math.Pow(col.G / 269.0 + 0.0513, 2.4);
            var bg = col.B <= 10 ? col.B / 3294.0 : Math.Pow(col.B / 269.0 + 0.0513, 2.4);
            var light = 0.2126 * rg + 0.7152 * gg + 0.0722 * bg;

            //Alt would be to calculate HSL Lightness from HSV
            //Simpler, but may not work as nicely
            //var light = Color.Valuef * (1 - (Color.Saturationf / 2f));

            context.DrawRectangle(new Pen(light > 0.5 ? Brushes.Black : Brushes.White, 2), new Rect(x - 5, y - 5, 10, 10), 5f);
        }

        private void RenderTriangleSelector(DrawingContext context)
        {
            //Apply scale to TriangleWheelThickness
            var wheelThicc = TriangleWheelThickness * (_lastWheelRect.Width / 500);
            var rect = _lastWheelRect.Inflate(-(wheelThicc + WheelPadding / 2));
            var radius = (rect.Width / 2);// - wheelThicc;
            Color.GetAHSVf(out float h, out float s, out float v, out float _);
            h *= MathF.PI / 180;
            var hx = rect.Center.X + radius * MathF.Cos(h);
            var hy = rect.Center.Y - radius * MathF.Sin(h);
            //var third = MathF.PI * 2f / 3;

            //TODO SIMPLIFY
            //Adapted from https://stackoverflow.com/questions/58222353/the-formulars-in-hsv-triangle
            //var x = hx * s * v + (rect.Center.X + radius * MathF.Cos(h + third)) * (1 - s) *
            //    v + (rect.Center.X + radius * MathF.Cos(h - third)) * (1 - v);
            //var y = hy * s * v + (rect.Center.Y - radius * MathF.Sin(h + third)) * (1 - s) *
            //    v + (rect.Center.Y - radius * MathF.Sin(h - third)) * (1 - v);

            //Draw line - uses hx,hy
            context.DrawLine(new Pen(Brushes.White, 1.5), new Point(hx, hy),
                new Point(rect.Center.X + (radius + wheelThicc) * MathF.Cos(h),
                rect.Center.Y - (radius + wheelThicc) * MathF.Sin(h)));

            //Draw ellipse
            var x = (3 * s * v / 2) - 0.5;
            var y = ((Math.Sqrt(3) / 2) * s * v) - (v * Math.Sqrt(3)) + (Math.Sqrt(3) / 2);

            var pt = new Point(x * radius, y * radius) * Matrix.CreateRotation(-h) * Matrix.CreateTranslation(_lastWheelRect.Center);

            context.DrawRectangle(new Pen(Brushes.Black, 2), new Rect(pt.X - 4, pt.Y - 4, 8, 8), 8);
        }

        private void RenderTriangle(DrawingContext context)
        {
            var rect = _lastWheelRect.Inflate(-TriangleWheelThickness / 2 + WheelPadding);

            var radius = rect.Width / 2;

            Color.GetAHSVf(out float h, out float s, out float v, out float _);
            h *= MathF.PI / 180;
            float third = MathF.PI * (2f / 3f);
            var hx = rect.Center.X + radius * MathF.Cos(h);
            var hy = rect.Center.Y - radius * MathF.Sin(h);

            var sx = rect.Center.X + radius * MathF.Cos(h - third);
            var sy = rect.Center.Y - radius * MathF.Sin(h - third);

            var vx = rect.Center.X + radius * MathF.Cos(h + third);
            var vy = rect.Center.Y - radius * MathF.Sin(h + third);

            StreamGeometry geom = StreamGeometry.Parse($"M{hx},{hy} L {sx} {sy} L {vx} {vy} Z");

            context.DrawGeometry(new SolidColorBrush(Color), null, geom);

            var xMid = (hx + vx) / 2;
            var yMid = (hy + vy) / 2;
            LinearGradientBrush bLGB = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(sx, sy, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(xMid, yMid, RelativeUnit.Absolute),
                GradientStops =
                {
                    new GradientStop(Colors.Black, 0),
                    new GradientStop(Colors.Transparent, 1)
                }
            };

            context.DrawGeometry(bLGB, null, geom);

            xMid = (hx + sx) / 2;
            yMid = (hy + sy) / 2;
            LinearGradientBrush wLGB = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(vx, vy, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(xMid, yMid, RelativeUnit.Absolute),
                GradientStops =
                {
                    new GradientStop(Colors.White, 0),
                    new GradientStop(AvColor.FromArgb(0,255,255,255), 1)
                }
            };

            context.DrawGeometry(wLGB, null, geom);
        }

        private void SetColorFromPosition(Point pt)
        {
            if (Shape == ColorSpectrumShape.Spectrum)
            {
                var rect = new Rect(Bounds.Size);

                var pX = pt.X / rect.Width;
                if (pX < 0)
                    pX = 0;
                if (pX > 1)
                    pX = 1;
                var pY = pt.Y / rect.Height;
                if (pY < 0)
                    pY = 0;
                if (pY > 1)
                    pY = 1;

                switch (Component)
                {
                    case ColorComponent.Hue:
                        Color = Color2.FromHSV(Hue, (float)(1 - pY), (float)pX);
                        break;
                    case ColorComponent.Saturation:
                        Color = Color2.FromHSV((float)(1 - pY) * 360, Color.Saturationf, (float)pX);
                        break;
                    case ColorComponent.Value:
                        Color = Color2.FromHSV((float)(1 - pY) * 360, (float)pX, Color.Valuef);
                        break;

                    case ColorComponent.Red:
                        Color = Color2.FromRGB(Color.Rf, (float)(1 - pY), (float)pX, Color.Af);
                        break;
                    case ColorComponent.Green:
                        Color = Color2.FromRGB((float)(1 - pY), Color.Gf, (float)(pX), Color.Af);
                        break;
                    case ColorComponent.Blue:
                        Color = Color2.FromRGB((float)(1 - pY), (float)pX, Color.Bf, Color.Af);
                        break;
                }
            }
            else if (Shape == ColorSpectrumShape.Wheel)
            {
                var dp = pt - _lastWheelRect.Center;
                var theta = Math.Atan2(-dp.Y, dp.X) * 180 / Math.PI;
                if (theta < 0)
                    theta += 360;

                var dist = Math.Clamp(Math.Sqrt(dp.X * dp.X + dp.Y * dp.Y) / (_lastWheelRect.Width / 2), 0, 1);
                Color = Color2.FromHSV((float)Math.Clamp(theta, 0, 360), (float)dist, Color.Valuef);
            }
            else if (Shape == ColorSpectrumShape.Triangle)
            {
                //First determine if we're in the wheel, triangle, or nothing
                var dist = Math.Sqrt(Math.Pow(pt.X - _lastWheelRect.Center.X, 2) + Math.Pow(pt.Y - _lastWheelRect.Center.Y, 2));
                var radius = _lastWheelRect.Width / 2;
                var wheelThicc = TriangleWheelThickness * (_lastWheelRect.Width / 500);
                var innerRadius = radius - wheelThicc - 2;

                Color.GetAHSVf(out float h, out float s, out float v, out float _);
                h *= MathF.PI / 180;
                var sf = 1 - _lastWheelRect.Width / _defBitmapSize;
                if (_isDownWheel || ((dist >= innerRadius && dist <= radius) && !_isDownTriangle))
                {
                    var newHue = 360 - (Math.Atan2(pt.Y - _lastWheelRect.Center.Y, pt.X - _lastWheelRect.Center.X) * 180 / Math.PI);
                    if (newHue < 0)
                        newHue += 360;
                    Color = Color.WithHuef((float)newHue);

                    if (_isDown && !_isDownWheel)
                        _isDownWheel = true;
                }
                else
                {
                    //Normalize points to Unit Circle & rotate accounting for hue & placing H=0 at top
                    //See below for more on this reasoning
                    //Then we know the coordinates of the triangle from the unit circle & we don't need
                    //to calculate them...
                    var norm = (pt - _lastWheelRect.Center) / innerRadius;
                    norm *= Matrix.CreateRotation(-Math.PI / 2 + h);

                    if (_isDownTriangle || PointInTriangle(norm, new Point(0, -1), new Point(Math.Sqrt(3) / 2, 1 / 2), new Point(-Math.Sqrt(3) / 2, 1 / 2)))
                    {
                        //Couple of hacks here for the time being...
                        //The example from SO this is based on directs the triangle up (H=0 at top of wheel)
                        //Whereas I direct it right
                        //Since I can't correctly get the equations to work out, we rotate the pointer coordinate
                        //-90° and use the equations from the example
                        //S & V are also switched so multiply x by -1

                        //My equations (these are correct) (used in drawing selection indicator)
                        //x = (3 * s * v) / 2 - 0.5
                        //y = sqrt(3)/2 * s * v - v * sqrt(3) + sqrt(3)/2

                        //But I have been unable to solve for s & v correctly with these, so...
                        //But I'm done for now, I've spent 2 days trying to solve this and I need
                        //to move on to other things so yea...

                        var newS = (1 - (2 * norm.Y)) / ((Math.Sqrt(3) * (norm.X * -1)) - norm.Y + 2);
                        var newV = (Math.Sqrt(3) * (norm.X * -1) - norm.Y + 2) / 3;

                        newV = MathF.Max(0, MathF.Min(1, (float)newV));
                        newS = MathF.Max(0, MathF.Min(1, (float)newS));
                        Color = Color2.FromHSV(h * 180f / MathF.PI, (float)newS, (float)newV);
                        _isDownTriangle = true;
                    }
                    //else //IGNORE (outside wheel & triangle)
                    //{
                    //}
                }
            }
        }

        public static bool PointInTriangle(Point p, Point p0, Point p1, Point p2)
        {
            var s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
            var t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;

            return A < 0 ?
                    (s <= 0 && s + t >= A) :
                    (s >= 0 && s + t <= A);
        }

        private void HandleUpDownKey(KeyEventArgs e)
        {
            bool inc = e.Key == Key.Up;
            var col = Color;
            if (Shape == ColorSpectrumShape.Spectrum)
            {
                //The "with[..]f" methods scale the values there so we don't 
                //need to check if we're in a valid range
                switch (Component)
                {
                    case ColorComponent.Hue: //Adjust saturation
                        Color = col.WithSatf(col.Saturationf + (inc ? 0.01f : -0.01f));
                        break;
                    case ColorComponent.Saturation: //Adjust hue
                    case ColorComponent.Value:
                        Color = col.WithHuef(col.Huef + (inc ? 1f : -1f));
                        break;
                    case ColorComponent.Red://Adjust green
                        Color = col.WithGreenf(col.Gf + (inc ? 0.01f : -0.01f));
                        break;
                    case ColorComponent.Green: //Adjust red
                    case ColorComponent.Blue:
                        Color = col.WithRedf(col.Rf + (inc ? 0.01f : -0.01f));
                        break;
                }
            }
            else if (Shape == ColorSpectrumShape.Wheel)
            {
                //TODO
            }
            else if (Shape == ColorSpectrumShape.Triangle)
            {
                //For simplicity, we'll map up/down to value
                //since v = 0 is the bottom corner of the triangle
                //If ctrl + up/down is used, we'll adjust the hue

                if ((KeyModifiers.Control & e.KeyModifiers) == KeyModifiers.Control)
                {
                    var h = col.Huef + (inc ? 1f : -1f);
                    if (h < 0)
                        h += 360;

                    Color = col.WithHuef(h);
                }
                else
                {
                    Color = col.WithValf(col.Valuef + (inc ? 0.01f : -0.01f));
                }
            }
            e.Handled = true;
        }

        private void HandleLeftRightKey(KeyEventArgs e)
        {
            bool inc = e.Key == Key.Right;
            var col = Color;
            if (Shape == ColorSpectrumShape.Spectrum)
            {
                //The "with[..]f" methods scale the values there so we don't 
                //need to check if we're in a valid range
                switch (Component)
                {
                    case ColorComponent.Hue: //Adjust value
                    case ColorComponent.Saturation:
                        Color = col.WithValf(col.Valuef + (inc ? 0.01f : -0.01f));
                        break;
                    case ColorComponent.Value: //Adjust saturation
                        Color = col.WithSatf(col.Saturationf + (inc ? 0.01f : -0.01f));
                        break;

                    case ColorComponent.Red: //Adjust blue
                    case ColorComponent.Green:
                        Color = col.WithBluef(col.Bf + (inc ? 0.01f : -0.01f));
                        break;
                    case ColorComponent.Blue: //Adjust green
                        Color = col.WithGreenf(col.Gf + (inc ? 0.01f : -0.01f));
                        break;
                }
            }
            else if (Shape == ColorSpectrumShape.Wheel)
            {

            }
            else if (Shape == ColorSpectrumShape.Triangle)
            {
                //For simplicity, we'll map left/right to saturation
                //since s increases left to right (assuming H=0, right is top of triangle)
                //If ctrl + up/down is used, we'll adjust the hue
                if ((KeyModifiers.Control & e.KeyModifiers) == KeyModifiers.Control)
                {
                    //Because of the direction of the wheel, we'll invert the key to 
                    //increase the hue (left increases it)
                    var h = col.Huef + (!inc ? 1f : -1f);
                    if (h < 0)
                        h += 360;

                    Color = col.WithHuef(h);
                }
                else
                {
                    Color = col.WithSatf(col.Saturationf + (inc ? 0.01f : -0.01f));
                }
            }
            e.Handled = true;
        }


        #endregion

        private bool _isDown;
        private bool _isDownWheel;
        private bool _isDownTriangle;
        private readonly int _defBitmapSize = 500;
        private bool _triangleDirty;

        //private RenderTargetBitmap _bitmap1;
        private IBitmap _tempBitmap;
        //private RenderTargetBitmap _triangleBitmap;
        private ColorSpectrumShape _shape = ColorSpectrumShape.Spectrum;
        private Rect _lastWheelRect;
        private readonly double WheelPadding = 4;
        private readonly float TriangleWheelThickness = 50f;

        //private ColorSpectrumComponents _components;

        ///////////////////////////////////////////////////////////////////////


        //public override void Render(DrawingContext context)
        //{
        //    if (_bitmap1 == null)
        //        CreateBitmaps();

        //    var minDim = Math.Min(Bounds.Width, Bounds.Height);

        //    Rect rect;
        //    if (Shape == ColorSpectrumShape.Wheel)
        //    {
        //        rect = new Rect(Bounds.Width / 2 - minDim / 2,
        //        Bounds.Height / 2 - minDim / 2, minDim, minDim);

        //        RenderWheel(context, rect);
        //    }
        //    else
        //    { 
        //        rect = new Rect(0, 0, Bounds.Width, Bounds.Height);

        //        context.DrawImage(_bitmap1, new Rect(0, 0, _defBitmapSize, _defBitmapSize), rect, 
        //            Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);

        //        RenderSelectorRects(context, rect.Width, rect.Height);

        //        using (context.PushClip(new Rect(0, 0, Bounds.Width, Bounds.Height)))
        //            context.DrawRectangle(null, new Pen(Brushes.Gray, 1), rect);
        //    }
        //}

        //protected override void OnPointerPressed(PointerPressedEventArgs e)
        //{
        //    base.OnPointerPressed(e);

        //    var cp = e.GetCurrentPoint(this);

        //    if (cp.Properties.IsLeftButtonPressed)
        //    {
        //        SetColorFromPosition(cp.Position);

        //        _isDown = true;
        //    }

        //}

        //protected override void OnPointerMoved(PointerEventArgs e)
        //{
        //    base.OnPointerMoved(e);
        //    if (_isDown)
        //    {
        //        SetColorFromPosition(e.GetPosition(this));
        //    }
        //}

        //protected override void OnPointerReleased(PointerReleasedEventArgs e)
        //{
        //    base.OnPointerReleased(e);
        //    if (_isDown)
        //    {
        //        _isDown = false;
        //        SetColorFromPosition(e.GetPosition(this));
        //    }
        //}

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    switch (e.Key)
        //    {
        //        case Key.Left:
        //        case Key.Right:
        //            HandleLeftRightArrowKey(e.Key == Key.Right);
        //            e.Handled = true;
        //            break;

        //        case Key.Up:
        //        case Key.Down:
        //            HandleUpDownArrowKey(e.Key == Key.Up);
        //            e.Handled = true;
        //            break;
        //    }

        //    base.OnKeyDown(e);
        //}

        //private void HandleUpDownArrowKey(bool up)
        //{
        //    if (Shape == ColorSpectrumShape.Wheel)
        //    {
        //        //TODO
        //    }
        //    else
        //    {
        //        switch (Components) 
        //        {
        //            case ColorSpectrumComponents.SaturationValue:
        //                Color = Color2.FromHSV(_color.Hue, up ? _color.Saturation + 0.01f : _color.Saturation - 0.01f, _color.Value);
        //                break;
        //            case ColorSpectrumComponents.SaturationHue:
        //            case ColorSpectrumComponents.ValueHue:
        //                Color = Color2.FromHSV(up ? _color.Hue + 1 : _color.Hue - 1, _color.Saturation, _color.Value);
        //                break;

        //            case ColorSpectrumComponents.BlueGreen:
        //                Color = new Color2(_color.A, _color.R, up ? (byte)(_color.G + 1) : (byte)(_color.G - 1), _color.B);
        //                break;
        //            case ColorSpectrumComponents.BlueRed:
        //                Color = new Color2(_color.A, up ? (byte)(_color.R + 1) : (byte)(_color.R - 1), _color.G, _color.B);
        //                break;
        //            case ColorSpectrumComponents.GreenRed:
        //                Color = new Color2(_color.A, _color.R, up ? (byte)(_color.B + 1) : (byte)(_color.B - 1));
        //                break;
        //        }
        //    }
        //}

        //private void HandleLeftRightArrowKey(bool right)
        //{
        //    if (Shape == ColorSpectrumShape.Wheel)
        //    {
        //        //TODO
        //    }
        //    else
        //    {
        //        switch (Components)
        //        {
        //            case ColorSpectrumComponents.ValueHue:
        //            case ColorSpectrumComponents.SaturationValue:
        //                Color = Color2.FromHSV(_color.Hue, _color.Saturation, right ? _color.Value + 0.01f : _color.Value - 0.01f);
        //                break;
        //            case ColorSpectrumComponents.SaturationHue:
        //                Color = Color2.FromHSV(_color.Hue, right ? _color.Saturation + 0.01f : _color.Saturation - 0.01f, _color.Value);
        //                break;

        //            case ColorSpectrumComponents.BlueRed:
        //            case ColorSpectrumComponents.BlueGreen:
        //                Color = new Color2(_color.A, _color.R, _color.G, right ? (byte)(_color.R + 1) : (byte)(_color.R - 1));
        //                break;
        //            case ColorSpectrumComponents.GreenRed:
        //                Color = new Color2(_color.A, _color.R, right ? (byte)(_color.G + 1) : (byte)(_color.G - 1), _color.B);
        //                break;
        //        }
        //    }
        //}

        ////NOTE (if desired later on)
        ////https://github.com/qgis/QGIS/blob/master/src/gui/qgscolorwidgets.cpp
        ////https://github.com/timjb/colortriangle/blob/master/colortriangle.js by Tim Baumann
        ////For doing the triangle color picker...
        //private void CreateBitmaps()
        //{
        //    _bitmap1?.Dispose();


        //    _bitmap1 = new RenderTargetBitmap(new PixelSize(500,500));

        //    SKRect rect = SKRect.Create(_bitmap1.PixelSize.Width, _bitmap1.PixelSize.Height);

        //    using (var ctx = _bitmap1.CreateDrawingContext(null))
        //    {
        //        var skDC = (ctx as ISkiaDrawingContextImpl).SkCanvas;

        //        DrawSaturationValueBitmap(ref skDC, rect);
        //        //DrawValueHueBitmap(ref skDC, rect);
        //        //DrawSaturationHueBitmap(ref skDC, rect);
        //        //DrawBlueGreenBitmap(ref skDC, rect);
        //        //DrawBlueRedBitmap(ref skDC, rect);
        //        //DrawGreenRedBitmap(ref skDC, rect);


        //        //if(Shape == ColorSpectrumShape.Spectrum)
        //        //{
        //        //    DrawWheelBitmap(ref skDC, new SKRect(0, 0, 500, 500));
        //        //}
        //        //else
        //        //{
        //        //    switch (Components)
        //        //    {
        //        //        case ColorSpectrumComponents.SaturationHue:
        //        //        case ColorSpectrumComponents.SaturationValue:
        //        //        case ColorSpectrumComponents.ValueHue:
        //        //            DrawHSVSpectrum(ref skDC, rect);
        //        //            break;

        //        //        case ColorSpectrumComponents.BlueGreen:
        //        //        case ColorSpectrumComponents.BlueRed:
        //        //        case ColorSpectrumComponents.GreenRed:
        //        //            //DrawRGBSpectrum(ref skDC, rect);
        //        //            break;
        //        //    }
        //        //}


        //        skDC.Dispose();

        //        //CODE FOR RGB BITMAPS...

        //        //SKBitmap bmp = new SKBitmap(500, 500, true);

        //        //Stopwatch sw = new Stopwatch();
        //        //sw.Start();
        //        //unsafe
        //        //{
        //        //    var pixels = (uint*)(void*)bmp.GetPixels();
        //        //    var count = bmp.Width * bmp.Height;

        //        //    uint x = 0;
        //        //    uint y = 0;
        //        //    for(int i = 0; i < count; i++)
        //        //    {
        //        //        var red = (x / 500.0) * 255;
        //        //        var green = 255 - (y / 500.0) * 255;
        //        //        //var col = new byte[] { 0xFF, red << 16, green << 8, 255 };

        //        //        //pixels[i] = (uint)Color2.PaleVioletRed.ToARGB();
        //        //        pixels[i] = (uint) (0xFF << 24 | (uint)red << 16 | (uint)green << 8 | 0x00);
        //        //        //pixels[i] = (uint)new Color2(255, (x / 500) * 255, 255 - ((y/500)*255), 255).ToARGB();
        //        //        x++;
        //        //        if(x % 500 == 0)
        //        //        {
        //        //            y++;
        //        //            x = 0;
        //        //        }
        //        //        //Debug.WriteLine($"{red}, {green}");
        //        //    }
        //        //}



        //        //sw.Stop();
        //        //Debug.WriteLine($"{sw.Elapsed}");
        //        //skDC.DrawBitmap(bmp, new SKPoint(0, 0));
        //        //bmp.Dispose();
        //        //bmp = null;




        //    }
        //}

        //private void DrawWheelBitmap(ref SKCanvas skDC, SKRect rect)
        //{
        //    SKPoint center = new SKPoint(rect.MidX, rect.MidY);
        //    float radius = rect.Width / 2f;

        //    SKShader spectrum = SKShader.CreateSweepGradient(center,
        //        new SKColor[]
        //        {
        //            new SKColor(255, 0, 0),
        //            new SKColor(255, 0, 255),
        //            new SKColor(0, 0, 255),
        //            new SKColor(0, 255, 255),
        //            new SKColor(0, 255, 0),
        //            new SKColor(255, 255, 0),
        //            new SKColor(255, 0, 0)
        //        });

        //    SKPaint paint = new SKPaint();
        //    paint.Shader = spectrum;
        //    paint.Style = SKPaintStyle.StrokeAndFill;
        //    paint.StrokeWidth = 2f;
        //    paint.IsAntialias = true;

        //    skDC.DrawCircle(center, radius, paint);

        //    paint.Dispose();
        //    spectrum.Dispose();

        //    var grad = SKShader.CreateRadialGradient(center, radius,
        //        new SKColor[] { SKColor.Parse("#FFFFFFFF"), SKColor.Parse("#00FFFFFF") },
        //        new float[] { 0, 1 }, SKShaderTileMode.Clamp);

        //    paint = new SKPaint();
        //    paint.Shader = grad;
        //    paint.Style = SKPaintStyle.Fill;
        //    paint.IsAntialias = true;

        //    skDC.DrawCircle(center, radius, paint);
        //    paint.Dispose();
        //    grad.Dispose();
        //}

        //private void DrawSaturationValueBitmap(ref SKCanvas skDC, SKRect rect)
        //{            
        //    //To Follow QGIS, this should be named ValueSaturation (x,y)
        //    using (var bmp = new SKBitmap(_defBitmapSize, _defBitmapSize, true))
        //    {
        //        Stopwatch sw = Stopwatch.StartNew();
        //        float hue = _color.Hue;
        //        unsafe
        //        {
        //            var pixels = (uint*)(void*)bmp.GetPixels();
        //            var count = _defBitmapSize * _defBitmapSize;
        //            uint x = 0;
        //            uint y = 0;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var value = (x / 500f);
        //                var sat = 1 - (y / 500f);

        //                var col = SKColor.FromHsv(hue, sat * 100, value * 100);
        //                pixels[i] = (uint)(0xFF << 24 | col.Red << 16 | col.Green << 8 | col.Blue);
        //                x++;
        //                if (x == _defBitmapSize)
        //                {
        //                    y++;
        //                    x = 0;
        //                }
        //            }
        //        }
        //        skDC.DrawBitmap(bmp, new SKPoint(0, 0));
        //        sw.Stop();
        //        Debug.WriteLine($"Bitmap build time {sw.ElapsedMilliseconds}");                
        //    }

        //}

        //private void DrawValueHueBitmap(ref SKCanvas skDC, SKRect rect)
        //{
        //    using (var bmp = new SKBitmap(_defBitmapSize, _defBitmapSize, true))
        //    {
        //        Stopwatch sw = Stopwatch.StartNew();
        //        float sat = _color.Saturation*100;
        //        unsafe
        //        {
        //            var pixels = (uint*)(void*)bmp.GetPixels();
        //            var count = _defBitmapSize * _defBitmapSize;
        //            uint x = 0;
        //            uint y = 0;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var value = (x / 500f);
        //                var hue = 360 - ((y / 500f) * 360);

        //                var col = SKColor.FromHsv(hue, sat, value * 100);
        //                pixels[i] = (uint)(0xFF << 24 | col.Red << 16 | col.Green << 8 | col.Blue);
        //                x++;
        //                if (x == _defBitmapSize)
        //                {
        //                    y++;
        //                    x = 0;
        //                }
        //            }
        //        }
        //        skDC.DrawBitmap(bmp, new SKPoint(0, 0));
        //        sw.Stop();
        //        Debug.WriteLine($"Bitmap build time {sw.ElapsedMilliseconds}");
        //    }
        //}

        //private void DrawSaturationHueBitmap(ref SKCanvas skDC, SKRect rect)
        //{
        //    using (var bmp = new SKBitmap(_defBitmapSize, _defBitmapSize, true))
        //    {
        //        Stopwatch sw = Stopwatch.StartNew();
        //        float value = _color.Value*100;
        //        unsafe
        //        {
        //            var pixels = (uint*)(void*)bmp.GetPixels();
        //            var count = _defBitmapSize * _defBitmapSize;
        //            uint x = 0;
        //            uint y = 0;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var sat = (x / 500f);
        //                var hue = 360 - ((y / 500f) * 360);

        //                var col = SKColor.FromHsv(hue, sat * 100, value);
        //                pixels[i] = (uint)(0xFF << 24 | col.Red << 16 | col.Green << 8 | col.Blue);
        //                x++;
        //                if (x == _defBitmapSize)
        //                {
        //                    y++;
        //                    x = 0;
        //                }
        //            }
        //        }
        //        skDC.DrawBitmap(bmp, new SKPoint(0, 0));
        //        sw.Stop();
        //        Debug.WriteLine($"Bitmap build time {sw.ElapsedMilliseconds}");
        //    }
        //}

        //private void DrawBlueGreenBitmap(ref SKCanvas skDC, SKRect rect)
        //{
        //    using (var bmp = new SKBitmap(_defBitmapSize, _defBitmapSize, true))
        //    {
        //        Stopwatch sw = Stopwatch.StartNew();
        //        uint red = (uint)_color.R;
        //        unsafe
        //        {
        //            var pixels = (uint*)(void*)bmp.GetPixels();
        //            var count = _defBitmapSize * _defBitmapSize;
        //            uint x = 0;
        //            uint y = 0;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var blue = (x / 500f) * 255;
        //                var green = 255 - ((y / 500f) * 255);

        //                pixels[i] = (uint)(0xFF << 24 | red << 16 | (uint)green << 8 | (uint)blue);
        //                x++;
        //                if (x == _defBitmapSize)
        //                {
        //                    y++;
        //                    x = 0;
        //                }
        //            }
        //        }
        //        skDC.DrawBitmap(bmp, new SKPoint(0, 0));
        //        sw.Stop();
        //        Debug.WriteLine($"Bitmap build time {sw.ElapsedMilliseconds}");
        //    }
        //}

        //private void DrawBlueRedBitmap(ref SKCanvas skDC, SKRect rect)
        //{
        //    using (var bmp = new SKBitmap(_defBitmapSize, _defBitmapSize, true))
        //    {
        //        Stopwatch sw = Stopwatch.StartNew();
        //        uint green = (uint)_color.G;
        //        unsafe
        //        {
        //            var pixels = (uint*)(void*)bmp.GetPixels();
        //            var count = _defBitmapSize * _defBitmapSize;
        //            uint x = 0;
        //            uint y = 0;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var blue = (x / 500f) * 255;
        //                var red = 255 - ((y / 500f) * 255);

        //                pixels[i] = (uint)(0xFF << 24 | (uint)red << 16 | green << 8 | (uint)blue);
        //                x++;
        //                if (x == _defBitmapSize)
        //                {
        //                    y++;
        //                    x = 0;
        //                }
        //            }
        //        }
        //        skDC.DrawBitmap(bmp, new SKPoint(0, 0));
        //        sw.Stop();
        //        Debug.WriteLine($"Bitmap build time {sw.ElapsedMilliseconds}");
        //    }
        //}

        //private void DrawGreenRedBitmap(ref SKCanvas skDC, SKRect rect)
        //{
        //    using (var bmp = new SKBitmap(_defBitmapSize, _defBitmapSize, true))
        //    {
        //        Stopwatch sw = Stopwatch.StartNew();
        //        uint blue = (uint)_color.B;
        //        unsafe
        //        {
        //            var pixels = (uint*)(void*)bmp.GetPixels();
        //            var count = _defBitmapSize * _defBitmapSize;
        //            uint x = 0;
        //            uint y = 0;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var green = (x / 500f) * 255;
        //                var red = 255 - ((y / 500f) * 255);

        //                pixels[i] = (uint)(0xFF << 24 | (uint)red << 16 | (uint)green << 8 | blue);
        //                x++;
        //                if (x == _defBitmapSize)
        //                {
        //                    y++;
        //                    x = 0;
        //                }
        //            }
        //        }
        //        skDC.DrawBitmap(bmp, new SKPoint(0, 0));
        //        sw.Stop();
        //        Debug.WriteLine($"Bitmap build time {sw.ElapsedMilliseconds}");
        //    }
        //}



        //private void RenderWheel(DrawingContext context, Rect rect)
        //{
        //    using (context.PushClip(new Rect(Bounds.Size)))
        //    {
        //        context.FillRectangle(Brushes.Black, rect, (float)rect.Width);

        //        using (context.PushOpacity(_color.Value))
        //            context.DrawImage(_bitmap1, new Rect(0, 0, 500, 500), rect, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
        //    }

        //    var radius = (rect.Width / 2) * _color.Saturation;
        //    var x = rect.Center.X + Math.Cos(_color.Hue * Math.PI / 180) * radius;
        //    var y = rect.Center.Y - Math.Sin(_color.Hue * Math.PI / 180) * radius;

        //    context.DrawRectangle(new Pen(Brushes.Black, 2), new Rect(x - 6, y - 6, 12, 12), 12);
        //}

        //private void RenderSelectorRects(DrawingContext context, double width, double height)
        //{
        //    double x = 0;
        //    double y = 0;
        //    Pen p1 = new Pen(Brushes.White, 3);
        //    Pen p2 = new Pen(Brushes.Black, 1);
        //    switch (Components)
        //    {
        //        case ColorSpectrumComponents.SaturationValue:
        //            x = width * _color.Value;
        //            y = height * (1 - _color.Saturation);

        //            context.DrawLine(p1, new Point(x, 0), new Point(x, height));
        //            context.DrawLine(p1, new Point(0, y), new Point(width, y));

        //            context.DrawLine(p2, new Point(x, 0), new Point(x, height));
        //            context.DrawLine(p2, new Point(0, y), new Point(width, y));

        //            //context.FillRectangle(Brushes.White, new Rect(x - 1, 0, 3, height));
        //            //context.FillRectangle(Brushes.White, new Rect(0, y - 1, width, 3));
        //            //context.FillRectangle(Brushes.Black, new Rect(x, 0, 1, height));
        //            //context.FillRectangle(Brushes.Black, new Rect(0, y, width, 1));
        //            break;



        //    }
        //}


        //private void SetColorFromPosition(Point pt)
        //{
        //    var rect = new Rect(Bounds.Size);
        //    if (Shape == ColorSpectrumShape.Wheel)
        //    {               
        //        var dp = pt - rect.Center;
        //        var theta = Math.Atan2(-dp.Y, dp.X) * 180 / Math.PI;

        //        if (theta < 0)
        //            theta += 360;

        //        var dist = Math.Clamp(Math.Sqrt(dp.X * dp.X + dp.Y * dp.Y) / (rect.Width / 2), 0, 1);

        //        Color = Color2.FromHSV((float)Math.Clamp(theta, 0, 360), (float)dist, _color.Value);
        //    }
        //    else
        //    {
        //        var pX = pt.X / rect.Width;
        //        var pY = pt.Y / rect.Height;
        //        switch (Components)
        //        {
        //            case ColorSpectrumComponents.SaturationValue:
        //                Color = Color2.FromHSV(_color.Hue, (float)(1 - pY), (float)pX);
        //                break;
        //            case ColorSpectrumComponents.ValueHue:
        //                Color = Color2.FromHSV((float)(1 - pY) * 360, _color.Saturation, (float)pX);
        //                break;
        //            case ColorSpectrumComponents.SaturationHue:
        //                Color = Color2.FromHSV((float)(1 - pY) * 360, (float)pX, _color.Value);
        //                break;

        //            case ColorSpectrumComponents.BlueGreen:
        //                Color = new Color2(_color.A, _color.R, (byte)((1 - pY) * 255), (byte)(pX * 255));
        //                break;
        //            case ColorSpectrumComponents.BlueRed:
        //                Color = new Color2(_color.A, (byte)((1 - pY) * 255), _color.G, (byte)(pX * 255));
        //                break;
        //            case ColorSpectrumComponents.GreenRed:
        //                Color = new Color2(_color.A, (byte)((1 - pY) * 255), (byte)(pX * 255), _color.B);
        //                break;
        //        }
        //    }



        //}

        //private void CheckBitmapAndReRender(Color2 oldColor, Color2 newColor, bool defRecreate)
        //{
        //    bool recreateBitmap = defRecreate;
        //    var comp = Components;

        //    //We only want to recreate the bitmap if
        //    // 1- We switch components (defCreate == true)
        //    // 2- If in RGB mode and the 3rd component changes
        //    if (!defRecreate)
        //    {
        //        switch (comp)
        //        {
        //            case ColorSpectrumComponents.BlueGreen:
        //                if (Math.Abs(oldColor.R - newColor.R) > 0.001)
        //                    recreateBitmap = true;
        //                break;
        //            case ColorSpectrumComponents.BlueRed:
        //                if (Math.Abs(oldColor.G - newColor.G) > 0.001)
        //                    recreateBitmap = true;
        //                break;
        //            case ColorSpectrumComponents.GreenRed:
        //                if (Math.Abs(oldColor.B - newColor.B) > 0.001)
        //                    recreateBitmap = true;
        //                break;

        //            case ColorSpectrumComponents.SaturationValue:
        //                if (Math.Abs(oldColor.Hue - newColor.Hue) > 0.001)
        //                    recreateBitmap = true;
        //                break;

        //            case ColorSpectrumComponents.ValueHue:
        //                if (Math.Abs(oldColor.Saturation - newColor.Saturation) > 0.001)
        //                    recreateBitmap = true;
        //                break;

        //            case ColorSpectrumComponents.SaturationHue:
        //                if (Math.Abs(oldColor.Value - newColor.Value) > 0.001)
        //                    recreateBitmap = true;
        //                break;
        //        }
        //    }

        //    if (recreateBitmap)
        //        CreateBitmaps();

        //    InvalidateVisual();
        //}

    }

    public enum ColorSpectrumShape
    {
        Wheel,
        Spectrum,
        Triangle
    }

    //Named by component on X axis, then Y axis
    //Other component is displayed in the slider
    public enum ColorSpectrumComponents
    {
        SaturationValue,
        ValueHue,
        SaturationHue,
        BlueGreen,
        BlueRed,
        GreenRed
    }
}

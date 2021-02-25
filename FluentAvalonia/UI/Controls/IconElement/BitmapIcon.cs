using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class BitmapIcon : IconElement
    {
        ~BitmapIcon()
        {
            Dispose();
            UnlinkFromBitmapIconSource();
        }

        public static readonly DirectProperty<BitmapIcon, Uri> UriSourceProperty =
            AvaloniaProperty.RegisterDirect<BitmapIcon, Uri>("UriSource",
                x => x.UriSource, (x, v) => x.UriSource = v);

        public static readonly DirectProperty<BitmapIcon, bool> ShowAsMonochromeProperty =
            AvaloniaProperty.RegisterDirect<BitmapIcon, bool>("ShowAsMonochrome",
                x => x.ShowAsMonochrome, (x, v) => x.ShowAsMonochrome = v);

        public Uri UriSource
        {
            get => _source;
            set
            {
                if (_bis != null)
                    throw new InvalidOperationException("Cannot edit properties of BitmapIcon if BitmapIconSource is linked");

                SetAndRaise(UriSourceProperty, ref _source, value);
                CreateBitmap();
                InvalidateVisual();
            }
        }

        public bool ShowAsMonochrome
        {
            get => _showAsMonochrome;
            set
            {
                if (_bis != null)
                    throw new InvalidOperationException("Cannot edit properties of BitmapIcon if BitmapIconSource is linked");

                SetAndRaise(ShowAsMonochromeProperty, ref _showAsMonochrome, value);
                InvalidateVisual();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_bis != null)
                return _originalSize;

            if (_bitmap == null || UriSource == null)
                return base.MeasureOverride(availableSize);

            return _originalSize;
        }

        public override void Render(DrawingContext context)
        {
            if (_bitmap == null && _bis == null)
                return;

            var dst = new Rect(Bounds.Size);
            //RTB will throw ArgumentException if height or width isn't at least 1, so
            //don't draw anything if we don't meet that requirement
            if (dst.Width < 1 || dst.Height < 1)
                return;

            var wid = (int)dst.Width;
            var hei = (int)dst.Height;

            using (var bmp = new RenderTargetBitmap(new PixelSize(wid, hei)))
            using (var ctx = bmp.CreateDrawingContext(null))
            {
                using (var skDC = (ctx as ISkiaDrawingContextImpl).SkCanvas)
                {
                    skDC.Clear(new SKColor(0, 0, 0, 0));

                    var finalBmp = _bitmap.Resize(new SKImageInfo(wid, hei), SKFilterQuality.High);

                    if (ShowAsMonochrome)
                    {
                        var avColor = Foreground is ISolidColorBrush sc ? sc.Color : Colors.White;

                        var color = new SKColor(avColor.R, avColor.G, avColor.B, avColor.A);
                        SKPaint paint = new SKPaint();
                        paint.ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcATop);

                        skDC.DrawBitmap(finalBmp, new SKRect(0, 0, (float)wid, (float)hei), paint);
                        paint.Dispose();
                    }
                    else
                    {
                        skDC.DrawBitmap(finalBmp, new SKRect(0, 0, (float)wid, (float)hei));
                    }

                    finalBmp.Dispose();
                }

                
                using (context.PushClip(dst))
                {
                    context.DrawImage(bmp, new Rect(bmp.Size), dst, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
                }

            }
        }

        private void CreateBitmap()
        {
            if (_bis != null)
                return;

            Dispose();

            if (_source == null)
                return;

            if (_source.IsAbsoluteUri && _source.IsFile)
            {
                _bitmap = SKBitmap.Decode(_source.LocalPath);
            }
            else
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                _bitmap = SKBitmap.Decode(assets.Open(_source));
            }
            _originalSize = new Size(_bitmap.Width, _bitmap.Height);
        }

        protected void Dispose()
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _originalSize = Size.Empty;
        }

        internal void LinkToBitmapIconSource(BitmapIconSource bis)
        {
            if (bis == null)
                throw new ArgumentNullException("BitmapIconSource", "BitmapIconSource cannot be null");
            _bis = bis;
            OnLinkedBitmapIconSourceChanged(null, null);
            bis.OnBitmapChanged += OnLinkedBitmapIconSourceChanged;
        }

        internal void UnlinkFromBitmapIconSource()
        {
            if (_bis != null)
                _bis.OnBitmapChanged -= OnLinkedBitmapIconSourceChanged;
            _bis = null;
        }

        private void OnLinkedBitmapIconSourceChanged(object sender, object e)
        {
            Dispose();
            _bitmap = _bis._bitmap;
            _originalSize = _bis.Size;
        }

        private BitmapIconSource _bis;
        protected SKBitmap _bitmap;
        private Size _originalSize;
        private Uri _source;
        private bool _showAsMonochrome;
    }
}

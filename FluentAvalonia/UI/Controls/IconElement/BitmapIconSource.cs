using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Visuals.Media.Imaging;
using SkiaSharp;
using System;

namespace FluentAvalonia.UI.Controls
{
    public class BitmapIconSource : IconSource, IDisposable
    {
        ~BitmapIconSource()
        {
            Dispose();
        }

        public static readonly DirectProperty<BitmapIconSource, Uri> UriSourceProperty =
            AvaloniaProperty.RegisterDirect<BitmapIconSource, Uri>("UriSource",
                x => x.UriSource, (x, v) => x.UriSource = v);

        public static readonly DirectProperty<BitmapIconSource, bool> ShowAsMonochromeProperty =
            AvaloniaProperty.RegisterDirect<BitmapIconSource, bool>("ShowAsMonochrome",
                x => x.ShowAsMonochrome, (x, v) => x.ShowAsMonochrome = v);

        public Uri UriSource
        {
            get => _source;
            set
            { 
                SetAndRaise(UriSourceProperty, ref _source, value);
                CreateBitmap();
            }
        }

        public bool ShowAsMonochrome
        {
            get => _showAsMonochrome;
            set 
            {
                SetAndRaise(ShowAsMonochromeProperty, ref _showAsMonochrome, value);
                OnBitmapChanged?.Invoke(this, null);
            }
        }

        public Size Size => _originalSize;

        public event EventHandler<object> OnBitmapChanged;

        public void Dispose()
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _originalSize = Size.Empty;
        }

        private void CreateBitmap()
        {
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

            OnBitmapChanged?.Invoke(this, null);
        }

        protected internal SKBitmap _bitmap;
        private Size _originalSize;
        private Uri _source;
        private bool _showAsMonochrome;
    }
}

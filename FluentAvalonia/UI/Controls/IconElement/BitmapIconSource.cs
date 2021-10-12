using Avalonia;
using Avalonia.Platform;
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

        public static readonly StyledProperty<Uri> UriSourceProperty =
            BitmapIcon.UriSourceProperty.AddOwner<BitmapIconSource>();

        public static readonly StyledProperty<bool> ShowAsMonochromeProperty =
            BitmapIcon.ShowAsMonochromeProperty.AddOwner<BitmapIconSource>();

        public Uri UriSource
        {
            get => GetValue(UriSourceProperty);
			set => SetValue(UriSourceProperty, value);
        }

        public bool ShowAsMonochrome
        {
			get => GetValue(ShowAsMonochromeProperty);
			set => SetValue(ShowAsMonochromeProperty, value);
        }

        public Size Size => _originalSize;

        public event EventHandler<object> OnBitmapChanged;

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == UriSourceProperty)
			{
				CreateBitmap(change.NewValue.GetValueOrDefault<Uri>());
			}
			else if (change.Property == ShowAsMonochromeProperty)
			{
				OnBitmapChanged?.Invoke(this, null);
			}
		}

		public void Dispose()
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _originalSize = Size.Empty;
        }

        private void CreateBitmap(Uri src)
        {
            Dispose();

            if (src == null)
                return;

            if (src.IsAbsoluteUri && src.IsFile)
            {
                _bitmap = SKBitmap.Decode(src.LocalPath);
            }
            else
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                _bitmap = SKBitmap.Decode(assets.Open(src));
            }
            _originalSize = new Size(_bitmap.Width, _bitmap.Height);

            OnBitmapChanged?.Invoke(this, null);
        }

        protected internal SKBitmap _bitmap;
        private Size _originalSize;
    }
}

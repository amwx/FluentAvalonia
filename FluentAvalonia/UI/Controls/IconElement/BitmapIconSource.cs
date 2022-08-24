using Avalonia;
using Avalonia.Platform;
using SkiaSharp;
using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents an icon source that uses a bitmap as its content.
    /// </summary>
    public class BitmapIconSource : IconSource, IDisposable
    {
        ~BitmapIconSource()
        {
            Dispose();
        }

        /// <summary>
        /// Defines the <see cref="UriSource"/> property
        /// </summary>
        public static readonly StyledProperty<Uri> UriSourceProperty =
            BitmapIcon.UriSourceProperty.AddOwner<BitmapIconSource>();

        /// <summary>
        /// Defines the <see cref="ShowAsMonochrome"/> property
        /// </summary>
        public static readonly StyledProperty<bool> ShowAsMonochromeProperty =
            BitmapIcon.ShowAsMonochromeProperty.AddOwner<BitmapIconSource>();

        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) of the bitmap to use as the icon content.
        /// </summary>
        public Uri UriSource
        {
            get => GetValue(UriSourceProperty);
			set => SetValue(UriSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the bitmap is shown in a single color.
        /// </summary>
        public bool ShowAsMonochrome
        {
			get => GetValue(ShowAsMonochromeProperty);
			set => SetValue(ShowAsMonochromeProperty, value);
        }

        public Size Size => _originalSize;

        public event EventHandler<object> OnBitmapChanged;

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == UriSourceProperty)
			{
				CreateBitmap(change.GetNewValue<Uri>());
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
            {
                OnBitmapChanged?.Invoke(this, null);
                return;
            }

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

using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Visuals.Media.Imaging;

namespace FluentAvalonia.UI.Controls
{
	public class ImageIcon : IconElement
	{
		public static readonly StyledProperty<IImage> SourceProperty =
			AvaloniaProperty.Register<ImageIcon, IImage>(nameof(Source));

		[Content]
		public IImage Source
		{
			get => GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == SourceProperty)
			{
				InvalidateMeasure();
				InvalidateVisual();
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			return Source?.Size ?? Size.Empty;
		}

		public override void Render(DrawingContext context)
		{
			var size = Bounds.Size;
			var src = Source;
			if (src != null && size.Width > 0 && size.Height > 0)
			{
				Rect viewport = new Rect(size);

				Vector scale = new Vector(size.Width / src.Size.Width, size.Height / src.Size.Height);
				Size scaledSize = src.Size * scale;

				Rect destRect = viewport
					.CenterRect(new Rect(scaledSize))
					.Intersect(viewport);
				Rect srcRect = new Rect(src.Size)
					.CenterRect(new Rect(destRect.Size / scale));

				context.DrawImage(src, srcRect, destRect, BitmapInterpolationMode.HighQuality);
			}
		}
	}
}

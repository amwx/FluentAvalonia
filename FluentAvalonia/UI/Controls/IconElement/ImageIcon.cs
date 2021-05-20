using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Visuals.Media.Imaging;

namespace FluentAvalonia.UI.Controls
{
	public class ImageIcon : IconElement
	{
		public static readonly DirectProperty<ImageIcon, IImage> SourceProperty =
			AvaloniaProperty.RegisterDirect<ImageIcon, IImage>("Source", x => x.Source, (x, v) => x.Source = v);

		[Content]
		public IImage Source
		{
			get => _source;
			set
			{
				if (SetAndRaise(SourceProperty, ref _source, value))
				{
					InvalidateMeasure();
					InvalidateVisual();
				}
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			return _source?.Size ?? Size.Empty;
		}

		public override void Render(DrawingContext context)
		{
			var size = Bounds.Size;
			if (_source != null && size.Width > 0 && size.Height > 0)
			{
				Rect viewport = new Rect(size);

				Vector scale = new Vector(size.Width / _source.Size.Width, size.Height / _source.Size.Height);
				Size scaledSize = _source.Size * scale;

				Rect destRect = viewport
					.CenterRect(new Rect(scaledSize))
					.Intersect(viewport);
				Rect srcRect = new Rect(_source.Size)
					.CenterRect(new Rect(destRect.Size / scale));

				context.DrawImage(_source, srcRect, destRect, BitmapInterpolationMode.HighQuality);
			}
		}

		private IImage _source;
	}
}

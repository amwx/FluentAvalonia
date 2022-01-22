using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents an icon source that uses an image type as its content.
	/// </summary>
	public class ImageIconSource : IconSource
	{
		/// <summary>
		/// Gets or sets the <see cref="Source"/> property
		/// </summary>
        public static readonly StyledProperty<IImage> SourceProperty =
            ImageIcon.SourceProperty.AddOwner<ImageIconSource>();

		/// <summary>
		/// Gets or sets the <see cref="Avalonia.Media.IImage"/> content this icon displays
		/// </summary>
		[Content]
		public IImage Source
		{
			get => GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}
	}
}

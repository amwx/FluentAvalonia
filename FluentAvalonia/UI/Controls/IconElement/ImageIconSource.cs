using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class ImageIconSource : IconSource
	{
		public static readonly DirectProperty<ImageIconSource, IImage> SourceProperty =
			AvaloniaProperty.RegisterDirect<ImageIconSource, IImage>("Source", x => x.Source, (x, v) => x.Source = v);

		[Content]
		public IImage Source
		{
			get => _source;
			set => SetAndRaise(SourceProperty, ref _source, value);
		}

		private IImage _source;
	}
}

using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class ImageIconSource : IconSource
	{
		public static readonly DirectProperty<ImageIcon, IImage> SourceProperty =
			AvaloniaProperty.RegisterDirect<ImageIcon, IImage>("Source", x => x.Source, (x, v) => x.Source = v);

		public IImage Source
		{
			get => _source;
			set => SetAndRaise(SourceProperty, ref _source, value);
		}

		private IImage _source;
	}
}

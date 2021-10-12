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
        public static readonly StyledProperty<IImage> SourceProperty =
            ImageIcon.SourceProperty.AddOwner<ImageIconSource>();

		[Content]
		public IImage Source
		{
			get => GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}
	}
}

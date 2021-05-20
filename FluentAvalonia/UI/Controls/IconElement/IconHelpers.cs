using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public static class IconHelpers
    {
        internal static FontIcon CreateFontIconFromFontIconSource(FontIconSource fis)
        {
            return new FontIcon
            {
                [!TextBlock.ForegroundProperty] = fis[!TextBlock.ForegroundProperty],
                [!TextBlock.FontWeightProperty] = fis[!TextBlock.FontWeightProperty],
                [!TextBlock.FontStyleProperty] = fis[!TextBlock.FontStyleProperty],
                [!TextBlock.FontFamilyProperty] = fis[!TextBlock.FontFamilyProperty],
                [!TextBlock.FontSizeProperty] = fis[!TextBlock.FontSizeProperty],
                [!FontIcon.GlyphProperty] = fis[!FontIconSource.GlyphProperty]
            };
        }

        internal static PathIcon CreatePathIconFromPathIconSource(PathIconSource pis)
        {
            return new PathIcon
            {
                [!PathIcon.DataProperty] = pis[!PathIconSource.DataProperty]
            };
        }

        internal static SymbolIcon CreateSymbolIconFromSymbolIconSource(SymbolIconSource sis)
        {
            return new SymbolIcon
            {
                [!SymbolIcon.SymbolProperty] = sis[!SymbolIconSource.SymbolProperty]
            };
        }

        internal static BitmapIcon CreateBitmapIconFromBitmapIconSource(BitmapIconSource bis)
        {
            //This one works slightly differently to avoid holding multiple instances of the same
            //bitmap in memory (since IconSources are meant for sharing). We don't alter the properties, 
            //but instead just link the SKBitmap from BitmapIconSource into the BitmapIcon. 
            BitmapIcon bi = new BitmapIcon();
            bi.LinkToBitmapIconSource(bis);
            return bi;
        }

		internal static ImageIcon CreateImageIconFromImageIconSource(ImageIconSource iis)
		{
			return new ImageIcon
			{
				[!ImageIcon.SourceProperty] = iis[!ImageIconSource.SourceProperty]
			};
		}

        internal static IconElement CreateFromUnknown(IconSource src)
        {
            if (src is BitmapIconSource bis)
            {
                return CreateBitmapIconFromBitmapIconSource(bis);
            }
            else if (src is FontIconSource fis)
            {
                return CreateFontIconFromFontIconSource(fis);
            }
            else if (src is PathIconSource pis)
            {
                return CreatePathIconFromPathIconSource(pis);
            }
            else if (src is SymbolIconSource sis)
            {
                return CreateSymbolIconFromSymbolIconSource(sis);
            }
			else if (src is ImageIconSource iis)
			{
				return CreateImageIconFromImageIconSource(iis);
			}

            return null;
        }
    }
}

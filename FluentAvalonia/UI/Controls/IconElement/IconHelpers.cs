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
            var fi = new FontIcon
            {
                [!TextBlock.FontWeightProperty] = fis[!TextBlock.FontWeightProperty],
                [!TextBlock.FontStyleProperty] = fis[!TextBlock.FontStyleProperty],
                [!TextBlock.FontFamilyProperty] = fis[!TextBlock.FontFamilyProperty],
                [!TextBlock.FontSizeProperty] = fis[!TextBlock.FontSizeProperty],
                [!FontIcon.GlyphProperty] = fis[!FontIconSource.GlyphProperty]
            };

            if (fis.IsSet(TextBlock.ForegroundProperty))
            {
                fi[!TextBlock.ForegroundProperty] = fis[!TextBlock.ForegroundProperty];
            }

            return fi;
        }

        internal static PathIcon CreatePathIconFromPathIconSource(PathIconSource pis)
        {
            var pi = new PathIcon
            {
                [!PathIcon.DataProperty] = pis[!PathIconSource.DataProperty]
            };

            if (pis.IsSet(TextBlock.ForegroundProperty))
            {
                pi[!TextBlock.ForegroundProperty] = pis[!TextBlock.ForegroundProperty];
            }

            return pi;
        }

        internal static SymbolIcon CreateSymbolIconFromSymbolIconSource(SymbolIconSource sis)
        {
            var si = new SymbolIcon
            {
                [!SymbolIcon.SymbolProperty] = sis[!SymbolIconSource.SymbolProperty],
                [!TextBlock.FontSizeProperty] = sis[!TextBlock.FontSizeProperty]
            };

            if (sis.IsSet(TextBlock.ForegroundProperty))
            {
                si[!TextBlock.ForegroundProperty] = sis[!TextBlock.ForegroundProperty];
            }

            return si;
        }

        internal static BitmapIcon CreateBitmapIconFromBitmapIconSource(BitmapIconSource bis)
        {
            //This one works slightly differently to avoid holding multiple instances of the same
            //bitmap in memory (since IconSources are meant for sharing). We don't alter the properties, 
            //but instead just link the SKBitmap from BitmapIconSource into the BitmapIcon. 
            BitmapIcon bi = new BitmapIcon();
            bi.LinkToBitmapIconSource(bis);

            if (bi.IsSet(TextBlock.ForegroundProperty))
            {
                bi[!TextBlock.ForegroundProperty] = bis[!TextBlock.ForegroundProperty];
            }

            return bi;
        }

		internal static ImageIcon CreateImageIconFromImageIconSource(ImageIconSource iis)
		{
			var ii = new ImageIcon
			{
				[!ImageIcon.SourceProperty] = iis[!ImageIconSource.SourceProperty]
			};

            if (iis.IsSet(TextBlock.ForegroundProperty))
            {
                ii[!TextBlock.ForegroundProperty] = iis[!TextBlock.ForegroundProperty];
            }

            return ii;
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

using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace FluentAvalonia.UI.Controls
{
    internal static class IconHelpers
    {
        public static FontIcon CreateFontIconFromFontIconSource(FontIconSource fis)
        {
            var fi = new FontIcon
            {
                [!TextBlock.FontWeightProperty] = fis[!TextBlock.FontWeightProperty],
                [!TextBlock.FontStyleProperty] = fis[!TextBlock.FontStyleProperty],
                [!TextBlock.FontFamilyProperty] = fis[!TextBlock.FontFamilyProperty],
                [!TextBlock.FontSizeProperty] = fis[!TextBlock.FontSizeProperty],
                [!FontIcon.GlyphProperty] = fis[!FontIconSource.GlyphProperty]
            };

            if (fis.IsSet(IconSource.ForegroundProperty))
            {
                fi.Bind(TextBlock.ForegroundProperty, fis.GetBindingObservable(IconSource.ForegroundProperty),
                    priority: BindingPriority.LocalValue);
            }
            else
            {
                fi.Bind(TextBlock.ForegroundProperty, fis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                    priority: BindingPriority.LocalValue);
            }

            return fi;
        }

        public static PathIcon CreatePathIconFromPathIconSource(PathIconSource pis)
        {
            var pi = new PathIcon
            {
                [!PathIcon.DataProperty] = pis[!PathIconSource.DataProperty],
                [!PathIcon.StretchProperty] = pis[!PathIconSource.StretchProperty],
                [!PathIcon.StretchDirectionProperty] = pis[!PathIconSource.StretchDirectionProperty]
            };

            if (pis.IsSet(IconSource.ForegroundProperty))
            {
                pi.Bind(TextBlock.ForegroundProperty, pis.GetBindingObservable(IconSource.ForegroundProperty),
                    priority: BindingPriority.LocalValue);
            }
            else
            {
                pi.Bind(TextBlock.ForegroundProperty, pis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                    priority: BindingPriority.LocalValue);
            }

            return pi;
        }

        public static SymbolIcon CreateSymbolIconFromSymbolIconSource(SymbolIconSource sis)
        {
            var si = new SymbolIcon
            {
                [!SymbolIcon.SymbolProperty] = sis[!SymbolIconSource.SymbolProperty],
                [!TextBlock.FontSizeProperty] = sis[!TextBlock.FontSizeProperty]
            };

            if (sis.IsSet(IconSource.ForegroundProperty))
            {
                si.Bind(TextBlock.ForegroundProperty, sis.GetBindingObservable(IconSource.ForegroundProperty),
                    priority: BindingPriority.LocalValue);
            }
            else
            {
                si.Bind(TextBlock.ForegroundProperty, sis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                    priority: BindingPriority.LocalValue);
            }
           
            return si;
        }

        public static BitmapIcon CreateBitmapIconFromBitmapIconSource(BitmapIconSource bis)
        {
            // This one works slightly differently to avoid holding multiple instances of the same
            // bitmap in memory (since IconSources are meant for sharing). We don't alter the properties, 
            // but instead just link the SKBitmap from BitmapIconSource into the BitmapIcon. 
            BitmapIcon bi = new BitmapIcon();
            bi.LinkToBitmapIconSource(bis);

            if (bis.IsSet(IconSource.ForegroundProperty))
            {
                bi.Bind(TextBlock.ForegroundProperty, bis.GetBindingObservable(IconSource.ForegroundProperty),
                    priority: BindingPriority.LocalValue);
            }
            else
            {
                bi.Bind(TextBlock.ForegroundProperty, bis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                    priority: BindingPriority.LocalValue);
            }

            return bi;
        }

        public static ImageIcon CreateImageIconFromImageIconSource(ImageIconSource iis)
		{
			var ii = new ImageIcon
			{
				[!ImageIcon.SourceProperty] = iis[!ImageIconSource.SourceProperty]
			};

            if (iis.IsSet(IconSource.ForegroundProperty))
            {
                ii.Bind(TextBlock.ForegroundProperty, iis.GetBindingObservable(IconSource.ForegroundProperty),
                    priority: BindingPriority.LocalValue);
            }
            else
            {
                ii.Bind(TextBlock.ForegroundProperty, iis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                    priority: BindingPriority.LocalValue);
            }

            return ii;
		}

        public static IconElement CreateFromUnknown(IconSource src)
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

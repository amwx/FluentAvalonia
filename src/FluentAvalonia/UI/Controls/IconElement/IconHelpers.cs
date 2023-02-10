using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;

namespace FluentAvalonia.UI.Controls;

internal static class IconHelpers
{
    public static FontIcon CreateFontIconFromFontIconSource(FontIconSource fis)
    {
        var fi = new FontIcon
        {
            [!TextElement.FontWeightProperty] = fis[!TextElement.FontWeightProperty],
            [!TextElement.FontStyleProperty] = fis[!TextElement.FontStyleProperty],
            [!TextElement.FontFamilyProperty] = fis[!TextElement.FontFamilyProperty],
            [!TextElement.FontSizeProperty] = fis[!TextElement.FontSizeProperty],
            [!FontIcon.GlyphProperty] = fis[!FontIconSource.GlyphProperty]
        };

        if (fis.IsSet(IconSource.ForegroundProperty))
        {
            fi.Bind(TextElement.ForegroundProperty, fis.GetBindingObservable(IconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            fi.Bind(TextElement.ForegroundProperty, fis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return fi;
    }

    public static FAPathIcon CreatePathIconFromPathIconSource(PathIconSource pis)
    {
        var pi = new FAPathIcon
        {
            [!FAPathIcon.DataProperty] = pis[!PathIconSource.DataProperty],
            [!FAPathIcon.StretchProperty] = pis[!PathIconSource.StretchProperty],
            [!FAPathIcon.StretchDirectionProperty] = pis[!PathIconSource.StretchDirectionProperty]
        };

        if (pis.IsSet(IconSource.ForegroundProperty))
        {
            pi.Bind(TextElement.ForegroundProperty, pis.GetBindingObservable(IconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            pi.Bind(TextElement.ForegroundProperty, pis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return pi;
    }

    public static SymbolIcon CreateSymbolIconFromSymbolIconSource(SymbolIconSource sis)
    {
        var si = new SymbolIcon
        {
            [!SymbolIcon.SymbolProperty] = sis[!SymbolIconSource.SymbolProperty],
            [!TextElement.FontSizeProperty] = sis[!TextElement.FontSizeProperty]
        };

        if (sis.IsSet(IconSource.ForegroundProperty))
        {
            si.Bind(TextElement.ForegroundProperty, sis.GetBindingObservable(IconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            si.Bind(TextElement.ForegroundProperty, sis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
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
            bi.Bind(TextElement.ForegroundProperty, bis.GetBindingObservable(IconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            bi.Bind(TextElement.ForegroundProperty, bis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
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
            ii.Bind(TextElement.ForegroundProperty, iis.GetBindingObservable(IconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            ii.Bind(TextElement.ForegroundProperty, iis.GetBindingObservable(IconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return ii;
    }

    public static FAIconElement CreateFromUnknown(IconSource src)
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

using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public static class FAIconHelpers
{
    internal static FAFontIcon CreateFontIconFromFontIconSource(FAFontIconSource fis)
    {
        var fi = new FAFontIcon
        {
            [!TextElement.FontWeightProperty] = fis[!TextElement.FontWeightProperty],
            [!TextElement.FontStyleProperty] = fis[!TextElement.FontStyleProperty],
            [!TextElement.FontFamilyProperty] = fis[!TextElement.FontFamilyProperty],
            [!TextElement.FontSizeProperty] = fis[!TextElement.FontSizeProperty],
            [!FAFontIcon.GlyphProperty] = fis[!FAFontIconSource.GlyphProperty]
        };

        if (fis.IsSet(FAIconSource.ForegroundProperty))
        {
            fi.Bind(TextElement.ForegroundProperty, fis.GetBindingObservable(FAIconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            fi.Bind(TextElement.ForegroundProperty, fis.GetBindingObservable(FAIconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return fi;
    }

    internal static FAPathIcon CreatePathIconFromPathIconSource(FAPathIconSource pis)
    {
        var pi = new FAPathIcon
        {
            [!FAPathIcon.DataProperty] = pis[!FAPathIconSource.DataProperty],
            [!FAPathIcon.StretchProperty] = pis[!FAPathIconSource.StretchProperty],
            [!FAPathIcon.StretchDirectionProperty] = pis[!FAPathIconSource.StretchDirectionProperty]
        };

        if (pis.IsSet(FAIconSource.ForegroundProperty))
        {
            pi.Bind(TextElement.ForegroundProperty, pis.GetBindingObservable(FAIconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            pi.Bind(TextElement.ForegroundProperty, pis.GetBindingObservable(FAIconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return pi;
    }

    internal static FASymbolIcon CreateSymbolIconFromSymbolIconSource(FASymbolIconSource sis)
    {
        var si = new FASymbolIcon
        {
            [!FASymbolIcon.SymbolProperty] = sis[!FASymbolIconSource.SymbolProperty],
            [!TextElement.FontSizeProperty] = sis[!TextElement.FontSizeProperty]
        };

        if (sis.IsSet(FAIconSource.ForegroundProperty))
        {
            si.Bind(TextElement.ForegroundProperty, sis.GetBindingObservable(FAIconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            si.Bind(TextElement.ForegroundProperty, sis.GetBindingObservable(FAIconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return si;
    }

    internal static FABitmapIcon CreateBitmapIconFromBitmapIconSource(FABitmapIconSource bis)
    {
        // This one works slightly differently to avoid holding multiple instances of the same
        // bitmap in memory (since IconSources are meant for sharing). We don't alter the properties, 
        // but instead just link the SKBitmap from BitmapIconSource into the BitmapIcon. 
        FABitmapIcon bi = new FABitmapIcon();
        bi.LinkToBitmapIconSource(bis);

        if (bis.IsSet(FAIconSource.ForegroundProperty))
        {
            bi.Bind(TextElement.ForegroundProperty, bis.GetBindingObservable(FAIconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            bi.Bind(TextElement.ForegroundProperty, bis.GetBindingObservable(FAIconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return bi;
    }

    internal static FAImageIcon CreateImageIconFromImageIconSource(FAImageIconSource iis)
    {
        var ii = new FAImageIcon
        {
            [!FAImageIcon.SourceProperty] = iis[!FAImageIconSource.SourceProperty]
        };

        if (iis.IsSet(FAIconSource.ForegroundProperty))
        {
            ii.Bind(TextElement.ForegroundProperty, iis.GetBindingObservable(FAIconSource.ForegroundProperty),
                priority: BindingPriority.LocalValue);
        }
        else
        {
            ii.Bind(TextElement.ForegroundProperty, iis.GetBindingObservable(FAIconSource.ForegroundProperty).Skip(1),
                priority: BindingPriority.LocalValue);
        }

        return ii;
    }

    internal static FAIconElement CreateFromUnknown(FAIconSource src)
    {
        if (src is FABitmapIconSource bis)
        {
            return CreateBitmapIconFromBitmapIconSource(bis);
        }
        else if (src is FAFontIconSource fis)
        {
            return CreateFontIconFromFontIconSource(fis);
        }
        else if (src is FAPathIconSource pis)
        {
            return CreatePathIconFromPathIconSource(pis);
        }
        else if (src is FASymbolIconSource sis)
        {
            return CreateSymbolIconFromSymbolIconSource(sis);
        }
        else if (src is FAImageIconSource iis)
        {
            return CreateImageIconFromImageIconSource(iis);
        }

        if (_customConverters != null)
        {
            var type = src.GetType();
            if (_customConverters.TryGetValue(type, out var value))
            {
                return value(src);
            }
        }

        return null;
    }

    /// <summary>
    /// Registers a <see cref="FAIconElement"/> creation factory for custom <see cref="FAIconSource"/> types
    /// </summary>
    /// <remarks>
    /// When creating a custom IconSource, you will also need to create a matching FAIconElement type that
    /// will actually be used for display. Just as the built-in icons do, you will need to handle the mapping
    /// between the custom IconSource and related FAIconElement.
    /// </remarks>
    public static void RegisterCustomIconSourceFactory(Type typeOfIconSource, Func<FAIconSource, FAIconElement> factory)
    {
        _customConverters ??= new Dictionary<Type, Func<FAIconSource, FAIconElement>>();

        _customConverters.Add(typeOfIconSource, factory);
    }

    private static Dictionary<Type, Func<FAIconSource, FAIconElement>> _customConverters;
}

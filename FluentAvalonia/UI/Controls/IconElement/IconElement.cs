//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace FluentAvalonia.UI.Controls
{
    /*
     * Microsoft also includes an IconElementSource, [Class]Source
     * in WinUI, which is just a DependencyObject with a reference to the Glyph, Symbol, etc.
     * used for display...which because its not a FrameworkElement, can be shared
     * Then the actual rendering still relies on classes below, see WinUI3 SharedHelpers.cpp file
     * On TODO list for here, but not of high priority
     */

    [TypeConverter(typeof(StringToIconElementConverter))]
    public abstract class IconElement : UserControl
    {
        static IconElement()
        {
            AffectsRender<IconElement>(ForegroundProperty);
        }
    }

    /// <summary>
    /// Creates an icon from a glyph in a specific font. This no longer has a default font, specified here
    /// and inherits like normal. If you want a symbol, either specify the font yourself or use a
    /// <see cref="SymbolIcon"/> to ensure its platform-independent
    /// </summary>
    public class FontIcon : IconElement
    {
        public FontIcon()
        {
        }

        static FontIcon()
        {
            //Ensures textlayout is changed after Font characteristic change
            GlyphProperty.Changed.AddClassHandler<FontIcon>((x, e) => x.OnGlyphChanged(e));
            FontFamilyProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            //We have AffectsRender<>(ForegroundProperty) set in parent IconElement class, but this is still necessary...
            ForegroundProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            FontSizeProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            FontWeightProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            FontStyleProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
        }

        public static readonly StyledProperty<string> GlyphProperty = AvaloniaProperty.Register<FontIcon, string>("Glyph", defaultValue: "");
        public string Glyph
        {
            get => GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        #region Override Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                if (_textlayout == null)
                    GenerateText();

                return new Size(_textlayout.Bounds.Width, _textlayout.Bounds.Height);
            }
            catch
            {
                return base.MeasureOverride(availableSize);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            try
            {
                return new Size(_textlayout.Bounds.Width, _textlayout.Bounds.Height);
            }
            catch
            {
                return base.MeasureOverride(finalSize);
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (_textlayout == null)//Shouldn't be the case, but...
                GenerateText();

            Point pt;
            if (double.IsNaN(Bounds.Width) || double.IsNaN(Bounds.Height))
                pt = new Point(0, 0);
            else
                pt = new Point(Bounds.Width / 2.0 - _textlayout.Bounds.Width / 2.0, Bounds.Height / 2.0 - _textlayout.Bounds.Height / 2.0);
            _textlayout.Draw(context.PlatformImpl, pt);

        }

        #endregion

        #region private methods

        private void OnGlyphChanged(AvaloniaPropertyChangedEventArgs e)
        {
            GenerateText();
            InvalidateVisual();
        }

        private void GenerateText()
        {
            _textlayout = new TextLayout(Glyph,
                FontManager.Current?.GetOrAddTypeface(FontFamily, FontWeight, FontStyle),
                FontSize, Foreground, TextAlignment.Left);
        }

        #endregion


        private TextLayout _textlayout;
    }

    /// <summary>
    /// Creates an icon from a StreamGeometry
    /// NOTE: this class has an issue where the bottom & right can be cut off
    /// Not sure why this is happening, but looking into it
    /// </summary>
    public class PathIcon : IconElement
    {
        public PathIcon()
        {

        }
        static PathIcon()
        {
            AffectsMeasure<PathIcon>(DataProperty);
            AffectsRender<PathIcon>(DataProperty);
        }

        public static StyledProperty<Geometry> DataProperty = AvaloniaProperty.Register<PathIcon, Geometry>("Data");
        public Geometry Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #region Override Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                if (Data == null)
                    return base.MeasureOverride(availableSize);

                var wid = 0.0;
                var hei = 0.0;
                var bounds = Data.PlatformImpl.GetRenderBounds(null);

                //If the geometry itself doesn't start at (0,0), the actual bounding rect calculated
                //doesn't account for this, so we need to extend the path by the (Left,Top) offset
                //to ensure the bounding box of the PathIcon is large enough to display the icon
                //This fixes the previous issue where the right/bottom would get cut off
                wid = bounds.Left > 0 ? bounds.Width + (bounds.Left * 2) : bounds.Width;
                hei = bounds.Top > 0 ? bounds.Height + (bounds.Top * 2) : bounds.Height;

                return new Size(wid + Padding.Left + Padding.Right, hei + Padding.Top + Padding.Bottom);
            }
            catch
            {
                return base.MeasureOverride(availableSize);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            try
            {
                if (Data == null)
                    return base.ArrangeOverride(finalSize);

                var wid = 0.0;
                var hei = 0.0;
                var bounds = Data.PlatformImpl.GetRenderBounds(null);

                //If the geometry itself doesn't start at (0,0), the actual bounding rect calculated
                //doesn't account for this, so we need to extend the path by the (Left,Top) offset
                //to ensure the bounding box of the PathIcon is large enough to display the icon
                //This fixes the previous issue where the right/bottom would get cut off
                wid = bounds.Left > 0 ? bounds.Width + (bounds.Left * 2) : bounds.Width;
                hei = bounds.Top > 0 ? bounds.Height + (bounds.Top * 2) : bounds.Height;

                return new Size(wid + Padding.Left + Padding.Right, hei + Padding.Top + Padding.Bottom);
            }
            catch
            {
                return base.ArrangeOverride(finalSize);
            }
        }

        public override void Render(DrawingContext context)
        {
            if (Data != null)
            {
                context.DrawGeometry(Foreground, null, Data);
            }
        }

        #endregion



        /// <summary>
        /// Quick and dirty check if we have a valid PathGeometry. This probably needs to be
        /// more robust, but this is better than a bunch of InvalidDataExceptions becase we 
        /// don't have a Path.TryParse() method. This does still fail sometimes, but its better
        /// than nothing. Its really only meant to be called from the StringToIconElementConverter
        /// </summary>
        /// <param name="data"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static bool IsDataValid(string data, out Geometry g)
        {
            if (data.Length == 0)
            {
                g = null;
                return false;
            }
            var first = data[0].ToString().ToUpper();

            var acceptFirst = new List<string>() { "M", "C", "L", "V", "H", "F" };

            if (acceptFirst.Contains(first))
            {
                g = StreamGeometry.Parse(data);
                return true;
            }

            try
            {
                var dat = StreamGeometry.Parse(data);
                g = dat;
                return true;
            }
            catch
            {
                g = null;
                return false;
            }

        }


    }

    /// <summary>
    /// Creates an icon from a bitmap
    /// Tested on actual files, haven't tested internal resources yet.
    /// </summary>
    public class BitmapIcon : IconElement
    {
        public BitmapIcon()
        {
        }

        public static readonly DirectProperty<BitmapIcon, Uri> UriSourceProperty =
            AvaloniaProperty.RegisterDirect<BitmapIcon, Uri>("UriSource",
                x => x.UriSource, (x, v) => x.UriSource = v);

        public Uri UriSource
        {
            get => _source;
            set
            {
                if (SetAndRaise(UriSourceProperty, ref _source, value))
                {
                    _ = RecreateBitmap();
                    InvalidateVisual();
                }
            }
        }


        #region override methods

        protected override Size MeasureOverride(Size availableSize)
        {
            if (UriSource == null || _internalBitmap == null)
                return base.MeasureOverride(availableSize);

            //If we have a width or height, we want to respect that and draw to that Rect,
            //otherwise we draw the raw image size 
            var wid = Width;
            var hei = Height;

            return new Size(double.IsInfinity(wid) || double.IsNaN(wid) ? _internalBitmap.Size.Width : wid,
                double.IsInfinity(hei) || double.IsNaN(hei) ? _internalBitmap.Size.Height : hei);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (UriSource == null || _internalBitmap == null)
                return base.ArrangeOverride(finalSize);

            //If we have a width or height, we want to respect that and draw to that Rect,
            //otherwise we draw the raw image size 
            var wid = Width;
            var hei = Height;

            return new Size(double.IsInfinity(wid) || double.IsNaN(wid) ? _internalBitmap.Size.Width : wid,
                double.IsInfinity(hei) || double.IsNaN(hei) ? _internalBitmap.Size.Height : hei);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (UriSource == null || _internalBitmap == null)
                return;

            var wid = Width;
            var hei = Height;

            //If we have a width or height, we want to respect that and draw to that Rect,
            //otherwise we draw the raw image size 
            Rect r = new Rect(new Size(double.IsInfinity(wid) || double.IsNaN(wid) ? _internalBitmap.Size.Width : wid,
                double.IsInfinity(hei) || double.IsNaN(hei) ? _internalBitmap.Size.Height : hei));

            context.DrawImage(_internalBitmap, r);
        }

        #endregion

        #region Private methods

        private bool RecreateBitmap()
        {
            if (_internalBitmap != null)
            {
                _internalBitmap.Dispose();
                _internalBitmap = null;
            }

            if (UriSource == null)
                return false;

            try
            {
                if (UriSource.IsAbsoluteUri && UriSource.IsFile)
                {
                    _internalBitmap = new Bitmap(UriSource.LocalPath);
                    return true;
                }
                else
                {
                    //If it's not a physical file, we attempt to pull it from the assembly resources
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    _internalBitmap = new Bitmap(assets.Open(UriSource));
                    return true;
                }
            }
            catch
            {
                return false;
            }

        }

        #endregion


        private Bitmap _internalBitmap;
        private Uri _source;
    }


    /// <summary>
    /// Creates an Icon from one of the Symbols in the <see cref="Symbol"/> enum
    /// This is safer to use cross-platform as the symbol character code isn't tied to
    /// only one font. SymbolThemeFontFamily, assuming it's not overridden, can either be
    /// Segoe MDL2 Assets or winjs-symbols, which is included in our Assembly.
    /// //Rendering logic is otherwise identical to FontIcon
    /// While FontSize can be changed, it's best to place this in a Viewbox and let scaling
    /// occur automatically
    /// </summary>
    public class SymbolIcon : IconElement
    {
        public SymbolIcon()
        {

        }

        static SymbolIcon()
        {
            SymbolProperty.Changed.AddClassHandler<SymbolIcon>((x, v) => x.OnSymbolChanged());
            FontSizeProperty.Changed.AddClassHandler<SymbolIcon>((x, v) => x.OnSymbolChanged());
            ForegroundProperty.Changed.AddClassHandler<SymbolIcon>((x, v) => x.OnSymbolChanged());
        }

        public static readonly StyledProperty<Symbol> SymbolProperty =
            AvaloniaProperty.Register<SymbolIcon, Symbol>("Symbol");

        public Symbol Symbol
        {
            get => GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }

        #region Override Methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            var ff = this.TryFindResource("SymbolThemeFontFamily", out object value);
            var fs = this.TryFindResource("ControlContentThemeFontSize", out object size);
            //We don't want to override the FontFamily property, as that at least provides
            //us a final fallback if SymbolThemeFontFamily doesn't exist or isn't found
            //it won't render the glyph properly, but its better than an NRE

            if (ff)
            {
                var tf = FontManager.Current.GetOrAddTypeface((FontFamily)value);

                _SymbolFontFamily = tf.FontFamily;
                OnSymbolChanged(); //Call to ensure glyph is created...
            }

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                if (_textLayout == null)
                    RecreateSymbolText();

                return new Size(_textLayout?.Bounds.Width ?? 0, _textLayout?.Bounds.Height ?? 0);
            }
            catch
            {
                return base.MeasureOverride(availableSize);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            try
            {
                return new Size(_textLayout?.Bounds.Width ?? 0, _textLayout?.Bounds.Height ?? 0);
            }
            catch
            {
                return base.MeasureOverride(finalSize);
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            //if (_textLayout == null)//Shouldn't be the case, but...
            //    RecreateSymbolText();

            if (_textLayout == null)
                return;

            Point pt;
            if (double.IsNaN(Bounds.Width) || double.IsNaN(Bounds.Height))
                pt = new Point(0, 0);
            else
                pt = new Point(Bounds.Width / 2.0 - _textLayout.Bounds.Width / 2.0, Bounds.Height / 2.0 - _textLayout.Bounds.Height / 2.0);
            _textLayout.Draw(context.PlatformImpl, pt);

        }

        #endregion


        #region private methods

        private void OnSymbolChanged()
        {
            bool useSegoe = false;
            if (_SymbolFontFamily != null && _SymbolFontFamily.Name == "Segoe MDL2 Assets")
                useSegoe = true;
            //Here we know whether or not we're using Segoe MDL2 Assets or winjs-symbols,
            //so we don't need SymbolHelper to check for us
            symbolGlyph = SymbolHelper.GetCharacterForSymbol(Symbol, useSegoe, false);

            RecreateSymbolText();
        }

        private void RecreateSymbolText()
        {
            if (_SymbolFontFamily == null)
                return;
            //Create the symbol text, hopefully using the SymbolFontFamily, but jic fall back to the 
            //FontFamily property, we force Normal FontWeight & FontStyle as those wouldn't normally
            //apply to the symbol anyway
            _textLayout = new TextLayout(symbolGlyph,
                FontManager.Current?.GetOrAddTypeface(_SymbolFontFamily != null ? _SymbolFontFamily : FontFamily,
                FontWeight.Normal, FontStyle.Normal),
                FontSize, Foreground, TextAlignment.Left);
        }

        #endregion

        private string symbolGlyph;
        private TextLayout _textLayout;
        private FontFamily _SymbolFontFamily;
    }


    public class StringToIconElementConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var val = value.ToString();

                //First we try if the text is a valid Symbol
                if (Enum.TryParse(typeof(Symbol), val, out object sym))
                {
                    return new SymbolIcon() { Symbol = (Symbol)sym };
                }

                //Try a PathIcon
                if (PathIcon.IsDataValid(val, out Geometry g))
                {
                    return new PathIcon() { Data = g };
                }

                try
                {
                    if (Uri.TryCreate(val, UriKind.RelativeOrAbsolute, out Uri result))
                    {
                        return new BitmapIcon() { UriSource = result };
                    }
                }
                catch (Exception ex) { throw ex; }

                //If we've reached this point, we'll make a FontIcon
                //Glyph can be anything (sort of), so we don't need to Try/Catch
                return new FontIcon() { Glyph = val };

            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}

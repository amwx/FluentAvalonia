//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace FluentAvalonia.UI.Controls
{
    /*
     * Microsoft has updated this to also include an IconElementSource, [Class]Source
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
    /// Creates an icon from a glyph in a specific font
    /// The default is to find SymbolThemeFontFamily in resources, or Segoe MDL2 Assets, the
    /// default symbol font on Win10. Either include this font, or change it, if supporting
    /// other than Win10
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

        protected override void OnApplyTemplate(Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            //Set the default symbol font only if the user didn't specify one when creating the FontIcon
            //if(!_userSetFont)
            //{
                //On Windows, SymbolThemeFontFamily corresponds to Segoe MDL2 Assets
            var ff = this.TryFindResource("SymbolThemeFontFamily", out object value);
            if (ff)
            {
                FontFamily = (FontFamily)value;
            }
            else
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets");
            }
           // }            
        }

        private void OnGlyphChanged(AvaloniaPropertyChangedEventArgs e)
        {
            GenerateText();
            InvalidateVisual();
        }
        private void OnFontChanged(AvaloniaPropertyChangedEventArgs e)
        {
            //_userSetFont = true;
            
            InvalidateVisual();
        }


        private TextLayout _textlayout;
        private void GenerateText()
        {
            //TO DO: Issue where using FontFamily property will override the default setting
            //For now, hard code the symbol font
            _textlayout = new TextLayout(Glyph,
                FontManager.Current?.GetOrAddTypeface(new FontFamily("Segoe MDL2 Assets"), FontWeight, FontStyle),
                FontSize, Foreground, TextAlignment.Left);

        }

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

            // context.DrawText(Foreground, pt, _formattedText);
            
        }

        //private bool _userSetFont = false;
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

        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                if (Data == null)
                    return base.MeasureOverride(availableSize);
                
                return new Size(Data.Bounds.Width + Padding.Left + Padding.Right, Data.Bounds.Height + Padding.Top + Padding.Bottom);
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

                return new Size(Data.Bounds.Width + Padding.Left + Padding.Right, Data.Bounds.Height + Padding.Top + Padding.Bottom);
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

        public static bool IsDataValid(string data, out Geometry g)
        {
            //Simple test for now, probably should make this more robust
            //or get AvaloniaUI team to implement TryParse...
            //This still seems to fail tho so...

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
                x => x.UriSource, (x,v) => x.UriSource = v);
        public Uri UriSource
        {
            get => _source;
            set 
            { 
                if(SetAndRaise(UriSourceProperty, ref _source, value))
                {
                    _ = RecreateBitmap();
                    InvalidateVisual();
                }
            }
        }

        private bool RecreateBitmap()
        {
            if(_internalBitmap != null)
            {
                _internalBitmap.Dispose();
                _internalBitmap = null;
            }

            if (UriSource == null)
                return false;

            try
            {
                _internalBitmap = new Bitmap(UriSource.LocalPath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if(UriSource == null || _internalBitmap == null)
                return base.MeasureOverride(availableSize);

            return _internalBitmap.Size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (UriSource == null || _internalBitmap == null)
                return base.ArrangeOverride(finalSize);

            return _internalBitmap.Size;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (UriSource == null || _internalBitmap == null)
                return;
            context.DrawImage(_internalBitmap, new Rect(_internalBitmap.Size));
        }


        private Bitmap _internalBitmap;
        private Uri _source;
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

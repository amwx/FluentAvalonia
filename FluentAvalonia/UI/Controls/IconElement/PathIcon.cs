using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls
{
    public class PathIcon : IconElement
    {
        public static DirectProperty<PathIcon, Geometry> DataProperty =
            AvaloniaProperty.RegisterDirect<PathIcon, Geometry>("Data",
                x => x.Data, (x, v) => x.Data = v);

        public Geometry Data
        {
            get => _data;
            set
            {
                SetAndRaise(DataProperty, ref _data, value);
                InvalidateVisual();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_data == null)
                return base.MeasureOverride(availableSize);

            return _data.Bounds.Size;
        }

        public override void Render(DrawingContext context)
        {
            if (_data == null)
                return;

            //Create scale & clip to make sure path is always drawn inside
            //the bounds specified
            var bounds = _data.Bounds;
            var destRect = new Rect(Bounds.Size);
            var scale = Matrix.CreateScale(
                destRect.Width / bounds.Width,
                destRect.Height / bounds.Height);
            var translate = Matrix.CreateTranslation(-(Vector)bounds.Position);

            using (context.PushClip(destRect))
            using (context.PushPreTransform(translate * scale))
            {
                context.DrawGeometry(Foreground, null, _data);
            }
        }

        /// <summary>
        /// Quick and dirty check if we have a valid PathGeometry. This probably needs to be
        /// more robust, but this is better than a bunch of InvalidDataExceptions becase we 
        /// don't have a Path.TryParse() method. This does still fail sometimes, but its better
        /// than nothing. Its really only meant to be called from the StringToIconElementConverter
        /// </summary>
        public static bool IsDataValid(string data, out Geometry g)
        {
            if (data.Length <= 1 || data.Contains(":") || data.Contains("/\\"))
            {
                g = null;
                return false;
            }

            try
            {
                var first = data[0].ToString().ToUpper();

                var acceptFirst = new List<string>() { "M", "C", "L", "V", "H", "F" };

                if (acceptFirst.Contains(first) || data.Contains(" ") || data.Contains(","))
                {
                    g = StreamGeometry.Parse(data);
                    return true;
                }

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

        private Geometry _data;
    }
}

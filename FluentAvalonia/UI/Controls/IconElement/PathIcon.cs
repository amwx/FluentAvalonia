using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents an icon that uses a vector path as its content.
    /// </summary>
    public class PathIcon : IconElement
    {
        /// <summary>
        /// Defines the <see cref="Data"/> property
        /// </summary>
        public static StyledProperty<Geometry> DataProperty =
            AvaloniaProperty.Register<PathIcon, Geometry>(nameof(Data));

        /// <summary>
        /// Gets or sets a Geometry that specifies the shape to be drawn. 
        /// In XAML. this can also be set using a string that describes Move and draw commands syntax.
        /// </summary>
        public Geometry Data
        {
            get => GetValue(DataProperty);
			set => SetValue(DataProperty, value);
        }

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == DataProperty)
			{
				InvalidateMeasure();
				InvalidateVisual();
			}
		}

		protected override Size MeasureOverride(Size availableSize)
        {
            return Data?.Bounds.Size ?? base.MeasureOverride(availableSize);
        }

        public override void Render(DrawingContext context)
        {
			var data = Data;
            if (data == null)
                return;

            //Create scale & clip to make sure path is always drawn inside
            //the bounds specified
            var bounds = data.Bounds;
            var destRect = new Rect(Bounds.Size);
            var scale = Matrix.CreateScale(
                destRect.Width / bounds.Width,
                destRect.Height / bounds.Height);
            var translate = Matrix.CreateTranslation(-(Vector)bounds.Position);

            using (context.PushClip(destRect))
            using (context.PushPreTransform(translate * scale))
            {
                context.DrawGeometry(Foreground, null, data);
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
    }
}

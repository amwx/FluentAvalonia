using System;
using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents an icon that uses a vector path as its content.
/// </summary>
public partial class FAPathIcon : FAIconElement
{
    static FAPathIcon()
    {
        StretchProperty.OverrideDefaultValue<FAPathIcon>(Stretch.Uniform);
        StretchDirectionProperty.OverrideDefaultValue<FAPathIcon>(StretchDirection.Both);
        ClipToBoundsProperty.OverrideDefaultValue<FAPathIcon>(true);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StretchProperty || 
            change.Property == StretchDirectionProperty ||
            change.Property == DataProperty)
        {
            InvalidateMeasure();
        }
    }

    protected override Size MeasureOverride(Size availableSize) =>
        Data != null ? CalculateSizeAndTransform(availableSize, Data.Bounds, Stretch, StretchDirection).size :
        base.MeasureOverride(availableSize);

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Data != null)
        {
            var (_, transform) = CalculateSizeAndTransform(finalSize, Data.Bounds, Stretch, StretchDirection);

            if (_transform != transform)
            {
                _transform = transform;
            }

            return finalSize;
        }

        return Size.Empty;
    }

    private static (Size size, Matrix transform) CalculateSizeAndTransform(Size availableSize, Rect shapeBounds,
        Stretch stretch, StretchDirection stretchDirection)
    {
        Size shapeSize = new Size(shapeBounds.Right, shapeBounds.Bottom);
        Matrix translate = Matrix.Identity;
        double desiredX = availableSize.Width;
        double desiredY = availableSize.Height;
        double sx = 0.0;
        double sy = 0.0;

        if (stretch != Stretch.None)
        {
            shapeSize = shapeBounds.Size;
        }

        if (double.IsInfinity(availableSize.Width))
        {
            desiredX = shapeSize.Width;
        }

        if (double.IsInfinity(availableSize.Height))
        {
            desiredY = shapeSize.Height;
        }

        if (shapeBounds.Width > 0)
        {
            sx = desiredX / shapeSize.Width;
        }

        if (shapeBounds.Height > 0)
        {
            sy = desiredY / shapeSize.Height;
        }

        if (double.IsInfinity(availableSize.Width))
        {
            sx = sy;
        }

        if (double.IsInfinity(availableSize.Height))
        {
            sy = sx;
        }

        switch (stretch)
        {
            case Stretch.Uniform:
                sx = sy = Math.Min(sx, sy);
                break;

            case Stretch.UniformToFill:
                sx = sy = Math.Max(sx, sy);
                break;

            case Stretch.Fill:
                if (double.IsInfinity(availableSize.Width))
                {
                    sx = 1.0;
                }

                if (double.IsInfinity(availableSize.Height))
                {
                    sy = 1.0;
                }

                break;
            default:
                sx = sy = 1;
                break;
        }

        // Apply stretch direction by bounding scales.
        switch (stretchDirection)
        {
            case StretchDirection.UpOnly:
                if (sx < 1.0)
                    sx = 1.0;
                if (sy < 1.0)
                    sy = 1.0;
                break;

            case StretchDirection.DownOnly:
                if (sx > 1.0)
                    sx = 1.0;
                if (sy > 1.0)
                    sy = 1.0;
                break;

            default:
                break;
        }

        // Calculate translation in order to center the Icon
        switch (stretch)
        {
            case Stretch.None:
                translate = Matrix.CreateTranslation(
                    -shapeBounds.Position.X - (shapeBounds.Width - desiredX) / 2,
                    -shapeBounds.Position.Y - (shapeBounds.Height - desiredY) / 2);
                break;

            case Stretch.Uniform:
            case Stretch.UniformToFill:
                if (sx != 0 && sy != 0)
                {
                    translate = Matrix.CreateTranslation(
                        -shapeBounds.Position.X - (shapeBounds.Width * sx - desiredX) / sx / 2,
                        -shapeBounds.Position.Y - (shapeBounds.Height * sy - desiredY) / sy / 2);
                }
                else
                {
                    translate = Matrix.CreateTranslation(-(Vector)shapeBounds.Position);
                }

                break;

            case Stretch.Fill:
                translate = Matrix.CreateTranslation(-(Vector)shapeBounds.Position);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(Stretch), stretch, null);
        }

        var transform = translate * Matrix.CreateScale(sx, sy);
        var size = new Size(shapeSize.Width * sx, shapeSize.Height * sy);
        return (size, transform);
    }

    public override void Render(DrawingContext context)
    {
        var geometry = Data;//RenderedGeometry;
        if (geometry == null)
            return;

        using var s = context.PushPreTransform(_transform);

        context.DrawGeometry(Foreground, null, geometry);
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

    private Matrix _transform = Matrix.Identity;
}

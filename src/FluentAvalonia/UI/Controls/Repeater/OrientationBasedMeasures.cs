using Avalonia;

namespace FluentAvalonia.UI.Controls;

internal interface IOrientationBasedMeasures
{
    ScrollOrientation ScrollOrientation { get; set; }
}

internal static class OrientationBasedMeasuresExt
{   
    public static double Major(this IOrientationBasedMeasures m, Size size) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ? size.Height : size.Width;

    public static double Minor(this IOrientationBasedMeasures m, Size size) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ? size.Width : size.Height;

    public static double MajorSize(this IOrientationBasedMeasures m, Rect rect) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ? rect.Height : rect.Width;

    public static void SetMajorSize(this IOrientationBasedMeasures m, ref Rect rect, double value)
    {
        if (m.ScrollOrientation == ScrollOrientation.Vertical)
            rect = rect.WithHeight(value);
        else
            rect = rect.WithWidth(value);
    }

    public static double MinorSize(this IOrientationBasedMeasures m, Rect rect) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ? rect.Width : rect.Height;

    public static void SetMinorSize(this IOrientationBasedMeasures m, ref Rect rect, double value)
    {
        if (m.ScrollOrientation == ScrollOrientation.Vertical)
            rect = rect.WithWidth(value);
        else
            rect = rect.WithHeight(value);
    }

    public static double MajorStart(this IOrientationBasedMeasures m, Rect rect) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ? rect.Y : rect.X;

    public static double MajorEnd(this IOrientationBasedMeasures m, Rect rect) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ?
            rect.Y + rect.Height : rect.X + rect.Width;

    public static double MinorStart(this IOrientationBasedMeasures m, Rect rect) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ? rect.X : rect.Y;

    public static void SetMinorStart(this IOrientationBasedMeasures m, ref Rect rect, double value)
    {
        if (m.ScrollOrientation == ScrollOrientation.Vertical)
            rect = rect.WithX(value);
        else
            rect = rect.WithY(value);
    }

    public static void SetMajorStart(this IOrientationBasedMeasures m, ref Rect rect, double value)
    {
        if (m.ScrollOrientation == ScrollOrientation.Vertical)
            rect = rect.WithY(value);
        else
            rect = rect.WithX(value);
    }

    public static double MinorEnd(this IOrientationBasedMeasures m, Rect rect) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ?
            rect.X + rect.Width : rect.Y + rect.Height;

    public static Rect MinorMajorRect(this IOrientationBasedMeasures m, 
        double minor, double major, double minorSize, double majorSize) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ?
            new Rect(minor, major, minorSize, majorSize) :
            new Rect(major, minor, majorSize, minorSize);

    public static Point MinorMajorPoint(this IOrientationBasedMeasures m, double minor, double major) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ?
            new Point(minor, major) : new Point(major, minor);

    public static Size MinorMajorSize(this IOrientationBasedMeasures m, double minor, double major) =>
        m.ScrollOrientation == ScrollOrientation.Vertical ?
            new Size(minor, major) :
            new Size(major, minor);
}

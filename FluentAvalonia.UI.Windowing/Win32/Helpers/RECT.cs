namespace FluentAvalonia.UI.Windowing.Win32;

internal struct RECT
{
    public RECT(Avalonia.Rect rect)
    {
        left = (int)rect.X;
        top = (int)rect.Y;
        right = (int)(rect.X + rect.Width);
        bottom = (int)(rect.Y + rect.Height);
    }

    public RECT(int l, int t, int r, int b)
    {
        left = l;
        top = t;
        right = r;
        bottom = b;
    }

    public int left;
    public int top;
    public int right;
    public int bottom;

    public int Width => right - left;
    public int Height => bottom - top;   
}

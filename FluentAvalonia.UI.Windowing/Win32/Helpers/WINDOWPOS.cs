namespace FluentAvalonia.UI.Windowing.Win32;

internal struct WINDOWPOS
{
    public HWND hWnd;
    public HWND hWndInsertAfter;
    public int x;
    public int y;
    public int cx;
    public int cy;
    public uint flags;
}

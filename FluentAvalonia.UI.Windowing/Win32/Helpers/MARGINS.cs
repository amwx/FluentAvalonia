using System.Runtime.InteropServices;

namespace FluentAvalonia.UI.Windowing.Win32;

[StructLayout(LayoutKind.Sequential)]
internal struct MARGINS
{
    public int leftWidth;
    public int rightWidth;
    public int topHeight;
    public int bottomHeight;
}

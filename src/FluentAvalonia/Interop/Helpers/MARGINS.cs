using System.Runtime.InteropServices;

namespace FluentAvalonia.Interop.Win32;

[StructLayout(LayoutKind.Sequential)]
internal struct MARGINS
{
    public int leftWidth;
    public int rightWidth;
    public int topHeight;
    public int bottomHeight;
}

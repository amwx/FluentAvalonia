using System.Runtime.InteropServices;
using Avalonia.Media;
using FluentAvalonia.UI.Media;

namespace FluentAvalonia.Interop.WinRT;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct WinRTColor
{
    public byte A;
    public byte R;
    public byte G;
    public byte B;

    public static explicit operator Color2(WinRTColor color)
    {
        return new Color2(color.R, color.G, color.B, color.A);
    }

    public static explicit operator Color(WinRTColor color)
    {
        return new Color(color.A, color.R, color.G, color.B);
    }
}

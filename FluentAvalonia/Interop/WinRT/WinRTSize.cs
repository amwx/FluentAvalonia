using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FluentAvalonia.Interop.WinRT
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct WinRTSize
    {
        public float Width;
        public float Height;
    }
}

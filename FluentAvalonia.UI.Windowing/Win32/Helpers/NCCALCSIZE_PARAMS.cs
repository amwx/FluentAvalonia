// Adapted from TerraFX.Interop.Windows, MIT license

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FluentAvalonia.UI.Windowing.Win32;

#pragma warning disable 0649

internal unsafe struct NCCALCSIZE_PARAMS
{
    public _rgrc_e__FixedBuffer rgrc;
     
    public WINDOWPOS* lppos;

    public struct _rgrc_e__FixedBuffer
    {
        public RECT e0;
        public RECT e1;
        public RECT e2;

        public ref RECT this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref AsSpan()[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<RECT> AsSpan()
        {
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return MemoryMarshal.CreateSpan(ref e0, 3);
#else
            return new Span<RECT>(Unsafe.AsPointer(ref e0), 3);
#endif
        }
    }
}

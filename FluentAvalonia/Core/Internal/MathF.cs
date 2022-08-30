#if NETSTANDARD2_0
using System.Runtime.CompilerServices;

namespace System;

internal static class MathF
{
    public static float PI = 3.14159265f;

    public static float Sin(float radians) =>
        (float)Math.Sin(radians);

    public static float Cos(float radians) =>
        (float)Math.Cos(radians);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float val1, float val2) =>
        Math.Max(val1, val2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(float val1, float val2) =>
        Math.Min(val1, val2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(float value) =>
        Math.Abs(value);
}
#endif

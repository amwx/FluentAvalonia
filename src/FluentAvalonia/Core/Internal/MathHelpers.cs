namespace FluentAvalonia.Core;

/// <summary>
/// Maths Helpers
/// </summary>
public static class MathHelpers
{
    /// <summary>
    /// Returns <paramref name="value"/> clamped to the inclusive range of min and max.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="min">The lower bound of the result.</param>
    /// <param name="max">The upper bound of the result.</param>
    /// <returns><see cref="float"/></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greatest of <paramref name="max"/>.</exception>
    public static float Clamp(float value, float min, float max)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (min > max)
            throw new ArgumentException("min is greater than max");

        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
#endif
    }

    /// <summary>
    /// Returns <paramref name="value"/> clamped to the inclusive range of min and max.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="min">The lower bound of the result.</param>
    /// <param name="max">The upper bound of the result.</param>
    /// <returns><see cref="double"/></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greatest of <paramref name="max"/>.</exception>
    public static double Clamp(double value, double min, double max)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (min > max)
            throw new ArgumentException("min is greater than max");

        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
#endif
    }

    /// <summary>
    /// Returns <paramref name="value"/> clamped to the inclusive range of min and max.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="min">The lower bound of the result.</param>
    /// <param name="max">The upper bound of the result.</param>
    /// <returns><see cref="int"/></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greatest of <paramref name="max"/>.</exception>
    public static int Clamp(int value, int min, int max)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (min > max)
            throw new ArgumentException("min is greater than max");

        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
#endif
    }

    /// <summary>
    /// Returns <paramref name="value"/> clamped to the inclusive range of min and max.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="min">The lower bound of the result.</param>
    /// <param name="max">The upper bound of the result.</param>
    /// <returns><see cref="byte"/></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greatest of <paramref name="max"/>.</exception>
    public static byte Clamp(byte value, byte min, byte max)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (min > max)
            throw new ArgumentException("min is greater than max");

        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
#endif
    }

    /// <summary>
    /// Returns <paramref name="value"/> clamped to the inclusive range of min and max.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="min">The lower bound of the result.</param>
    /// <param name="max">The upper bound of the result.</param>
    /// <returns><see cref="long"/></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greatest of <paramref name="max"/>.</exception>
    public static long Clamp(long value, long min, long max)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (min > max)
            throw new ArgumentException("min is greater than max");

        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
#endif
    }



}

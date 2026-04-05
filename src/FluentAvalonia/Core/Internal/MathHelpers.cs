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
    [Obsolete("Use float.Clamp methods instead")]
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
    [Obsolete("Use double clamp methods instead")]
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
    [Obsolete("Use int.Clamp methods instead")]
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

    public static bool IsZero(double value, double eps = 1e-5) =>
        double.Abs(value) < eps;

    public static bool IsClose(double value1, double value2, double eps = 1e-5) =>
        double.Abs(value1 - value2) < eps;

}

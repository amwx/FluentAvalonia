using System;

namespace FluentAvalonia.Core
{
	public static class MathHelpers
	{
		public static float Clamp(float value, float min, float max)
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			return Math.Clamp(value, min,max);
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

		public static double Clamp(double value, double min, double max)
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			return Math.Clamp(value, min,max);
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

		public static int Clamp(int value, int min, int max)
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			return Math.Clamp(value, min,max);
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

		public static byte Clamp(byte value, byte min, byte max)
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			return Math.Clamp(value, min,max);
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

		public static long Clamp(long value, long min, long max)
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			return Math.Clamp(value, min,max);
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
}
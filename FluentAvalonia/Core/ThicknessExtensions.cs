using Avalonia;

namespace FluentAvalonia.Core
{
	public static class ThicknessExtensions
	{
		/// <summary>
		/// Retreives the total vertical thickness (top + bottom)
		/// </summary>
		public static double Vertical(this Thickness t)
		{
			return t.Top + t.Bottom;
		}

		/// <summary>
		/// Retreives the total horizontal thickness (left + right)
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static double Horizontal(this Thickness t)
		{
			return t.Left + t.Right;
		}
	}
}

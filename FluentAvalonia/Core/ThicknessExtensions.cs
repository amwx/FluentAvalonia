using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.Core
{
	public static class ThicknessExtensions
	{
		public static double Vertical(this Thickness t)
		{
			return t.Top + t.Bottom;
		}

		public static double Horizontal(this Thickness t)
		{
			return t.Left + t.Right;
		}
	}
}

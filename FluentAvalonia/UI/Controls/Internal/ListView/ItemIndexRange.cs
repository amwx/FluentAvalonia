using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public struct ItemIndexRange
	{
		public ItemIndexRange(int first, int count)
		{
			FirstIndex = first;
			LastIndex = first + count;
			Length = count;
		}

		public int FirstIndex { get; }
		public int LastIndex { get; }
		public int Length { get; }
	}
}

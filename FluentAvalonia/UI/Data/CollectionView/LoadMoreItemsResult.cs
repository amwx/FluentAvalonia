using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Data
{
	/// <summary>
	/// Wraps the asynchronous results of a LoadMoreItemsAsync call
	/// </summary>
	public struct LoadMoreItemsResult
	{
		/// <summary>
		/// The number of items that were actually loaded
		/// </summary>
		public uint Count;
	}
}

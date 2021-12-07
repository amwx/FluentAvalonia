using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Data
{
	public interface ICollectionViewFactory
	{
		ICollectionView CreateView();
	}
}

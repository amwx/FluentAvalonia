using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls
{
	public class DataTemplateSelector
	{
		public IDataTemplate SelectTemplate(object item) => SelectTemplateCore(item);

		public IDataTemplate SelectTemplate(object item, IControl container) => SelectTemplateCore(item, container);

		protected virtual IDataTemplate SelectTemplateCore(object item) => null;

		protected virtual IDataTemplate SelectTemplateCore(object item, IControl container) => null;
	}
}

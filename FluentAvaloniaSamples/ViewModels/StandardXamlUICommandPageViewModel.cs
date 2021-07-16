using Avalonia.Collections;
using FluentAvalonia.UI.Input;
using System;
using System.Collections.Generic;

namespace FluentAvaloniaSamples.ViewModels
{
	public class StandardXamlUICommandPageViewModel : ViewModelBase
	{
		public StandardXamlUICommandPageViewModel()
		{
			var coms = Enum.GetValues<StandardUICommandKind>();
			StandardCommands = new List<StandardCommandItem>(coms.Length);
			foreach(var item in coms)
			{
				if (item == StandardUICommandKind.None)
					continue;

				StandardCommands.Add(new StandardCommandItem
				{
					Name = item.ToString(),
					Command = new StandardUICommand(item)
				});
			}

			TempItems = new AvaloniaList<string>(10);
			for (int i = 0; i < 10; i++)
			{
				TempItems.Add($"Temp item {i + 1}");
			}
		}

		public string XamlSource => DescriptionServiceProvider.Instance.GetInfo("StandardUICommand", "XamlSource");
		public string CSharpSource => DescriptionServiceProvider.Instance.GetInfo("StandardUICommand", "CSharpSource");
		public string UsageNotes => DescriptionServiceProvider.Instance.GetInfo("StandardUICommand", "UsageNotes");
		public string Header => DescriptionServiceProvider.Instance.GetInfo("StandardUICommand", "Header");

		public IList<StandardCommandItem> StandardCommands { get; }

		public AvaloniaList<string> TempItems { get; set; }

		public void DeleteItem(object param)
		{
			if (param != null)
			{
				TempItems.Remove(param.ToString());
			}
		}

		public void AddItem()
		{
			TempItems.Add($"New Item {++_addCounter}");
		}

		private int _addCounter = 0;
	}

	public class StandardCommandItem
	{
		public string Name { get; set; }
		public StandardUICommand Command { get; set; }
	}
}

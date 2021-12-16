using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.Core.Attributes;
using System.ComponentModel;

namespace FluentAvalonia.UI.Controls
{
	public class GroupStyle : INotifyPropertyChanged
	{
		[NotImplemented]
		public ITemplate<IPanel> Panel
		{
			get => _panel;
			set
			{
				if (_panel != value)
				{
					_panel = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Panel)));
				}
			}
		}

		[NotImplemented]
		public bool HidesIfEmpty
		{
			get => _hidesIfEmpty;
			set
			{
				if (value != _hidesIfEmpty)
				{
					_hidesIfEmpty = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HidesIfEmpty)));
				}
			}
		}

		public DataTemplateSelector HeaderTemplateSelector
		{
			get => _headerTemplateSelector;
			set
			{
				if (_headerTemplateSelector != value)
				{
					_headerTemplateSelector = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeaderTemplateSelector)));
				}
			}
		}

		public IDataTemplate HeaderTemplate
		{
			get => _headerTemplate;
			set
			{
				if (_headerTemplate != value)
				{
					_headerTemplate = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeaderTemplate)));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private ITemplate<IPanel> _panel;
		private bool _hidesIfEmpty;
		private DataTemplateSelector _headerTemplateSelector;
		private IDataTemplate _headerTemplate;
	}
}

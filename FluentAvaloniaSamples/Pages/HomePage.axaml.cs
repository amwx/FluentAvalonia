using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;
using System;
using System.Linq;

namespace FluentAvaloniaSamples.Pages
{
    public class HomePage : UserControl
    {
        public HomePage()
        {
            this.InitializeComponent();

			var listBox = this.FindControl<ListBox>("WhatsNewListBox");
			if (listBox != null)
			{
				listBox.PointerReleased += OnListBoxPointerReleased;
			}
        }

		private void OnListBoxPointerReleased(object sender, PointerReleasedEventArgs e)
		{
			if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased &&
				e.InitialPressMouseButton == MouseButton.Left)
			{
				var lbi = ((IVisual)e.Source).FindAncestorOfType<ListBoxItem>();
				if (lbi != null)
				{
					var content = lbi.Content as ItemUpdateDescription;
					if (content != null)
					{
						var type = AppDomain.CurrentDomain.GetAssemblies()
							.Where(a => a.GetName().Name == "FluentAvaloniaSamples")
							.SelectMany(a => a.GetTypes())
							.FirstOrDefault(t => t.Name.Equals(content.PageType));

						if (type != null)
						{
							var frm = this.FindAncestorOfType<Frame>();
							if (frm != null)
							{
								frm.Navigate(type);
							}
						}
					}
				}
			}
		}

		private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

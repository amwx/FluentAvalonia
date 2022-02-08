using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FASandbox
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            var tv2 = this.FindControl<TabView>("TabView2");
            tv2.TabStripDragOver += Tv2_TabStripDragOver;
		}

        private void Tv2_TabStripDragOver(object sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Link;
        }

        private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}

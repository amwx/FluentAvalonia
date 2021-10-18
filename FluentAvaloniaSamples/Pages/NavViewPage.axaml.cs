using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Pages.NVSamplePages;
using FluentAvaloniaSamples.ViewModels;
using System;

namespace FluentAvaloniaSamples.Pages
{
    public class NavViewPage : UserControl
    {
        public NavViewPage()
        {
            this.InitializeComponent();

            DataContext = new NavViewPageViewModel();

			//NavigationView w/ default PaneDisplayMode
			var nv = this.FindControl<NavigationView>("nvSample");
			nv.SelectedItem = nv.MenuItems.ElementAt(0);
			nv.SelectionChanged += OnNVSample1SelectionChanged;

			//NavigationView w/ PaneDisplayMode set to Top
			nv = this.FindControl<NavigationView>("nvSample2");
			nv.SelectedItem = nv.MenuItems.ElementAt(0);
			nv.SelectionChanged += OnNVSample2SelectionChanged;

			//Adaptive
			nv = this.FindControl<NavigationView>("nvSample3");
			nv.SelectedItem = nv.MenuItems.ElementAt(0);
			nv.SelectionChanged += OnNVSample3SelectionChanged;

			//Tying selection and focus - Tabs
			nv = this.FindControl<NavigationView>("nvSample5");
			nv.SelectedItem = nv.MenuItems.ElementAt(0);
			nv.SelectionChanged += OnNVSample5SelectionChanged;

			//Databinding one is handled in ViewModel

			// With FooterMenuItems
			nv = this.FindControl<NavigationView>("nvSample9");
			nv.SelectedItem = nv.MenuItems.ElementAt(0);
			nv.SelectionChanged += OnNVSample9SelectionChanged;

			var rb = this.FindControl<RadioButton>("NV9Left");
			rb.Checked += NV9LeftRadioChecked;
			rb = this.FindControl<RadioButton>("NV9Top");
			rb.Checked += NV9TopRadioChecked;

			//API in action
			nv = this.FindControl<NavigationView>("nvSample6");
			nv.SelectedItem = nv.MenuItems.ElementAt(0);
			nv.SelectionChanged += OnNVSample6SelectionChanged;
		}

		private void OnNVSample9SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
		{
			// Settings is disabled here, so we don't need to check
			var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NV{(e.SelectedItem as NavigationViewItem).Tag}";
			var pg = Activator.CreateInstance(Type.GetType(smpPage));
			(sender as NavigationView).Content = pg;
		}

		private void NV9TopRadioChecked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			this.FindControl<NavigationView>("nvSample9").PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
		}

		private void NV9LeftRadioChecked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			this.FindControl<NavigationView>("nvSample9").PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
		}

		private void OnNVSample1SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.IsSettingsSelected)
            {
                (sender as NavigationView).Content = new NVSamplePageSettings();
            }
            else if (e.SelectedItem is NavigationViewItem nvi)
            {
                var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NV{nvi.Tag}";
                var pg = Activator.CreateInstance(Type.GetType(smpPage));
                (sender as NavigationView).Content = pg;
            }
        }

        private void OnNVSample2SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.IsSettingsSelected)
            {
                (sender as NavigationView).Content = new NVSamplePageSettings();
            }
            else if (e.SelectedItem is NavigationViewItem nvi)
            {
                var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NV{nvi.Tag}";
                var pg = Activator.CreateInstance(Type.GetType(smpPage));
                (sender as NavigationView).Content = pg;
            }
        }

        private void OnNVSample3SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.IsSettingsSelected)
            {
                (sender as NavigationView).Content = new NVSamplePageSettings();
            }
            else if (e.SelectedItem is NavigationViewItem nvi)
            {
                var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NV{nvi.Tag}";
                var pg = Activator.CreateInstance(Type.GetType(smpPage));
                (sender as NavigationView).Content = pg;
            }
        }

        private void OnNVSample5SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.IsSettingsSelected)
            {
                (sender as NavigationView).Content = new NVSamplePageSettings();
            }
            else if (e.SelectedItem is NavigationViewItem nvi)
            {
                var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NV{nvi.Tag}";
                var pg = Activator.CreateInstance(Type.GetType(smpPage));
                (sender as NavigationView).Content = pg;
            }
        }

        private void OnNVSample6SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.IsSettingsSelected)
            {
                (sender as NavigationView).Content = new NVSamplePageSettings();
            }
            else if (e.SelectedItem is NavigationViewItem nvi)
            {
                var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NV{nvi.Tag}";
                var pg = Activator.CreateInstance(Type.GetType(smpPage));
                (sender as NavigationView).Content = pg;
            }
        }





        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

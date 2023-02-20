using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.Pages.NVSamplePages;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class NavigationViewPage : FAControlsPageBase
{
    public NavigationViewPage()
    {
        InitializeComponent();

        TargetType = typeof(NavigationView);
        
        DataContext = new NavViewPageViewModel();

        // Default NavView
        var nv = this.FindControl<NavigationView>("nvSample1");
        nv.SelectionChanged += OnNVSample1SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);

        // Adaptive NavView
        nv = this.FindControl<NavigationView>("nvSample2");
        nv.SelectionChanged += OnNVSample2SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);

        // Binding NavView is handled by ViewModel

        // Hierarchical
        nv = this.FindControl<NavigationView>("nvSample4");
        nv.SelectionChanged += OnNVSample4SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);


        // FooterMenuItems
        nv = this.FindControl<NavigationView>("nvSample5");
        nv.SelectionChanged += OnNVSample5SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);


        // API in Action
        nv = this.FindControl<NavigationView>("nvSample6");
        nv.SelectionChanged += OnNVSample6SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);
    }

    private void OnNVSample1SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            (sender as NavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is NavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
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
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            (sender as NavigationView).Content = pg;
        }
    }

    private void OnNVSample4SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            (sender as NavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is NavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            (sender as NavigationView).Content = pg;
        }
    }

    private void OnNVSample5SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        // Settings is disabled here, so we don't need to check
        var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{(e.SelectedItem as NavigationViewItem).Tag}";
        var pg = Activator.CreateInstance(Type.GetType(smpPage));
        (sender as NavigationView).Content = pg;
    }

    private void OnNVSample6SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            (sender as NavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is NavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            (sender as NavigationView).Content = pg;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

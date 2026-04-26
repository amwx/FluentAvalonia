using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.Pages.NVSamplePages;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class NavigationViewPage : ControlsPageBase
{
    public NavigationViewPage()
    {
        InitializeComponent();

        TargetType = typeof(FANavigationView);
        
        DataContext = new NavViewPageViewModel();

        // Default NavView
        var nv = this.FindControl<FANavigationView>("nvSample1");
        nv.SelectionChanged += OnNVSample1SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);

        // Adaptive NavView
        nv = this.FindControl<FANavigationView>("nvSample2");
        nv.SelectionChanged += OnNVSample2SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);

        // Binding NavView is handled by ViewModel

        // Hierarchical
        nv = this.FindControl<FANavigationView>("nvSample4");
        nv.SelectionChanged += OnNVSample4SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);


        // FooterMenuItems
        nv = this.FindControl<FANavigationView>("nvSample5");
        nv.SelectionChanged += OnNVSample5SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);


        // API in Action
        nv = this.FindControl<FANavigationView>("nvSample6");
        nv.SelectionChanged += OnNVSample6SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);
    }

    private void OnNVSample1SelectionChanged(object sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            (sender as FANavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is FANavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            (sender as FANavigationView).Content = pg;
        }
    }

    private void OnNVSample2SelectionChanged(object sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            (sender as FANavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is FANavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            (sender as FANavigationView).Content = pg;
        }
    }

    private void OnNVSample4SelectionChanged(object sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            (sender as FANavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is FANavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            (sender as FANavigationView).Content = pg;
        }
    }

    private void OnNVSample5SelectionChanged(object sender, FANavigationViewSelectionChangedEventArgs e)
    {
        // Settings is disabled here, so we don't need to check
        var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{(e.SelectedItem as FANavigationViewItem).Tag}";
        var pg = Activator.CreateInstance(Type.GetType(smpPage));
        (sender as FANavigationView).Content = pg;
    }

    private void OnNVSample6SelectionChanged(object sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            (sender as FANavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is FANavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NV{nvi.Tag}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            (sender as FANavigationView).Content = pg;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

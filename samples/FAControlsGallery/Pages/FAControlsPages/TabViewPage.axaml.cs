using System;
using System.Collections;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.Pages.NVSamplePages;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class TabViewPage : ControlsPageBase
{
    public TabViewPage()
    {
        InitializeComponent();
        
        var txt = "Make sure this TabView is focused before trying these Keyboard shortcuts. (Click on first tab)\n";
        txt += $"- {new KeyGesture(Key.T, KeyModifiers.Control)} opens a new tab\n";
        txt += $"- {new KeyGesture(Key.W, KeyModifiers.Control)} opens a new tab\n";
        txt += $"- {new KeyGesture(Key.D1, KeyModifiers.Control)} to {new KeyGesture(Key.D8, KeyModifiers.Control)} selects that number tab\n";
        txt += $"- {new KeyGesture(Key.D9, KeyModifiers.Control)} selects the last tab\n";

        DataContext = new TabViewPageViewModel
        {
            KeyBindingText = txt
        };

        TargetType = typeof(FATabView);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TabView_AddButtonClick(FATabView sender, EventArgs args)
    {
        (sender.TabItems as IList).Add(CreateNewTab(sender.TabItems.Count()));
    }

    private void TabView_TabCloseRequested(FATabView sender, FATabViewTabCloseRequestedEventArgs args)
    {
        (sender.TabItems as IList).Remove(args.Tab);
    }

    private FATabViewItem CreateNewTab(int index)
    {
        var tvi = new FATabViewItem
        {
            Header = $"Document {index}",
            IconSource = new FASymbolIconSource { Symbol = FASymbol.Document }
        };

        switch (index % 3)
        {
            case 0:
                tvi.Content = new NVSamplePage2();
                break;

            case 1:
                tvi.Content = new NVSamplePage3();
                break;

            case 2:
                tvi.Content = new NVSamplePage6();
                break;
        }

        return tvi;
    }

    private void BindingTabView_TabCloseRequested(FATabView sender, FATabViewTabCloseRequestedEventArgs args)
    {
        (DataContext as TabViewPageViewModel).Documents.Remove(args.Item as DocumentItem);
    }

    private void LaunchWindowingSample(object sender, RoutedEventArgs args)
    {
        TabViewWindowingSample.LaunchRoot();
    }
}

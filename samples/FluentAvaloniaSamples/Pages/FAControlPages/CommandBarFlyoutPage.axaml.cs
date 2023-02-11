using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages;

public partial class CommandBarFlyoutPage : FAControlsPageBase
{
    public CommandBarFlyoutPage()
    {
        InitializeComponent();

        TargetType = typeof(CommandBarFlyout);
        WinUINamespace = "Microsoft.UI.Xaml.Controls.CommandBar";
        WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/uwp/api/microsoft.ui.xaml.controls.commandbarflyout?view=winrt-22000");
        WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/command-bar-flyout");
        Description = "A mini-toolbar which displays a set of proactive commands, as well as a secondary menu of commands if desired";

        DataContext = new CommandBarFlyoutPageViewModel();

        this.FindControl<Avalonia.Controls.Button>("myImageButton").ContextRequested += OnMyImageButtonContextRequested;
    }

    private void OnMyImageButtonContextRequested(object sender, ContextRequestedEventArgs e)
    {
        ShowMenu(false);
        e.Handled = true;
    }

    private void MyImageButton_Click(object sender, RoutedEventArgs args)
    {
        ShowMenu(true);
    }

    private void ShowMenu(bool isTransient)
    {
        var flyout = Resources["CommandBarFlyout1"] as CommandBarFlyout;
        flyout.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;

        flyout.ShowAt(this.FindControl<Image>("Image1"));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}

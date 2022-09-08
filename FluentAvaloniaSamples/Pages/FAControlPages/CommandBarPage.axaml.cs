using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
namespace FluentAvaloniaSamples.Pages;

public partial class CommandBarPage : FAControlsPageBase
{
    public CommandBarPage()
    {
        InitializeComponent();

        TargetType = typeof(CommandBar);
        WinUINamespace = "Microsoft.UI.Xaml.Controls.CommandBar";
        WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.commandbar");
        WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/command-bar");
        Description = "Command bars provide users with easy access to your app's most common tasks. Command bars can provide access to app-level or page-specific commands and can be used with any navigation pattern.";

        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}

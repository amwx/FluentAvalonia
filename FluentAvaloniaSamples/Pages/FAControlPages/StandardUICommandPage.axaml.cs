using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Input;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages;

public partial class StandardUICommandPage : FAControlsPageBase
{
    public StandardUICommandPage()
    {
        TargetType = typeof(StandardUICommand);
        WinUINamespace = "Microsoft.UI.Xaml.Input.StandardUICommand";
        WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.input.standarduicommand");
        WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/commanding#command-experiences-using-the-standarduicommand-class");
        Description = "StandardUICommands are a set of build-in XamlUICommands representing commonly used commands. Including the look and feel of a given command, which can be reused across your app, and which is understood natively by teh standard XAML controls, e.g., Save, Open, Copy, Paste, etc.";

        InitializeComponent();

        DataContext = new StandardUICommandPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

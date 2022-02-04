using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Input;

namespace FluentAvaloniaSamples.Pages
{
    public partial class XamlUICommandPage : FAControlsPageBase
    {
        public XamlUICommandPage()
        {
            TargetType = typeof(XamlUICommand);
            WinUINamespace = "Microsoft.UI.Xaml.Input.XamlUICommand";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.input.xamluicommand");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/commanding#command-experiences-using-the-xamluicommand-class");
            Description = "An object which is used to define the look and feel of a given command, which can be reused across your app, and which is understood natively the standard XAML controls.";

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CustomXamlUICommand_ExecuteRequested(XamlUICommand command, ExecuteRequestedEventArgs args)
        {
            counter++;
            this.FindControl<TextBlock>("XamlUICommandOutput").Text = $"You fired the custom command {counter} times";
        }

        int counter = 0;
    }
}

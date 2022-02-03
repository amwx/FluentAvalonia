using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Services;

namespace FluentAvaloniaSamples.Pages
{
    public partial class HyperlinkButtonPage : FAControlsPageBase
    {
        public HyperlinkButtonPage()
        {
            InitializeComponent();

            TargetType = typeof(HyperlinkButton);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.HyperlinkButton";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.hyperlinkbutton?view=winui-3.0");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/hyperlinks");
            Description = "A HyperlinkButton appears as a text hyperlink. When a user clicks it, it opens the page you specify in the NavigateUri property" +
                " in the default browser. Or you can handle its Click event, typically to navigate within your app";            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void HyperlinkButton_Click(object sender, RoutedEventArgs args)
        {
            NavigationService.Instance.Navigate(typeof(SettingsPage));
        }
    }
}

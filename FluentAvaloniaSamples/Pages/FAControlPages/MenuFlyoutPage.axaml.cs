using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
    public partial class MenuFlyoutPage : FAControlsPageBase
    {
        public MenuFlyoutPage()
        {
            InitializeComponent();

            TargetType = typeof(MenuFlyout);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.DropDownButton";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/uwp/api/microsoft.ui.xaml.controls.menuflyout");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/menus");
            Description = "A control that drops down a flyout of choices from which one can be chosen";


            DataContext = new MenuFlyoutPageViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

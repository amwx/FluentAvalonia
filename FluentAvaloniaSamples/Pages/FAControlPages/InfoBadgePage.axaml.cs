using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
    public partial class InfoBadgePage : FAControlsPageBase
    {
        public InfoBadgePage()
        {
            InitializeComponent();

            TargetType = typeof(InfoBadge);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.InfoBadge";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.infobadge");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/info-badge");
            Description = "Badging is a non-intrusive and intuitive way to display notifications or bring focus to an area within an app - whether that be for notifications, indicating new content, or showing an alert";


            DataContext = new InfoBadgePageViewModel();
        }



        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

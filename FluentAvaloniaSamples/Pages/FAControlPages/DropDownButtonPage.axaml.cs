using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.Pages
{
    public partial class DropDownButtonPage : FAControlsPageBase
    {
        public DropDownButtonPage()
        {
            InitializeComponent();

            TargetType = typeof(DropDownButton);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.DropDownButton";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.dropdownbutton?view=winui-3.0");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/buttons");
            Description = "A control that drops down a flyout of choices from which one can be chosen";


            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

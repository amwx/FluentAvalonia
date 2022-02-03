using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.Pages
{
    public partial class IconSourcePage : FAControlsPageBase
    {
        public IconSourcePage()
        {
            InitializeComponent();

            TargetType = typeof(IconSource);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.IconSource";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.iconsource?view=winui-3.0");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/style/icons");
            Description = "Represents the base class for shareable icon controls that use different image types as its content. IconSources can be declared as resources and used in many different areas of an app.";


            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

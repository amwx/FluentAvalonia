using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using ToggleSplitButton = FluentAvalonia.UI.Controls.ToggleSplitButton;

namespace FluentAvaloniaSamples.Pages
{
    public partial class ToggleSplitButtonPage : FAControlsPageBase
    {
        public ToggleSplitButtonPage()
        {
            InitializeComponent();

            TargetType = typeof(ToggleSplitButton);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.ToggleSplitButton";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.togglesplitbutton?view=winui-3.0");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/buttons");
            Description = "Represents a button with two parts that can be invoked separately. One part behaves like a standard button and the other part invokes a flyout.";
            XamlSource = "<ui:ToggleSplitButton Content=\"Toggle\">\n" +
                "    <ui:ToggleSplitButton.Flyout>\n" +
                "        <Flyout>\n" +
                "            <Grid Width=\"200\" Height=\"200\">\n" +
                "                <TextBlock Text=\"SplitButton Flyout!\" />\n" +
                "            </Grid>\n        </Flyout>\n    </ui:ToggleSplitButton.Flyout>\n</ui:ToggleSplitButton>";
            
            DataContext = this;
        }

        public string XamlSource { get; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

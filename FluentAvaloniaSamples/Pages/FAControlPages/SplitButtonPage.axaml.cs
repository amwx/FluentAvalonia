using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using static System.Net.Mime.MediaTypeNames;
using SplitButton = FluentAvalonia.UI.Controls.SplitButton;

namespace FluentAvaloniaSamples.Pages
{
    public partial class SplitButtonPage : FAControlsPageBase
    {
        public SplitButtonPage()
        {
            InitializeComponent();

            TargetType = typeof(SplitButton);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.SplitButton";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.splitbutton?view=winui-3.0");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/buttons#create-a-split-button");
            Description = "Represents a button with two parts that can be invoked separately. One part behaves like a standard button and the other part invokes a flyout.";
            XamlSource = "<ui:SplitButton Content=\"Toggle\">\n" +
                "    <ui:SplitButton.Flyout>\n" +
                "        <Flyout>\n" +
                "            <Grid Width=\"200\" Height=\"200\">\n" +
                "                <TextBlock Text=\"SplitButton Flyout!\" />\n" +
                "            </Grid>\n        </Flyout>\n    </ui:SplitButton.Flyout>\n</ui:SplitButton>";
            
            DataContext = this;
        }

        public string XamlSource { get; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

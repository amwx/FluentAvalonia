using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvaloniaSamples.Pages.NVSamplePages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.Pages
{
    public class FramePage : UserControl
    {
        public FramePage()
        {
            InitializeComponent();

            _frame = this.FindControl<Frame>("TestFrame");
            PageOptions = new List<string>
            {
                "Sample Page 1\n(Default Animation)",
                "Sample Page 2\n(Slide Animation)",
                "Sample Page 3\n(DrillIn Animation)",
                "Sample Page 4\n(No Animation)"
            };

            var lb = this.FindControl<ListBox>("PageSelection");
            lb.SelectionChanged += OnListBoxSelectionChanged;

            XElement xe = XElement.Parse(GetAssemblyResource("FluentAvaloniaInfo.txt"));
            var pages = xe.Elements("ControlPage").Where(x => x.Attribute("Name").Value == "Frame").First();

            Header = pages.Element("Header").Value;
            var controls = pages.Elements("Control");
            foreach (var ctrl in controls)
            {
                if (ctrl.Attribute("Name").Value == "Frame")
                {
                    XamlSource = ctrl.Element("XamlSource").Value;
                    CSharpSource = ctrl.Element("CSharpSource").Value;
                }                
            }

            DataContext = this;
        }

        public List<string> PageOptions { get; }

        public string Header { get; }
        public string XamlSource { get; }
        public string CSharpSource { get; }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {        
            switch ((sender as ListBox).SelectedIndex)
            {
                case 0:
                    _frame.Navigate(typeof(NVSamplePage1));
                    break;

                case 1:
                    _frame.Navigate(typeof(NVSamplePage2), null, new SlideNavigationTransitionInfo());
                    break;

                case 2:
                    _frame.Navigate(typeof(NVSamplePage3), null, new DrillInNavigationTransitionInfo());
                    break;

                case 3:
                    _frame.Navigate(typeof(NVSamplePage4), null, new SuppressNavigationTransitionInfo());
                    break;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected string GetAssemblyResource(string name)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using (var stream = assets.Open(new Uri($"avares://FluentAvaloniaSamples/DescriptionTexts/{name}")))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private Frame _frame;
    }
}

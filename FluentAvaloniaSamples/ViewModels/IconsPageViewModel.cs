using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class IconsPageViewModel : ViewModelBase
    {
        public IconsPageViewModel()
        {
            Symbols = new List<string>();
            foreach (var item in Enum.GetValues(typeof(Symbol)))
            {
                Symbols.Add(item.ToString());
            }

            XElement xe = XElement.Parse(GetAssemblyResource("FluentAvaloniaInfo.txt"));
            var pages = xe.Elements("ControlPage").Where(x => x.Attribute("Name").Value == "Icons").First();

            Header = pages.Element("Header").Value;
            var controls = pages.Elements("Control");
            foreach (var ctrl in controls)
            {
                if (ctrl.Attribute("Name").Value == "SymbolIcon")
                {
                    SymbolIconXaml = ctrl.Element("XamlSource").Value;
                    SymbolIconNotes = ctrl.Element("UsageNotes").Value;
                }
                else if (ctrl.Attribute("Name").Value == "FontIcon")
                {
                    FontIconXaml = ctrl.Element("XamlSource").Value;
                    FontIconNotes = ctrl.Element("UsageNotes").Value;
                }
                else if (ctrl.Attribute("Name").Value == "PathIcon")
                {
                    PathIconXaml = ctrl.Element("XamlSource").Value;
                }
                else if (ctrl.Attribute("Name").Value == "BitmapIcon")
                {
                    BitmapIconXaml = ctrl.Element("XamlSource").Value;
                    BitmapIconNotes = ctrl.Element("UsageNotes").Value;
                }
            }
        }

        public List<string> Symbols { get; }

        public string Header { get; }

        public string SymbolIconXaml { get; }
        public string SymbolIconNotes { get; }

        public string FontIconXaml { get; }
        public string FontIconNotes { get; }

        public string PathIconXaml { get; }

        public string BitmapIconXaml { get; }
        public string BitmapIconNotes { get; }

    }
}

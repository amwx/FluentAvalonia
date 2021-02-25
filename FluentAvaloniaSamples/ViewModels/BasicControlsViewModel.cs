using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class BasicControlsViewModel : ViewModelBase
    {
        public BasicControlsViewModel()
        {
            ComboBoxItems = new List<string> { "Item 1", "Item 2", "Item 3" };

            XElement xe = XElement.Parse(GetAssemblyResource("FluentAvaloniaInfo.txt"));
            var pages = xe.Elements("ControlPage").Where(x => x.Attribute("Name").Value == "Basic Controls").First();

            Header = pages.Element("Header").Value;
            var controls = pages.Elements("Control");
            foreach(var ctrl in controls)
            {
                if (ctrl.Attribute("Name").Value == "ButtonFlyout")
                {
                    ButtonFlyoutXaml = ctrl.Element("XamlSource").Value;
                    ButtonFlyoutNotes = ctrl.Element("UsageNotes").Value;
                }
                else if (ctrl.Attribute("Name").Value == "SplitButton")
                {
                    SplitButtonXaml = ctrl.Element("XamlSource").Value;
                }
                else if (ctrl.Attribute("Name").Value == "ToggleSplitButton")
                {
                    ToggleSplitButtonXaml = ctrl.Element("XamlSource").Value;
                }
                else if (ctrl.Attribute("Name").Value == "DropDownButton")
                {
                    DropDownButtonXaml = ctrl.Element("XamlSource").Value;
                    //DropDownButtonNotes = ctrl.Element("UsageNotes").Value;
                }
                else if (ctrl.Attribute("Name").Value == "ComboBox")
                {
                    ComboBoxXaml = ctrl.Element("XamlSource").Value;
                    ComboBoxNotes = ctrl.Element("UsageNotes").Value;
                }
                else if (ctrl.Attribute("Name").Value == "Expander")
                {
                    ExpanderNotes = ctrl.Element("UsageNotes").Value;
                }
            }
        }

        public List<string> ComboBoxItems { get; }

        public string Header { get; }

        public string ButtonFlyoutXaml { get; }
        public string ButtonFlyoutNotes { get; }

        public string SplitButtonXaml { get; }

        public string ToggleSplitButtonXaml { get; }

        public string DropDownButtonXaml { get; }
        //public string DropDownButtonNotes { get; }

        public string ComboBoxXaml { get; }
        public string ComboBoxNotes { get; }

        public string ExpanderNotes { get; }
    }
}

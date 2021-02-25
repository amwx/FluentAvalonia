using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class NumberBoxPageViewModel : ViewModelBase
    {
        public NumberBoxPageViewModel()
        {
            XElement xe = XElement.Parse(GetAssemblyResource("FluentAvaloniaInfo.txt"));
            var pages = xe.Elements("ControlPage").Where(x => x.Attribute("Name").Value == "NumberBox").First();

            Header = pages.Element("Header").Value;
            var controls = pages.Elements("Control");
            foreach (var ctrl in controls)
            {
                if (ctrl.Attribute("Name").Value == "NumberBoxExpression")
                {
                    ExpressionXaml = ctrl.Element("XamlSource").Value;
                }
                else if (ctrl.Attribute("Name").Value == "NumberBoxSpin")
                {
                    SpinXaml = ctrl.Element("XamlSource").Value;
                }
                else if (ctrl.Attribute("Name").Value == "NumberBoxFormat")
                {
                    FormattedXaml = ctrl.Element("XamlSource").Value;
                    FormattedCSharp = ctrl.Element("CSharpSource").Value;
                    UsageNotes = ctrl.Element("UsageNotes").Value;
                }
            }
        }


        public NumberBoxSpinButtonPlacementMode SpinPlacementMode
        {
            get => spinPlacement;
            set => RaiseAndSetIfChanged(ref spinPlacement, value);
        }

        private NumberBoxSpinButtonPlacementMode spinPlacement = NumberBoxSpinButtonPlacementMode.Inline;

        public string Header { get; }

        public string ExpressionXaml { get; }

        public string SpinXaml { get; }

        public string FormattedXaml { get; }
        public string FormattedCSharp { get; }
        public string UsageNotes { get; }
    }
}

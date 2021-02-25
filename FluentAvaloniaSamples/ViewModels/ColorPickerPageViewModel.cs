using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class ColorPickerPageViewModel : ViewModelBase
    {
        public ColorPickerPageViewModel()
        {
            Components = new List<ColorSpectrumComponents>(Enum.GetValues(typeof(ColorSpectrumComponents)).Cast<ColorSpectrumComponents>().ToList());
            SpectrumShapes = new List<ColorSpectrumShape>(Enum.GetValues(typeof(ColorSpectrumShape)).Cast<ColorSpectrumShape>().ToList());

            XElement xe = XElement.Parse(GetAssemblyResource("FluentAvaloniaInfo.txt"));
            var pages = xe.Elements("ControlPage").Where(x => x.Attribute("Name").Value == "ColorPicker").First();

            //Header = pages.Element("Header").Value;
            var controls = pages.Elements("Control");
            foreach (var ctrl in controls)
            {
                if (ctrl.Attribute("Name").Value == "StandardColorPicker")
                {
                    ColorPickerXaml = ctrl.Element("XamlSource").Value;
                    ColorPickerNotes = ctrl.Element("UsageNotes").Value;
                }
                else if (ctrl.Attribute("Name").Value == "ColorPickerButton")
                {
                    ColorButtonXaml = ctrl.Element("XamlSource").Value;
                    ColorButtonNotes = ctrl.Element("UsageNotes").Value;
                }
            }
        }

        //public string Header { get; }

        public string ColorPickerXaml { get; }
        public string ColorPickerNotes { get; }

        public string ColorButtonXaml { get; }
        public string ColorButtonNotes { get; }

        public List<ColorSpectrumComponents> Components { get; }
        public List<ColorSpectrumShape> SpectrumShapes { get; }

        public void SetColorToWhite(ColorPicker cp)
        {
            cp.Color = Colors.White;
        }

        public void SetColorToRed(ColorPicker cp)
        {
            cp.Color = Colors.Red;
        }

        public void SetColorToGreen(ColorPicker cp)
        {
            cp.Color = Colors.Green;
        }

        public void SetColorToBlue(ColorPicker cp)
        {
            cp.Color = Colors.Blue;
        }
    }
}

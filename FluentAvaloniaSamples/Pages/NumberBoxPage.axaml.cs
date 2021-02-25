using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;
using System;

namespace FluentAvaloniaSamples.Pages
{
    public class NumberBoxPage : UserControl
    {
        public NumberBoxPage()
        {
            InitializeComponent();

            DataContext = new NumberBoxPageViewModel();

            var nm = this.FindControl<NumberBox>("FormattedNB");
            nm.NumberFormatter = (input) =>
            {
                double increment = 1/0.25;
                return (Math.Round(input * increment, MidpointRounding.AwayFromZero) / increment).ToString("F2");
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

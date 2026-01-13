using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class NumberBoxPage : ControlsPageBase
{
    public NumberBoxPage()
    {
        InitializeComponent();

        TargetType = typeof(NumberBox);
        
        var nm = this.FindControl<NumberBox>("FormattedNumBox");
        nm.NumberFormatter = (input) =>
        {
            double increment = 1 / 0.25;
            return (Math.Round(input * increment, MidpointRounding.AwayFromZero) / increment).ToString("F2");
        };

        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void FocusButtonOnClick(object sender, RoutedEventArgs e)
    {
        var numBoxToFocus = this.FindControl<NumberBox>("NumBoxToFocus");
        numBoxToFocus?.Focus();
    }
}

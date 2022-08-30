using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.Pages;

public partial class NumberBoxPage : FAControlsPageBase
{
    public NumberBoxPage()
    {
        InitializeComponent();

        TargetType = typeof(NumberBox);
        WinUINamespace = "Microsoft.UI.Xaml.Controls.NumberBox";
        WinUIDocsLink = new Uri("https://docs.microsoft.com/uwp/api/microsoft.ui.xaml.controls.numberbox");
        WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/number-box");
        Description = "Use NumberBox to allow users to enter algebraic equations and numeric input in your app";

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
}

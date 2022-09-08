using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvaloniaSamples.Pages;

public partial class PickerFlyoutBasePage : FAControlsPageBase
{
    public PickerFlyoutBasePage()
    {
        InitializeComponent();

        TargetType = typeof(PickerFlyoutBase);
        WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.primitives.pickerflyoutbase?view=winui-3.0");
        Description = "Represents a base class for flyouts that present an option to the user allowing them to confirm or dismiss the Flyout.";

        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

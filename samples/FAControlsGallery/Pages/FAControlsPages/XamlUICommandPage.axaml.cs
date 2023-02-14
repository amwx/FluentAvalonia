using Avalonia.Controls;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Input;

namespace FAControlsGallery.Pages;

public partial class XamlUICommandPage : FAControlsPageBase
{
    public XamlUICommandPage()
    {
        InitializeComponent();

        DataContext = new StandardUICommandPageViewModel();
    }

    public void CustomXamlUICommand_ExecuteRequested(XamlUICommand command, ExecuteRequestedEventArgs args)
    {
        counter++;
        this.FindControl<TextBlock>("XamlUICommandOutput").Text = $"You fired the custom command {counter} times";
    }

    int counter = 0;
}

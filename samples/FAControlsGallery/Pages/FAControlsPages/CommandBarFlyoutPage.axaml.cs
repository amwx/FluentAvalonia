using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.ViewModels;
using Avalonia.Input;

namespace FAControlsGallery.Pages;

public partial class CommandBarFlyoutPage : ControlsPageBase
{
    public CommandBarFlyoutPage()
    {
        InitializeComponent();
               
        DataContext = new CommandBarFlyoutPageViewModel();
        TargetType = typeof(FACommandBarFlyout);

        this.FindControl<Button>("myImageButton").ContextRequested += OnMyImageButtonContextRequested;
    }

    private void OnMyImageButtonContextRequested(object sender, ContextRequestedEventArgs e)
    {
        ShowMenu(false);
        e.Handled = true;
    }

    private void MyImageButton_Click(object sender, RoutedEventArgs args)
    {
        ShowMenu(true);
    }

    private void ShowMenu(bool isTransient)
    {
        var flyout = Resources["CommandBarFlyout1"] as FACommandBarFlyout;
        flyout.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;

        flyout.ShowAt(this.FindControl<Image>("Image1"));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}

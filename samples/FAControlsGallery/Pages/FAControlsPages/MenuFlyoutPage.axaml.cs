using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class MenuFlyoutPage : ControlsPageBase
{
    public MenuFlyoutPage()
    {
        InitializeComponent();

        TargetType = typeof(FAMenuFlyout);        
        DataContext = new MenuFlyoutPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

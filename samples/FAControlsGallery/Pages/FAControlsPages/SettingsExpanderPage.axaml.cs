using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class SettingsExpanderPage : FAControlsPageBase
{
    public SettingsExpanderPage()
    {
        InitializeComponent();

        DataContext = new SettingsExpanderPageViewModel();
    }
}

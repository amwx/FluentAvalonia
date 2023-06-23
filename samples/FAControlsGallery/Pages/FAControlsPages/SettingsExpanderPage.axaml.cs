using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class SettingsExpanderPage : ControlsPageBase
{
    public SettingsExpanderPage()
    {
        InitializeComponent();

        DataContext = new SettingsExpanderPageViewModel();
        TargetType = typeof(SettingsExpander);
    }
}

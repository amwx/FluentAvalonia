using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class SettingsExpanderPage : FAControlsPageBase
{
    public SettingsExpanderPage()
    {
        InitializeComponent();

        DataContext = new SettingsExpanderPageViewModel();
        TargetType = typeof(SettingsExpander);
    }
}

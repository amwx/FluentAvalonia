using Avalonia.Controls;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;
public partial class FAComboBoxPage : ControlsPageBase
{
    public FAComboBoxPage()
    {
        InitializeComponent();

        DataContext = new FAComboBoxPageViewModel();
        TargetType = typeof(FAComboBox);
    }
}

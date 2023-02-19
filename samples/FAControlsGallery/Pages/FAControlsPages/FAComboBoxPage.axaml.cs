using Avalonia.Controls;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;
public partial class FAComboBoxPage : FAControlsPageBase
{
    public FAComboBoxPage()
    {
        InitializeComponent();

        DataContext = new FAComboBoxPageViewModel();
    }
}

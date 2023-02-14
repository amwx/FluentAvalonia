using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class IconsPage : FAControlsPageBase
{
    public IconsPage()
    {
        InitializeComponent();

        DataContext = new IconElementPageViewModel();
    }
}

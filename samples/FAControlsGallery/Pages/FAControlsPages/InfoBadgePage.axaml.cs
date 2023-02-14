using Avalonia.Markup.Xaml;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class InfoBadgePage : FAControlsPageBase
{
    public InfoBadgePage()
    {
        InitializeComponent();

        DataContext = new InfoBadgePageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

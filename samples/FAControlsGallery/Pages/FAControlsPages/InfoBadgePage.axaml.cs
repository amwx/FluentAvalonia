using Avalonia.Markup.Xaml;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class InfoBadgePage : ControlsPageBase
{
    public InfoBadgePage()
    {
        InitializeComponent();

        DataContext = new InfoBadgePageViewModel();
        TargetType = typeof(FAInfoBadge);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

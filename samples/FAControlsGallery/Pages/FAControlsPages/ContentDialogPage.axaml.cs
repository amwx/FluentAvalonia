using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class ContentDialogPage : FAControlsPageBase
{
    public ContentDialogPage()
    {
        InitializeComponent();

        DataContext = new ContentDialogPageViewModel();
        TargetType = typeof(ContentDialog);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

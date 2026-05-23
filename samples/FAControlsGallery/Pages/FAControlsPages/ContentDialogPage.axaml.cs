using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class ContentDialogPage : ControlsPageBase
{
    public ContentDialogPage()
    {
        InitializeComponent();

        DataContext = new ContentDialogPageViewModel();
        TargetType = typeof(FAContentDialog);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

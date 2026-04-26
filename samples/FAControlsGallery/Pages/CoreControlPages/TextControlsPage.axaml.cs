using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class TextControlsPage : ControlsPageBase
{
    public TextControlsPage()
    {
        InitializeComponent();
        ControlName = "Text Controls";
        App.Current.Resources.TryGetResource("TextPageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;

        DataContext = new TextControlsPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

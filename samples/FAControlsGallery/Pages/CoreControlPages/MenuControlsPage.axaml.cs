using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class MenuControlsPage : ControlsPageBase
{
    public MenuControlsPage()
    {
        InitializeComponent();
        ControlName = "Menu Controls";
        App.Current.Resources.TryGetResource("MenusPageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

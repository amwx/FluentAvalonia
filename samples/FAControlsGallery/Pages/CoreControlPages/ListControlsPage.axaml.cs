using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class ListControlsPage : ControlsPageBase
{
    public ListControlsPage()
    {
        InitializeComponent();
        ControlName = "List Controls";
        App.Current.Resources.TryGetResource("ListPageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

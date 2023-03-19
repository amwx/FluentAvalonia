using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class ViewControlsPage : ControlsPageBase
{
    public ViewControlsPage()
    {
        InitializeComponent();

        ControlName = "View Controls";
        App.Current.Resources.TryGetResource("ViewPageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class MiscControlsPage : ControlsPageBase
{
    public MiscControlsPage()
    {
        InitializeComponent();
        ControlName = "Miscellaneous Controls";
        App.Current.Resources.TryGetResource("MiscPageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

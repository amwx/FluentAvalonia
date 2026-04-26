using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class BasicInputControlsPage : ControlsPageBase
{
    public BasicInputControlsPage()
    {
        InitializeComponent();

        ControlName = "Basic Input Controls";
        App.Current.Resources.TryGetResource("BasicInputPageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

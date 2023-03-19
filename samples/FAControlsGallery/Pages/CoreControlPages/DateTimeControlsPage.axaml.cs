using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class DateTimeControlsPage : ControlsPageBase
{
    public DateTimeControlsPage()
    {
        InitializeComponent();
        ControlName = "Date / Time Controls";
        App.Current.Resources.TryGetResource("DatePageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

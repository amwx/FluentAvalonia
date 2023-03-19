using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls.Primitives;

namespace FAControlsGallery.Pages;

public partial class PickerFlyoutBasePage : ControlsPageBase
{
    public PickerFlyoutBasePage()
    {
        InitializeComponent();

        TargetType = typeof(PickerFlyoutBase);
        
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Immutable;

namespace FAControlsGallery.Pages;

public partial class ProgressRingPage : ControlsPageBase
{
    public ProgressRingPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        IndetRing.IsIndeterminate = true;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        IndetRing.IsIndeterminate = false;
    }

    private void ColorPicker_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (sender == CPIndet)
        {
            IndetRing.Background = new ImmutableSolidColorBrush(e.NewColor);
        }
        else if (sender == CP1)
        {
            DetRing.Background = new ImmutableSolidColorBrush(e.NewColor);
        }
    }
}

using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;
public partial class TeachingTipPage : FAControlsPageBase
{
    public TeachingTipPage()
    {
        InitializeComponent();

        Button1.Click += Button1Click;
        Button2.Click += Button2Click;
        Button3.Click += Button3Click;

        TargetType = typeof(TeachingTip);
    }

    private void Button1Click(object sender, RoutedEventArgs e)
    {
        TeachingTip1.IsOpen = true;
    }

    private void Button2Click(object sender, RoutedEventArgs e)
    {
        TeachingTip2.IsOpen = true;
    }

    private void Button3Click(object sender, RoutedEventArgs e)
    {
        TeachingTip3.IsOpen = true;
    }
}

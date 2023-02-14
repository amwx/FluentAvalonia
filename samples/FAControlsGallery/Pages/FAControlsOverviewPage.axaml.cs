using System.Security.AccessControl;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using FAControlsGallery.Services;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class FAControlsOverviewPage : UserControl
{
    public FAControlsOverviewPage()
    {
        InitializeComponent();

        Tapped += FAControlsOverviewPageTapped;
    }

    private void FAControlsOverviewPageTapped(object sender, TappedEventArgs e)
    {
        if (e.Source is Visual v)
        {
            var lbi = v.FindAncestorOfType<ListBoxItem>(true);
            if (lbi != null && lbi.DataContext is FAControlsPageItem fci)
            {
                NavigationService.Instance.NavigateFromContext(fci);
            }
        }
    }
}

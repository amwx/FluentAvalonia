using Avalonia.Controls;
using Avalonia.Interactivity;
using FAControlsGallery.Pages.DesignPages;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.UI.Media.Animation;

namespace FAControlsGallery.Pages;

public partial class DesignPage : UserControl
{
    public DesignPage()
    {
        InitializeComponent();

        TabStrip1.SelectionChanged += TabStrip1SelectionChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        TabStrip1SelectionChanged(null, null);
    }

    private void TabStrip1SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var vm = DataContext as DesignPageViewModel;

#if DEBUG
        if (vm == null)
            return;
#endif

        var idx = TabStrip1.SelectedIndex;

        InnerNavFrame.Navigate(idx switch
        {
            0 => typeof(TypographyPage),
            1 => typeof(DesignIconsPage),
            2 => typeof(ColorsPage),
            _ => throw new Exception()
        }, null, GetTransitionInfo(vm.LastSelectedIndex, idx));

        vm.LastSelectedIndex = TabStrip1.SelectedIndex;
    }

    private SlideNavigationTransitionEffect GetEffect(int oldIndex, int index)
    {
        if (oldIndex < 0)
            return SlideNavigationTransitionEffect.FromBottom;

        if (oldIndex > index)
            return SlideNavigationTransitionEffect.FromRight;
        else
            return SlideNavigationTransitionEffect.FromLeft;
    }

    private NavigationTransitionInfo GetTransitionInfo(int oldIndex, int newIndex)
    {
        if (oldIndex == -1)
        {
            return new SuppressNavigationTransitionInfo();
        }
        else
        {
            return new SlideNavigationTransitionInfo
            {
                Effect = GetEffect(oldIndex, newIndex),
                FromHorizontalOffset = 70
            };
        }
    }
}

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

    private FASlideNavigationTransitionEffect GetEffect(int oldIndex, int index)
    {
        if (oldIndex < 0)
            return FASlideNavigationTransitionEffect.FromBottom;

        if (oldIndex > index)
            return FASlideNavigationTransitionEffect.FromRight;
        else
            return FASlideNavigationTransitionEffect.FromLeft;
    }

    private FANavigationTransitionInfo GetTransitionInfo(int oldIndex, int newIndex)
    {
        if (oldIndex == -1)
        {
            return new FASuppressNavigationTransitionInfo();
        }
        else
        {
            return new FASlideNavigationTransitionInfo
            {
                Effect = GetEffect(oldIndex, newIndex),
                FromHorizontalOffset = 70
            };
        }
    }
}

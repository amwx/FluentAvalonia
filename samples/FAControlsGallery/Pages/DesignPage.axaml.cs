using Avalonia.Controls;
using FAControlsGallery.Pages.DesignPages;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace FAControlsGallery.Pages;

public partial class DesignPage : UserControl
{
    public DesignPage()
    {
        InitializeComponent();

        TabStrip1.SelectionChanged += TabStrip1SelectionChanged;
        InnerNavFrame.NavigationPageFactory = new DesignPageFactory();
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        TabStrip1.SelectedIndex = 0;
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

        InnerNavFrame.NavigateFromObject(idx switch
                    {
                        0 => "Typography",
                        1 => "Icons",
                        2 => "Colors",
                        _ => throw new Exception()
                    },
                    new FrameNavigationOptions
                    {
                        TransitionInfoOverride = new SlideNavigationTransitionInfo
                        {
                            Effect = GetEffect(vm.LastSelectedIndex, idx),
                            FromHorizontalOffset = 70
                        }
                    });

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

    private class DesignPageFactory : INavigationPageFactory
    {
        public Control GetPage(Type srcType) => null;

        public Control GetPageFromObject(object target)
        {
            if (target.Equals("Typography"))
            {
                return new TypographyPage();
            }
            else if (target.Equals("Icons"))
            {
                //return new DesignIconsPage();
            }
            else if (target.Equals("Colors"))
            {
                return new ColorsPage();
            }

            return null;
        }
    }
}

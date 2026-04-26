using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FAControlsGallery.Services;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Experimental;
using FluentAvalonia.UI.Navigation;

namespace FAControlsGallery.Pages;

public partial class FAControlsOverviewPage : UserControl
{
    public FAControlsOverviewPage()
    {
        InitializeComponent();
        // Use the frame events here to ensure ConnectedAnimations still work with
        // Back/Forward navigation and not just explicit page invokes
        AddHandler(Frame.NavigatingFromEvent, OnNavigatingFrom, RoutingStrategies.Direct);
        AddHandler(Frame.NavigatedToEvent, OnNavigatedTo, RoutingStrategies.Direct);

        Tapped += FAControlsOverviewPageTapped;
    }

    private void FAControlsOverviewPageTapped(object sender, TappedEventArgs e)
    {
        if (e.Source is Visual v)
        {
            var lbi = v.FindAncestorOfType<ListBoxItem>(true);
            if (lbi != null && lbi.DataContext is FAControlsPageItem fci)
            {
                var item = lbi.GetVisualDescendants()
                    .Where(x => x is Viewbox && x.Name == "IconHost")
                    .FirstOrDefault();
                _animationPage = fci;

                NavigationService.Instance.NavigateFromContext(fci);
            }
        }
    }

    private void OnNavigatedTo(object sender, NavigationEventArgs e)
    {
        if (_animationPage == null)
            return;

        var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
        var anim = svc.GetAnimation("BackAnimation");

        if (anim == null)
            return;

        var item = this.GetVisualDescendants()
                    .Where(x => x is ListBoxItem && x.DataContext == _animationPage)
                    .FirstOrDefault()
                    .GetVisualDescendants()
                    .Where(x => x is Viewbox && x.Name == "IconHost")
                    .FirstOrDefault();
        var presenter = item;// GetAnimationSource();

        // In WinUI, ConnectedAnimation is somehow exempt from all clipping behaviors
        // Here, we are not, so disable ClipToBounds on all elements in the SettingsExpander
        // The rest are taken care of in the xaml.
        // NOTE: The ScrollViewer is not changed here as that's important for scrolling - thus
        // the animation will be cut off, but the back animation is pretty fast and mostly is
        // only visible closer to the element so we're ok, I think
        var x = presenter.GetVisualParent();
        while (!(x is ScrollContentPresenter) && x != null)
        {
            x.ClipToBounds = false;
            x = x.GetVisualParent();
        }

        anim.Configuration = new DirectConnectedAnimationConfiguration();
        anim.TryStart(presenter);
    }

    private void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
    {
        if (_animationPage == null)
            return;

        // We're not navigating to a control page, don't set up the animation & clear
        // the previous animation source
        if (!e.SourcePageType.Name.Equals(nameof(FAControlsPageItem)))
        {
            _animationPage = null;
            _animationPage = null;
            return;
        }

        var item = this.GetVisualDescendants()
                    .Where(x => x is ListBoxItem && x.DataContext == _animationPage)
                    .FirstOrDefault()
                    .GetVisualDescendants()
                    .Where(x => x is Viewbox && x.Name == "IconHost")
                    .FirstOrDefault();
        var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
        svc.PrepareToAnimate("ForwardAnimation", item);
    }

    private FAControlsPageItem _animationPage;
}

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Experimental;
using FluentAvalonia.UI.Navigation;

namespace FAControlsGallery.Pages;

public partial class CoreControlsPage : UserControl
{
    public CoreControlsPage()
    {
        InitializeComponent();
        AddHandler(SettingsExpander.ClickEvent, SettingsExpanderClick);

        // Use the frame events here to ensure ConnectedAnimations still work with
        // Back/Forward navigation and not just explicit page invokes
        AddHandler(Frame.NavigatingFromEvent, OnNavigatingFrom, RoutingStrategies.Direct);
        AddHandler(Frame.NavigatedToEvent, OnNavigatedTo, RoutingStrategies.Direct);
    }

    private void OnNavigatedTo(object sender, NavigationEventArgs e)
    {
        if (_connectedAnimationSource == -1)
            return;

        var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
        var anim = svc.GetAnimation("BackAnimation");

        if (anim == null)
            return;

        var presenter = GetAnimationSource();

        // In WinUI, ConnectedAnimation is somehow exempt from all clipping behaviors
        // Here, we are not, so disable ClipToBounds on all elements in the SettingsExpander
        // The rest are taken care of in the xaml.
        // NOTE: The ScrollViewer is not changed here as that's important for scrolling - thus
        // the animation will be cut off, but the back animation is pretty fast and mostly is
        // only visible closer to the element so we're ok, I think
        var x = presenter.GetVisualParent();
        while (!(x is SettingsExpander) && x != null)
        {
            x.ClipToBounds = false;
            x = x.GetVisualParent();
        }

        anim.Configuration = new DirectConnectedAnimationConfiguration();
        anim.TryStart(presenter);
    }

    private void SettingsExpanderClick(object sender, RoutedEventArgs e)
    {
        var presenter = (e.Source as SettingsExpander)
            .GetVisualDescendants().OfType<ContentPresenter>()
            .Where(x => x.Name == "IconPresenter")
            .FirstOrDefault();
        
        Debug.Assert(presenter != null);

        _connectedAnimationSource = ItemsControl.IndexFromContainer(
            (e.Source as SettingsExpander).Parent as ContentPresenter);        
    }

    private void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
    {
        if (_connectedAnimationSource == -1)
            return;

        // We're not navigating to a control page, don't set up the animation & clear
        // the previous animation source
        if (!e.SourcePageType.Name.Equals(nameof(PageBaseViewModel)))
        {
            _connectedAnimationSource = -1;
            return;
        }    

        var presenter = GetAnimationSource();

        var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
        svc.PrepareToAnimate("ForwardAnimation", presenter);
    }

    private Visual GetAnimationSource()
    {
        var container = ItemsControl.ContainerFromIndex(_connectedAnimationSource);
        var presenter = container
            .GetVisualDescendants().OfType<ContentPresenter>()
            .Where(x => x.Name == "IconPresenter")
            .FirstOrDefault();

        return presenter;
    }

    private int _connectedAnimationSource = -1;
}

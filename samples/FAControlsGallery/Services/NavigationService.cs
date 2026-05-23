using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace FAControlsGallery.Services;

public class NavigationService
{
    public static NavigationService Instance { get; } = new NavigationService();

    public Control PreviousPage { get; set; }

    public void SetFrame(FAFrame f)
    {
        _frame = f;
    }

    public void SetOverlayHost(Panel p)
    {
        _overlayHost = p;
    }

    public void Navigate(Type t)
    {
        _frame.Navigate(t);
    }

    public void NavigateFromContext(object dataContext, FANavigationTransitionInfo transitionInfo = null)
    {
        _frame.NavigateFromObject(dataContext,
            new FAFrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new FASuppressNavigationTransitionInfo()
            });
    }

    public void ShowControlDefinitionOverlay(Type targetType)
    {
        if (_overlayHost != null)
        {
            (_overlayHost.Children[0] as ControlDefinitionOverlay).TargetType = targetType;
            (_overlayHost.Children[0] as ControlDefinitionOverlay).Show();
        }
    }

    public void ClearOverlay()
    {
        _overlayHost?.Children.Clear();

    }

    private FAFrame _frame;
    private Panel _overlayHost;
}



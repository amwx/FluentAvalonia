using Avalonia.Animation;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Media.Animation;

/// <summary>
/// Specifies that animations are suppressed during navigation.
/// </summary>
public class SuppressNavigationTransitionInfo : NavigationTransitionInfo
{
    public override void RunAnimation(Animatable ctrl)
    {
        //Do nothing
        (ctrl as IVisual).Opacity = 1;
    }
}


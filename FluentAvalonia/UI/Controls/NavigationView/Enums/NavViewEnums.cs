namespace FluentAvalonia.UI.Controls;

// Ignore NavigationViewShoulderNavigationEnabled - xbox only
// Removed NavigationViewOverflowLabelMode - deprecated

internal enum TopNavigationViewLayoutState
{
    Uninitialized = 0,
    Initialized
}

internal enum NavigationRecommendedTransitionDirection
{
    FromOverflow,
    FromLeft,
    FromRight,
    Default
}

internal enum NavigationViewVisualStateDisplayMode
{
    Compact,
    Expanded,
    Minimal,
    MinimalWithBackButton
}

internal enum NavigationViewRepeaterPosition
{
    LeftNav,
    TopPrimary,
    TopOverflow,
    LeftFooter,
    TopFooter
}

internal enum NavigationViewPropagateTarget
{
    LeftListView,
    TopListView,
    OverflowListView,
    All
}

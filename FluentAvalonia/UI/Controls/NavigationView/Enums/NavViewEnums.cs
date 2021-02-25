namespace FluentAvalonia.UI.Controls
{
    public enum NavigationViewDisplayMode
    {
        Minimal = 0,
        Compact,
        Expanded
    }

    //Replaced with bool
    //public enum NavigationViewBackButtonVisible
    //{
    //    Collapsed = 0,
    //    Visible,
    //    Auto
    //}

    public enum NavigationViewPaneDisplayMode
    {
        Auto = 0,
        Left,
        Top,
        LeftCompact,
        LeftMinimal
    }

    ////Ignore NavigationViewShoulderNavigationEnabled - xbox only

    //Removed...
    public enum NavigationViewOverflowLabelMode
    {
        MoreLabel = 0,
        NoLabel
    }

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
}

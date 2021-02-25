using FluentAvalonia.UI.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Navigation
{
    public class FrameNavigationOptions
    {
        public NavigationTransitionInfo TransitionInfoOverride { get; set; }
        public bool IsNavigationStackEnabled { get; set; }
    }
}

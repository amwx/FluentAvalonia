using FluentAvalonia.UI.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Navigation
{
    public delegate void NavigatingCancelEventHandler(object sender, NavigatingCancelEventArgs e);

    public class NavigatingCancelEventArgs
    {
        internal NavigatingCancelEventArgs(NavigationMode mode, NavigationTransitionInfo info,
            object param, Type srcType)
        {
            NavigationMode = mode;
            NavigationTransitionInfo = info;
            Parameter = param;
            SourcePageType = srcType;
        }

        public bool Cancel { get; set; }
        public NavigationMode NavigationMode { get; }
        public Type SourcePageType { get; }
        public NavigationTransitionInfo NavigationTransitionInfo { get; }
        public object Parameter { get; }
    }
}

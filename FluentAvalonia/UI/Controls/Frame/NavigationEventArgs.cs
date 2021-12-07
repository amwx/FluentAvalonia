using FluentAvalonia.UI.Media.Animation;
using System;

namespace FluentAvalonia.UI.Navigation
{
    public delegate void NavigatedEventHandler(object sender, NavigationEventArgs e);
    public delegate void NavigationStoppedEventHandler(object sender, NavigationEventArgs e);

    public class NavigationEventArgs : EventArgs
    {
        internal NavigationEventArgs(object content, NavigationMode mode,
            NavigationTransitionInfo navInfo, object param,
            Type srcPgType)
        {
            Content = content;
            NavigationMode = mode;
            NavigationTransitionInfo = navInfo;
            Parameter = param;
            SourcePageType = srcPgType;
        }

        //public Uri Uri { get; set; }
        public object Content { get; }
        public NavigationMode NavigationMode { get; }
        public object Parameter { get; }
        public Type SourcePageType { get; }
        public NavigationTransitionInfo NavigationTransitionInfo { get; }
    }
}

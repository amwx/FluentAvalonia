using Avalonia.Controls;
using FluentAvalonia.UI.Media.Animation;
using System;

namespace FluentAvalonia.UI.Navigation
{
    public class PageStackEntry
    {
        public PageStackEntry(Type sourcePageType, object parameter, NavigationTransitionInfo navigationTransitionInfo)
        {
            NavigationTransitionInfo = navigationTransitionInfo;
            SourcePageType = sourcePageType;
            Parameter = parameter; 
        }

        public Type SourcePageType { get; set; }
        public NavigationTransitionInfo NavigationTransitionInfo { get; internal set; }
        public object Parameter { get; set; }
        internal IControl Instance { get; set; }
    }
}

using System;

namespace FluentAvalonia.UI.Navigation
{
    public delegate void NavigationFailedEventHandler(object sender, NavigationFailedEventArgs e);

    public class NavigationFailedEventArgs : EventArgs
    {
        internal NavigationFailedEventArgs(Exception ex, Type srcPageType)
        {
            Exception = ex;
            SourcePageType = srcPageType;
        }

        public bool Handled { get; set; }
        public Exception Exception { get; }
        public Type SourcePageType { get; }
    }
}

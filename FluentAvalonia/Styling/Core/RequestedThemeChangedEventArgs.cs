using System;

namespace FluentAvalonia.Styling
{
    public class RequestedThemeChangedEventArgs : EventArgs
    {
        internal RequestedThemeChangedEventArgs(string theme)
        {
            NewTheme = theme;
        }
        public string NewTheme { get; }
    }
}

using System;

namespace FluentAvalonia.UI.Controls
{
    public class ContentDialogClosedEventArgs : EventArgs
    {
        public ContentDialogResult Result { get; }

        public ContentDialogClosedEventArgs(ContentDialogResult res)
        {
            Result = res;
        }
    }
}

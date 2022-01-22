using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides data for the Closed event.
    /// </summary>
    public class ContentDialogClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="ContentDialogResult"/> of the button click event.
        /// </summary>
        public ContentDialogResult Result { get; }

        internal ContentDialogClosedEventArgs(ContentDialogResult res)
        {
            Result = res;
        }
    }
}

using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides data for the closing event.
    /// </summary>
    public class ContentDialogClosingEventArgs : EventArgs
    {
        internal ContentDialogClosingEventArgs(ContentDialog owner, ContentDialogResult res)
        {
            Result = res;
            _owner = owner;
        }

        /// <summary>
        /// Gets or sets a value that can cancel the closing of the dialog.
        /// A true value for Cancel cancels the default behavior.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the <see cref="ContentDialogResult"/> of the closing event.
        /// </summary>
        public ContentDialogResult Result { get; }

        internal bool IsDeferred => _deferral != null;

        /// <summary>
        /// Gets a <see cref="ContentDialogClosingDeferral"/> that the app can use to 
        /// respond asynchronously to the closing event.
        /// </summary>
        /// <returns></returns>
        public ContentDialogClosingDeferral GetDeferral()
        {
            _deferral = new ContentDialogClosingDeferral(_owner);
            return _deferral;
        }

        private ContentDialog _owner;
        private ContentDialogClosingDeferral _deferral;
    }
}

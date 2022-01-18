using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides data for the button click events.
    /// </summary>
    public class ContentDialogButtonClickEventArgs : EventArgs
    {
        internal ContentDialogButtonClickEventArgs(ContentDialog owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Gets or sets a value that can cancel the button click. 
        /// A true value for Cancel cancels the default behavior.
        /// </summary>
        public bool Cancel { get; set; }

        internal bool IsDeferred => _deferral != null;

        /// <summary>
        /// Gets a <see cref="ContentDialogButtonClickDeferral"/> the app can use to respond
        /// asyncronously to a button click event
        /// </summary>
        public ContentDialogButtonClickDeferral GetDeferral()
        {
            _deferral = new ContentDialogButtonClickDeferral(_owner);
            return _deferral;
        }

        private ContentDialog _owner;
        private ContentDialogButtonClickDeferral _deferral;
    }
}

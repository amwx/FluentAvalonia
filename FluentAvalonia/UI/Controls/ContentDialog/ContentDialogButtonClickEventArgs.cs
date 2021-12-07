using System;

namespace FluentAvalonia.UI.Controls
{
    public class ContentDialogButtonClickEventArgs : EventArgs
    {
        internal ContentDialogButtonClickEventArgs(ContentDialog owner)
        {
            _owner = owner;
        }

        public bool Cancel { get; set; }

        internal bool IsDeferred => _deferral != null;

        /// <summary>
        /// Gets a ContentDialogButtonClickDeferral the app can use to respond
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

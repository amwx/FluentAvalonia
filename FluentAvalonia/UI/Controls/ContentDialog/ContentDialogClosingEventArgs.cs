using System;

namespace FluentAvalonia.UI.Controls
{
    public class ContentDialogClosingEventArgs : EventArgs
    {
        internal ContentDialogClosingEventArgs(ContentDialog owner, ContentDialogResult res)
        {
            Result = res;
            _owner = owner;
        }

        public bool Cancel { get; set; }

        public ContentDialogResult Result { get; }

        internal bool IsDeferred => _deferral != null;

        public ContentDialogClosingDeferral GetDeferral()
        {
            _deferral = new ContentDialogClosingDeferral(_owner);
            return _deferral;
        }

        private ContentDialog _owner;
        private ContentDialogClosingDeferral _deferral;
    }
}

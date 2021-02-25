//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml


namespace FluentAvalonia.UI.Controls
{
    public class ContentDialogButtonClickEventArgs
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

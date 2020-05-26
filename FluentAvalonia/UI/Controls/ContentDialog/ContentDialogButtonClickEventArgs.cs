//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml


namespace FluentAvalonia.UI.Controls
{
    public class ContentDialogButtonClickEventArgs
    {
        public bool Cancel { get; set; }
        /// <summary>
        /// Gets a ContentDialogButtonClickDeferral the app can use to respond
        /// asyncronously to a button click event
        /// </summary>
        public ContentDialogButtonClickDeferral GetDeferral()
        {
            return Owner.GetButtonClickDeferral();
        }

        public ContentDialogButtonClickEventArgs(bool cancel, ContentDialog owner)
        {
            Cancel = cancel;
            Owner = owner;
        }

        private ContentDialog Owner;
    }
}

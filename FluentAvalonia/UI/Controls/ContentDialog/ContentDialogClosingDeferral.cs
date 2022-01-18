namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a deferral that can be used by an app to respond asyncronously to
    /// a <see cref="ContentDialog"/> closing
    /// </summary>
    public class ContentDialogClosingDeferral
    {
        internal ContentDialogClosingDeferral(ContentDialog owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Notifies the system that the app has finished processing the closing event.
        /// </summary>
        public void Complete()
        {
            _owner.CompleteClosingDeferral();
        }

        private ContentDialog _owner;
    }
}

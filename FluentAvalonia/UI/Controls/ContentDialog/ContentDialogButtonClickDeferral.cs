namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a deferral that can be used by an app to respond asyncronously to
    /// a button click event
    /// </summary>
    public sealed class ContentDialogButtonClickDeferral
    {
        internal ContentDialogButtonClickDeferral(ContentDialog owner)
        {
            Owner = owner;
        }

        internal bool IsDeferred { get; set; }

        /// <summary>
        /// Notifies the system that the app has finished processing the button click event.
        /// </summary>
        public void Complete() 
        {
            Owner.CompleteButtonClickDeferral();
        }

        private ContentDialog Owner;
    }
}

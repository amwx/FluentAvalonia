namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Specifies identifiers to indicate the return value of a ContentDialog
    /// </summary>
    public enum ContentDialogResult
    {
        /// <summary>
        /// No button was tapped.
        /// </summary>
        None = 0,

        /// <summary>
        /// The primary button was tapped by the user.
        /// </summary>
        Primary = 1,

        /// <summary>
        /// The secondary button was tapped by the user.
        /// </summary>
        Secondary = 2
    }
}

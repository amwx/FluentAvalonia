namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Defines constants that specify where in the XAML visual tree a ContentDialog is rooted.
    /// </summary>
    public enum ContentDialogPlacement
    {
        /// <summary>
        /// Place in the XamlRoot of the Window above all content. A "light dismiss" layer 
        /// appears below.
        /// </summary>
        Popup = 0,

        /// <summary>
        /// Rooted in a parent container - currently not supported
        /// </summary>
        InPlace = 1
    }
}

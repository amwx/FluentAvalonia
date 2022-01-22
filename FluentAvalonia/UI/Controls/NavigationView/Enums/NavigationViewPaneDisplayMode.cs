namespace FluentAvalonia.UI.Controls
{
	//Replaced with bool
	//public enum NavigationViewBackButtonVisible
	//{
	//    Collapsed = 0,
	//    Visible,
	//    Auto
	//}

	/// <summary>
	/// Defines constants that specify how and where the NavigationView pane is shown.
	/// </summary>
	public enum NavigationViewPaneDisplayMode
    {
        /// <summary>
        /// The pane is shown on the left side of the control, and changes between minimal, 
        /// compact, and full states depending on the width of the window.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// The pane is shown on the left side of the control in its fully open state.
        /// </summary>
        Left,

        /// <summary>
        /// The pane is shown at the top of the control.
        /// </summary>
        Top,

        /// <summary>
        /// The pane is shown on the left side of the control. Only the pane icons are shown by default.
        /// </summary>
        LeftCompact,

        /// <summary>
        /// The pane is shown on the left side of the control. Only the pane menu button is shown by default.
        /// </summary>
        LeftMinimal
    }
}

using Avalonia;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides calculated values that can be referenced as TemplatedParent 
    /// sources when defining templates for a NavigationView. Not intended for general use.
    /// </summary>
    public class NavigationViewTemplateSettings : AvaloniaObject
    {
        internal NavigationViewTemplateSettings() { }

        /// <summary>
        /// Defines the <see cref="BackButtonVisibility"/> property
        /// </summary>
        public static readonly StyledProperty<bool> BackButtonVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(BackButtonVisibility), false);

        /// <summary>
        /// Defines the <see cref="LeftPaneVisibility"/> property
        /// </summary>
        public static readonly StyledProperty<bool> LeftPaneVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(LeftPaneVisibility), true);

        /// <summary>
        /// Defines the <see cref="OverflowButtonVisibility"/> property
        /// </summary>
        public static readonly StyledProperty<bool> OverflowButtonVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(OverflowButtonVisibility), false);

        /// <summary>
        /// Defines the <see cref="PaneToggleButtonVisibility"/> property
        /// </summary>
        public static readonly StyledProperty<bool> PaneToggleButtonVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(PaneToggleButtonVisibility), true);

        /// <summary>
        /// Defines the <see cref="PaneToggleButtonWidth"/> property
        /// </summary>
		public static readonly StyledProperty<double> PaneToggleButtonWidthProperty =
			AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(PaneToggleButtonWidth));

        /// <summary>
        /// Defines the <see cref="SingleSelectionFollowsFocus"/> property
        /// </summary>
        public static readonly StyledProperty<bool> SingleSelectionFollowsFocusProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(SingleSelectionFollowsFocus), false);

        /// <summary>
        /// Defines the <see cref="SmallerPaneToggleButtonWidth"/> property
        /// </summary>
		public static readonly StyledProperty<double> SmallerPaneToggleButtonWidthProperty =
			AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(SmallerPaneToggleButtonWidth));

        /// <summary>
        /// Defines the <see cref="TopPadding"/> property
        /// </summary>
		public static readonly StyledProperty<double> TopPaddingProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(TopPadding), 0d);

        /// <summary>
        /// Defines the <see cref="TopPaneVisibility"/> property
        /// </summary>
        public static readonly StyledProperty<bool> TopPaneVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(TopPaneVisibility), false);

        /// <summary>
        /// Defines the <see cref="OpenPaneWidth"/> property
        /// </summary>
        public static readonly StyledProperty<double> OpenPaneWidthProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(OpenPaneWidth), 320d);

        /// <summary>
        /// Gets the visibility of the back button.
        /// </summary>
        public bool BackButtonVisibility
        {
            get => GetValue(BackButtonVisibilityProperty);
            internal set => SetValue(BackButtonVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the visibility of the left pane.
        /// </summary>
        public bool LeftPaneVisibility
        {
            get => GetValue(LeftPaneVisibilityProperty);
            internal set => SetValue(LeftPaneVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the visibility of the overflow button.
        /// </summary>
        public bool OverflowButtonVisibility
        {
            get => GetValue(OverflowButtonVisibilityProperty);
            internal set => SetValue(OverflowButtonVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the visibility of the pane toggle button.
        /// </summary>
        public bool PaneToggleButtonVisibility
        {
            get => GetValue(PaneToggleButtonVisibilityProperty);
            internal set => SetValue(PaneToggleButtonVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the width of the pane toggle button
        /// </summary>
		public double PaneToggleButtonWidth
		{
			get => GetValue(PaneToggleButtonWidthProperty);
			internal set => SetValue(PaneToggleButtonWidthProperty, value);
		}

        /// <summary>
        /// Gets the SelectionFollowsFocus value.
        /// </summary>
		public bool SingleSelectionFollowsFocus
        {
            get => GetValue(SingleSelectionFollowsFocusProperty);
            internal set => SetValue(SingleSelectionFollowsFocusProperty, value);
        }

        /// <summary>
        /// TODO: Relatively new property - need docs from MS
        /// </summary>
		public double SmallerPaneToggleButtonWidth
		{
			get => GetValue(SmallerPaneToggleButtonWidthProperty);
			internal set => SetValue(SmallerPaneToggleButtonWidthProperty, value);
		}

        /// <summary>
        /// Gets the padding value of the top pane.
        /// </summary>
		public double TopPadding
        {
            get => GetValue(TopPaddingProperty);
            internal set => SetValue(TopPaddingProperty, value);
        }

        /// <summary>
        /// Gets the visibility of the top pane.
        /// </summary>
        public bool TopPaneVisibility
        {
            get => GetValue(TopPaneVisibilityProperty);
            internal set => SetValue(TopPaneVisibilityProperty, value);
        }

        /// <summary>
        /// TODO: Relatively new property - need docs from MS
        /// </summary>
        public double OpenPaneWidth // WinUI #5800
        {
            get => GetValue(OpenPaneWidthProperty);
            internal set => SetValue(OpenPaneWidthProperty, value);
        }
    }
}

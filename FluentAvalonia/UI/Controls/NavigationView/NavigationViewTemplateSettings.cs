using Avalonia;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewTemplateSettings : AvaloniaObject
    {
        internal NavigationViewTemplateSettings() { }

        public static readonly StyledProperty<bool> BackButtonVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(BackButtonVisibility), false);

        //public static readonly StyledProperty<double> LatestPaneButtonWidthProperty =
        //    AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(LatestPaneButtonWidth), 0d);

        public static readonly StyledProperty<bool> LeftPaneVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(LeftPaneVisibility), true);

        public static readonly StyledProperty<bool> OverflowButtonVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(OverflowButtonVisibility), false);

        //public static readonly StyledProperty<double> PaneButtonWidthProperty =
        //    AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(PaneButtonWidth), 0d);

        public static readonly StyledProperty<bool> PaneToggleButtonVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(PaneToggleButtonVisibility), true);

		public static readonly StyledProperty<double> PaneToggleButtonWidthProperty =
			AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(PaneToggleButtonWidth));

        public static readonly StyledProperty<bool> SingleSelectionFollowsFocusProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(SingleSelectionFollowsFocus), false);

		public static readonly StyledProperty<double> SmallerPaneToggleButtonWidthProperty =
			AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(SmallerPaneToggleButtonWidth));

		public static readonly StyledProperty<double> TopPaddingProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(TopPadding), 0d);

        public static readonly StyledProperty<bool> TopPaneVisibilityProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, bool>(nameof(TopPaneVisibility), false);

        public static readonly StyledProperty<double> OpenPaneWidthProperty =
            AvaloniaProperty.Register<NavigationViewTemplateSettings, double>(nameof(OpenPaneWidth), 320d);


        public bool BackButtonVisibility
        {
            get => GetValue(BackButtonVisibilityProperty);
            internal set => SetValue(BackButtonVisibilityProperty, value);
        }

        //public double LatestPaneButtonWidth
        //{
        //    get => GetValue(LatestPaneButtonWidthProperty);
        //    internal set => SetValue(LatestPaneButtonWidthProperty, value);
        //} 

        public bool LeftPaneVisibility
        {
            get => GetValue(LeftPaneVisibilityProperty);
            internal set => SetValue(LeftPaneVisibilityProperty, value);
        }

        public bool OverflowButtonVisibility
        {
            get => GetValue(OverflowButtonVisibilityProperty);
            internal set => SetValue(OverflowButtonVisibilityProperty, value);
        }

        //public double PaneButtonWidth
        //{
        //    get => GetValue(PaneButtonWidthProperty);
        //    internal set => SetValue(PaneButtonWidthProperty, value);
        //}

        public bool PaneToggleButtonVisibility
        {
            get => GetValue(PaneToggleButtonVisibilityProperty);
            internal set => SetValue(PaneToggleButtonVisibilityProperty, value);
        }

		public double PaneToggleButtonWidth
		{
			get => GetValue(PaneToggleButtonWidthProperty);
			internal set => SetValue(PaneToggleButtonWidthProperty, value);
		}

		public bool SingleSelectionFollowsFocus
        {
            get => GetValue(SingleSelectionFollowsFocusProperty);
            internal set => SetValue(SingleSelectionFollowsFocusProperty, value);
        }

		public double SmallerPaneToggleButtonWidth
		{
			get => GetValue(SmallerPaneToggleButtonWidthProperty);
			internal set => SetValue(SmallerPaneToggleButtonWidthProperty, value);
		}

		public double TopPadding
        {
            get => GetValue(TopPaddingProperty);
            internal set => SetValue(TopPaddingProperty, value);
        }

        public bool TopPaneVisibility
        {
            get => GetValue(TopPaneVisibilityProperty);
            internal set => SetValue(TopPaneVisibilityProperty, value);
        }

        public double OpenPaneWidth // WinUI #5800
        {
            get => GetValue(OpenPaneWidthProperty);
            internal set => SetValue(OpenPaneWidthProperty, value);
        }
    }
}

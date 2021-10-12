using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using FluentAvalonia.Core;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;

namespace FluentAvalonia.UI.Controls
{
    public partial class NavigationView : HeaderedContentControl
    {
        public static readonly StyledProperty<bool> AlwaysShowHeaderProperty =
            AvaloniaProperty.Register<NavigationView, bool>(nameof(AlwaysShowHeader), true);

        public static readonly StyledProperty<AutoCompleteBox> AutoCompleteBoxProperty =
            AvaloniaProperty.Register<NavigationView, AutoCompleteBox>(nameof(AutoCompleteBox));

        public static readonly StyledProperty<double> CompactModeThresholdWidthProperty =
            AvaloniaProperty.Register<NavigationView, double>(nameof(CompactModeThresholdWidth),
                641.0, coerce: CoercePropertyValueToGreaterThanZero);

        public static readonly StyledProperty<double> CompactPaneLengthProperty =
            AvaloniaProperty.Register<NavigationView, double>(nameof(CompactPaneLength),
                48.0, coerce: CoercePropertyValueToGreaterThanZero);

        public static readonly StyledProperty<IControl> ContentOverlayProperty =
            AvaloniaProperty.Register<NavigationView, IControl>(nameof(ContentOverlay));

        public static readonly DirectProperty<NavigationView, NavigationViewDisplayMode> DisplayModeProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, NavigationViewDisplayMode>(nameof(DisplayMode),
                x => x.DisplayMode);

        public static readonly StyledProperty<double> ExpandedModeThresholdWidthProperty =
            AvaloniaProperty.Register<NavigationView, double>(nameof(ExpandedModeThresholdWidth), 1008.0,
                coerce: CoercePropertyValueToGreaterThanZero);

        public static readonly DirectProperty<NavigationView, IEnumerable> FooterMenuItemsProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, IEnumerable>(nameof(FooterMenuItems),
                x => x.FooterMenuItems, (x, v) => x.FooterMenuItems = v);

        //In WinUI, this is enum NavigationViewBackButtonVisible
        //Visible
        //Collapsed
        //Auto - depends on form factor, for now, not concern
        //So fall back to bool
        public static readonly DirectProperty<NavigationView, bool> IsBackButtonVisibleProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(IsBackButtonVisible),
                x => x.IsBackButtonVisible, (x,v) => x.IsBackButtonVisible = v);

        public static readonly StyledProperty<bool> IsBackEnabledProperty =
            AvaloniaProperty.Register<NavigationView, bool>(nameof(IsBackEnabled), false);

        public static readonly DirectProperty<NavigationView, bool> IsPaneOpenProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(IsPaneOpen), 
                x => x.IsPaneOpen, (x,v) => x.IsPaneOpen = v);

        public static readonly StyledProperty<bool> IsPaneToggleButtonVisibleProperty =
            AvaloniaProperty.Register<NavigationView, bool>(nameof(IsPaneToggleButtonVisible), true);

        public static readonly StyledProperty<bool> IsPaneVisibleProperty =
            AvaloniaProperty.Register<NavigationView, bool>(nameof(IsPaneVisible), true);

        public static readonly StyledProperty<bool> IsSettingsVisibleProperty =
            AvaloniaProperty.Register<NavigationView, bool>(nameof(IsSettingsVisible), true);

        //SKIP for now, IsTitleBarAutoPaddingEnabled...

        public static readonly DirectProperty<NavigationView, IEnumerable> MenuItemsProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, IEnumerable>(nameof(MenuItems),
                o => o.MenuItems, (o, v) => o.MenuItems = v);

        public static readonly StyledProperty<IDataTemplate> MenuItemTemplateProperty =
            AvaloniaProperty.Register<NavigationView, IDataTemplate>(nameof(MenuItemTemplate));

		public static readonly StyledProperty<DataTemplateSelector> MenuItemTemplateSelectorProperty =
			AvaloniaProperty.Register<NavigationView, DataTemplateSelector>(nameof(MenuItemTemplateSelector));

        public static readonly StyledProperty<double> OpenPaneLengthProperty =
            AvaloniaProperty.Register<NavigationView, double>(nameof(OpenPaneLength),
                320.0, coerce: CoercePropertyValueToGreaterThanZero);

		//OverflowLabelModeProperty removed, as it was deprecated

		public static readonly StyledProperty<IControl> PaneCustomContentProperty =
            AvaloniaProperty.Register<NavigationView, IControl>(nameof(PaneCustomContent));

        public static readonly StyledProperty<NavigationViewPaneDisplayMode> PaneDisplayModeProperty =
            AvaloniaProperty.Register<NavigationView, NavigationViewPaneDisplayMode>(nameof(PaneDisplayMode),
                NavigationViewPaneDisplayMode.Auto);

        public static readonly StyledProperty<IControl> PaneFooterProperty =
            AvaloniaProperty.Register<NavigationView, IControl>(nameof(PaneFooter));

        public static readonly StyledProperty<IControl> PaneHeaderProperty =
            AvaloniaProperty.Register<NavigationView, IControl>(nameof(PaneHeader));

        public static readonly StyledProperty<string> PaneTitleProperty =
            AvaloniaProperty.Register<NavigationView, string>(nameof(PaneTitle));

        public static readonly DirectProperty<NavigationView, object> SelectedItemProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, object>(nameof(SelectedItem),
                x => x.SelectedItem, (x,v) => x.SelectedItem = v, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        //WinUI uses an enum here, but only has Disabled/Enabled, so just use bool
        public static readonly DirectProperty<NavigationView, bool> SelectionFollowsFocusProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(SelectionFollowsFocus),
                x => x.SelectionFollowsFocus, (x,v) => x.SelectionFollowsFocus = v);

        public static readonly DirectProperty<NavigationView, NavigationViewItem> SettingsItemProperty =
            AvaloniaProperty.RegisterDirect<NavigationView, NavigationViewItem>(nameof(SettingsItem),
                x => x.SettingsItem, (x,v) => x.SettingsItem = v);

        //Ignore Shoulder Navigation (xbox)

        public static readonly StyledProperty<NavigationViewTemplateSettings> TemplateSettingsProperty =
            AvaloniaProperty.Register<NavigationView, NavigationViewTemplateSettings>(nameof(TemplateSettings));

        public bool AlwaysShowHeader
        {
            get => GetValue(AlwaysShowHeaderProperty);
            set => SetValue(AlwaysShowHeaderProperty, value);
        }

        public AutoCompleteBox AutoCompleteBox
        {
            get => GetValue(AutoCompleteBoxProperty);
            set => SetValue(AutoCompleteBoxProperty, value);
        }

        public double CompactModeThresholdWidth
        {
            get => GetValue(CompactModeThresholdWidthProperty);
            set => SetValue(CompactModeThresholdWidthProperty, value);
        }

        public double CompactPaneLength
        {
            get => GetValue(CompactPaneLengthProperty);
            set => SetValue(CompactPaneLengthProperty, value);
        }

        public IControl ContentOverlay
        {
            get => GetValue(ContentOverlayProperty);
            set => SetValue(ContentOverlayProperty, value);
        }

        public NavigationViewDisplayMode DisplayMode
        {
            get => _displayMode;
            private set => SetAndRaise(DisplayModeProperty, ref _displayMode, value);
        }

        public double ExpandedModeThresholdWidth
        {
            get => GetValue(ExpandedModeThresholdWidthProperty);
            set => SetValue(ExpandedModeThresholdWidthProperty, value);
        }

        public IEnumerable FooterMenuItems
        {
            get => _footerMenuItems;
            set 
            {
                var old = _footerMenuItems;
                if (SetAndRaise(FooterMenuItemsProperty, ref _footerMenuItems, value))
                {
                    if (old is INotifyCollectionChanged oldINCC)
                    {
                        oldINCC.CollectionChanged -= OnFooterItemsSourceCollectionChanged;
                    }
                    if (value is INotifyCollectionChanged newINCC)
                    {
                        newINCC.CollectionChanged += OnFooterItemsSourceCollectionChanged;
                    }

                    UpdateFooterRepeaterItemsSource(true, true);
                }
            }
        }

        public bool IsBackButtonVisible
        {
            get => _isBackVisible;
            set
            {
                if (SetAndRaise(IsBackButtonVisibleProperty, ref _isBackVisible, value))
                {
                    UpdateBackAndCloseButtonsVisibility();
                    UpdateAdaptiveLayout(Bounds.Width);
                    if (IsTopNavigationView)
                    {
                        InvalidateTopNavPrimaryLayout();
                    }

					// Enabling back button shifts grid instead of resizing, so let's update the layout.
					if (_backButton != null)
					{
						//Don't have update layout, so
						_backButton.InvalidateMeasure();
					}
                    UpdatePaneLayout();
                }
            }
        }

        public bool IsBackEnabled
        {
            get => GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set 
            {
                if (SetAndRaise(IsPaneOpenProperty, ref _isPaneOpen, value))
                {
                    OnIsPaneOpenChanged();
                    UpdateVisualStateForDisplayModeGroup(_displayMode);
                }
            }
        }

        public bool IsPaneToggleButtonVisible
        {
            get => GetValue(IsPaneToggleButtonVisibleProperty);
            set => SetValue(IsPaneToggleButtonVisibleProperty, value);
        }

        public bool IsPaneVisible
        {
            get => GetValue(IsPaneVisibleProperty);
            set => SetValue(IsPaneVisibleProperty, value);
        }

        public bool IsSettingsVisible
        {
            get => GetValue(IsSettingsVisibleProperty);
            set => SetValue(IsSettingsVisibleProperty, value);
        }

        public IDataTemplate MenuItemTemplate
        {
            get => GetValue(MenuItemTemplateProperty);
            set => SetValue(MenuItemTemplateProperty, value);
        }

		public DataTemplateSelector MenuItemTemplateSelector
		{
			get => GetValue(MenuItemTemplateSelectorProperty);
			set => SetValue(MenuItemTemplateSelectorProperty, value);
		}

        public IEnumerable MenuItems
        {
            get => _menuItems;
            set
            {
                var old = _menuItems;
                if (SetAndRaise(MenuItemsProperty, ref _menuItems, value))
                {
                    if (_menuItems is INotifyCollectionChanged oldINCC)
                    {
                        oldINCC.CollectionChanged -= OnMenuItemsSourceCollectionChanged;
                    }
                    if (value is INotifyCollectionChanged newINCC)
                    {
                        newINCC.CollectionChanged += OnMenuItemsSourceCollectionChanged;
                    }
                    UpdateRepeaterItemsSource(true);
                }
            }
        }

        public double OpenPaneLength
        {
            get => GetValue(OpenPaneLengthProperty);
            set => SetValue(OpenPaneLengthProperty, value);
        }

        //OverflowLabelMode removed, deprecated in WinUI

        public IControl PaneCustomContent
        {
            get => GetValue(PaneCustomContentProperty);
            set => SetValue(PaneCustomContentProperty, value);
        }

        public NavigationViewPaneDisplayMode PaneDisplayMode
        {
            get => GetValue(PaneDisplayModeProperty);
            set => SetValue(PaneDisplayModeProperty, value);
        }

        public IControl PaneFooter
        {
            get => GetValue(PaneFooterProperty);
            set => SetValue(PaneFooterProperty, value);
        }

        public IControl PaneHeader
        {
            get => GetValue(PaneHeaderProperty);
            set => SetValue(PaneHeaderProperty, value);
        }

        public string PaneTitle
        {
            get => GetValue(PaneTitleProperty);
            set => SetValue(PaneTitleProperty, value);
        }

        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
				SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
            }
        }

        //WinUI uses an enum here, but only has Disabled/Enabled, so just use bool
        public bool SelectionFollowsFocus
        {
            get => _selectionFollowsFocus;
            set => SetAndRaise(SelectionFollowsFocusProperty, ref _selectionFollowsFocus, value);
        }

        public NavigationViewItem SettingsItem
        {
            get => _settingsItem;
            set => SetAndRaise(SettingsItemProperty, ref _settingsItem, value);
        }

        public NavigationViewTemplateSettings TemplateSettings
        {
            get => GetValue(TemplateSettingsProperty);
            protected set => SetValue(TemplateSettingsProperty, value);
        }

        /// <summary>
        /// Coerces a double to ensure its valid for use, i.e. >= 0 and not NaN or infinity
        /// Used for: CompactModeThresholdWidthProperty, CompactPaneLengthProperty, 
        /// ExpandedModeThresholdWidthProperty, and OpenPaneLengthProperty
        /// </summary>
        /// <returns></returns>
        private static double CoercePropertyValueToGreaterThanZero(IAvaloniaObject arg1, double arg2)
        {
            if (double.IsNaN(arg2) || double.IsInfinity(arg2))
                return 0;
            return Math.Max(arg2, 0.0);
        }

        public event TypedEventHandler<NavigationView, NavigationViewPaneClosingEventArgs> PaneClosing;
        public event TypedEventHandler<NavigationView, object> PaneClosed;
        public event TypedEventHandler<NavigationView, object> PaneOpening;
        public event TypedEventHandler<NavigationView, object> PaneOpened;

        public event EventHandler<NavigationViewBackRequestedEventArgs> BackRequested;
        public event EventHandler<NavigationViewSelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<NavigationViewItemInvokedEventArgs> ItemInvoked;
        public event EventHandler<NavigationViewDisplayModeChangedEventArgs> DisplayModeChanged;

        public event EventHandler<NavigationViewItemExpandingEventArgs> ItemExpanding;
        public event EventHandler<NavigationViewItemCollapsedEventArgs> ItemCollapsed;


        public static readonly AttachedProperty<CompositeDisposable> NavigationViewItemRevokersProperty =
            AvaloniaProperty.RegisterAttached<NavigationView, NavigationViewItem, CompositeDisposable>("NavigationViewItemRevokers");

        private bool _isPaneOpen = true;
        private object _selectedItem;
        private bool _isBackVisible;
        private bool _selectionFollowsFocus;
        private IEnumerable _menuItems;
        private IEnumerable _footerMenuItems;
        private NavigationViewDisplayMode _displayMode = NavigationViewDisplayMode.Minimal;
        private NavigationViewItem _settingsItem;
    }
}

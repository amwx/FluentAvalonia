using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls.Primitives
{
    [PseudoClasses(":expanded")]
    [PseudoClasses(":closedcompacttop", ":notclosedcompacttop")]
    [PseudoClasses(":leftnav", ":topnav", ":topoverflow")]
    [PseudoClasses(":chevronopen", ":chevronclosed", ":chevronhidden")]
    [PseudoClasses(":iconleft", ":icononly", ":contentonly")]
    public class NavigationViewItemPresenter : ContentControl
    {
        public NavigationViewItemPresenter()
        {
            TemplateSettings = new NavigationViewItemPresenterTemplateSettings();
        }

        public static readonly StyledProperty<IconElement> IconProperty =
            AvaloniaProperty.Register<NavigationViewItemPresenter, IconElement>(nameof(Icon));

        public static readonly StyledProperty<NavigationViewItemPresenterTemplateSettings> TemplateSettingsProperty =
            AvaloniaProperty.Register<NavigationViewItemPresenter, NavigationViewItemPresenterTemplateSettings>(nameof(TemplateSettings));

        public static readonly StyledProperty<InfoBadge> InfoBadgeProperty =
            NavigationViewItem.InfoBadgeProperty.AddOwner<NavigationViewItemPresenter>();

        public IconElement Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public NavigationViewItemPresenterTemplateSettings TemplateSettings
        {
            get => GetValue(TemplateSettingsProperty);
            set => SetValue(TemplateSettingsProperty, value);
        }

        public InfoBadge InfoBadge
        {
            get => GetValue(InfoBadgeProperty);
            set => SetValue(InfoBadgeProperty, value);
        }

        public NavigationViewItem GetNVI
        {
            get
            {
                return this.FindAncestorOfType<NavigationViewItem>();
            }
        }

        internal IControl SelectionIndicator => _selectionIndicator;

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            //TODO check types
            _selectionIndicator = e.NameScope.Get<Border>("SelectionIndicator");

            //This doesn't exist in the TopPane template, so use Find and allow it to be null
            _contentGrid = e.NameScope.Find<Panel>("PresenterContentRootGrid");

            _infoBadgePresenter = e.NameScope.Find<ContentPresenter>("InfoBadgePresenter");

            var nvi = GetNVI;
            if (nvi != null)
            {
                _expandCollapseChevron = e.NameScope.Get<Panel>("ExpandCollapseChevron");

                if (_expandCollapseChevron != null)
                {
                    _expandCollapseChevron.Tapped += nvi.OnExpandCollapseChevronTapped;
                }
                nvi.UpdateVisualState();

                // We probably switched displaymode, so restore width now, otherwise the next time we will restore is when the CompactPaneLength changes
                var navView = nvi.GetNavigationView;
                if (navView != null)
                {
                    if (navView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top)
                    {
                        UpdateCompactPaneLength(_compactPaneLengthValue, true);
                    }
                }
            }

            UpdateMargin();
        }

		protected override void OnPointerPressed(PointerPressedEventArgs e)
		{
			base.OnPointerPressed(e);
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				PseudoClasses.Set(":pressed", true);
			}
		}

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
			base.OnPointerReleased(e);
			if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased 
				&& e.InitialPressMouseButton == MouseButton.Left)
			{
				PseudoClasses.Set(":pressed", false);
			}
		}

		protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
		{
			base.OnPointerCaptureLost(e);
			PseudoClasses.Set(":pressed", false);
		}

		internal void RotateExpandCollapseChevron(bool isExpanded)
        {
            PseudoClasses.Set(":expanded", isExpanded);
        }

        internal void UpdateContentLeftIndentation(double leftIndent)
        {
            _leftIndentation = leftIndent;
            UpdateMargin();
        }

        private void UpdateMargin()
        {
            if (_contentGrid != null)
            {
                var oldMargin = _contentGrid.Margin;
                _contentGrid.Margin = new Thickness(_leftIndentation, oldMargin.Top, oldMargin.Right, oldMargin.Bottom);
            }
        }

        internal void UpdateCompactPaneLength(double len, bool update)
        {
            _compactPaneLengthValue = len;

            if (update)
            {
                TemplateSettings.IconWidth = len;
                TemplateSettings.SmallerIconWidth = len - 8;
            }
        }

        internal void UpdateClosedCompactVisualState(bool topLevel, bool isClosedCompact)
        {
            // We increased the ContentPresenter margin to align it visually with the expand/collapse chevron. This updated margin is even applied when the
            // NavigationView is in a visual state where no expand/collapse chevrons are shown, leading to more content being cut off than necessary.
            // This is the case for top-level items when the NavigationView is in a compact mode and the NavigationView pane is closed. To keep the original
            // cutoff visual experience intact, we restore  the original ContentPresenter margin for such top-level items only (children shown in a flyout
            // will use the updated margin).

            //states :closedcompacttop, :notclosedcompacttop

            PseudoClasses.Set(":closedcompacttop", isClosedCompact && topLevel);
            PseudoClasses.Set(":notclosedcompacttop", !isClosedCompact && topLevel);
        }


        private Panel _contentGrid;
        private Panel _expandCollapseChevron;
        private IControl _selectionIndicator;
        private ContentPresenter _infoBadgePresenter;
        private double _compactPaneLengthValue = 40;
        private double _leftIndentation;
    }

    public class NavigationViewItemPresenterTemplateSettings : AvaloniaObject
    {
        internal NavigationViewItemPresenterTemplateSettings() { }

        public static readonly StyledProperty<double> IconWidthProperty =
            AvaloniaProperty.Register<NavigationViewItemPresenterTemplateSettings, double>(nameof(IconWidth));

        public static readonly StyledProperty<double> SmallerIconWidthProperty =
            AvaloniaProperty.Register<NavigationViewItemPresenterTemplateSettings, double>(nameof(SmallerIconWidth));

        public double IconWidth
        {
            get => GetValue(IconWidthProperty);
            internal set => SetValue(IconWidthProperty, value);
        }

        public double SmallerIconWidth
        {
            get => GetValue(SmallerIconWidthProperty);
            internal set => SetValue(SmallerIconWidthProperty, value);
        }
    }
}

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Internal;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(":lightDismiss")]
[PseudoClasses(":actionButton", ":closeButton")]
[PseudoClasses(":content", ":icon")]
[PseudoClasses(":footerClose")]
[PseudoClasses(":heroContentTop", ":heroContentBottom")]
[PseudoClasses(s_pcTop, s_pcBottom, s_pcLeft, s_pcRight, s_pcCenter)]
[PseudoClasses(s_pcTopRight, s_pcTopLeft, s_pcBottomLeft, s_pcBottomRight)]
[PseudoClasses(s_pcLeftTop, s_pcLeftBottom, s_pcRightTop, s_pcRightBottom)]
[PseudoClasses(":showTitle", ":showSubTitle")]
public partial class TeachingTip : ContentControl
{
    public TeachingTip()
    {
        Unloaded += ClosePopupOnUnloadEvent;
        TemplateSettings = new TeachingTipTemplateSettings();

        this.GetPropertyChangedObservable(AutomationProperties.NameProperty).Subscribe(OnAutomationNameChanged);
        this.GetPropertyChangedObservable(AutomationProperties.AutomationIdProperty).Subscribe(OnAutomationIdChanged);
        TemplateSettings = new TeachingTipTemplateSettings();
    }

    protected override AutomationPeer OnCreateAutomationPeer() =>
        new TeachingTipAutomationPeer(this);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _acceleratorKeyActivatedRevoker?.Dispose();
        //m_previewKeyDownForF6Revoker
        _effectiveViewportChangedRevoker?.Revoke();
        _contentSizeChangedRevoker?.Dispose();
        if (_closeButton != null)
            _closeButton.Click -= OnCloseButtonClicked;

        if (_alternateCloseButton != null)
            _alternateCloseButton.Click -= OnCloseButtonClicked;

        if (_actionButton != null)
            _actionButton.Click -= OnActionButtonClicked;

        //_windowSizeChangedRevoker.Revoke();

        // BEGIN APPLY TEMPLATE

        // All of these components are required, so we use Get instead of Find to throw
        // if not found and avoid null checks below
        _container = e.NameScope.Get<Border>("Container");
        _rootElement = (Control)_container.Child;
        _tailOcclusionGrid = e.NameScope.Get<Grid>("TailOcclusionGrid");
        _contentRootGrid = e.NameScope.Get<Grid>("ContentRootGrid");
        _nonHeroContentRootGrid = e.NameScope.Get<Grid>("NonHeroContentRootGrid");
        _heroContentBorder = e.NameScope.Get<Border>("HeroContentBorder");
        _actionButton = e.NameScope.Get<Button>("ActionButton");
        _alternateCloseButton = e.NameScope.Get<Button>("AlternateCloseButton");
        _closeButton = e.NameScope.Get<Button>("CloseButton");
        // This isn't used in Fluentv2
        // _tailEdgeBorder = e.NameScope.Get<Grid>("TailEdgeBorder");
        _tailPolygon = e.NameScope.Get<Path>("TailPolygon");
        ToggleVisibilityForEmptyContent(s_TitleTextVisibleStateName, Title);
        ToggleVisibilityForEmptyContent(s_SubTitleTextVisibleStateName, Subtitle);

        // We rip out the bulk of the template content and reparent it into a Popup. This allows declaring
        // the TeachingTip in Xaml without worrying about its parent and making sure it returns back in the
        // right place and order. 
        _container.Child = null;

        _contentSizeChangedRevoker = _tailOcclusionGrid.GetObservable(BoundsProperty).Subscribe(OnContentSizeChanged);

        // We don't have LocalizedLandmarkType property, so skip this...
        //AutomationProperties.SetLocalizedLandmarkType(_contentRootGrid, ...)

        _closeButton.Click += OnCloseButtonClicked;

        _alternateCloseButton.Click += OnCloseButtonClicked;

        AutomationProperties.SetName(_alternateCloseButton, 
            FALocalizationHelper.Instance.GetLocalizedStringResource(SR_TeachingTipAlternateCloseButtonName));
        ToolTip.SetTip(_alternateCloseButton, 
            FALocalizationHelper.Instance.GetLocalizedStringResource(SR_TeachingTipAlternateCloseButtonTooltip));


        _actionButton.Click += OnActionButtonClicked;

        UpdateButtonsState();
        OnIsLightDismissEnabledChanged();
        OnIconSourceChanged();
        OnHeroContentPlacementChanged();

        UpdateButtonAutomationProperties(_actionButton, ActionButtonContent);
        UpdateButtonAutomationProperties(_closeButton, CloseButtonContent);

        EstablishShadows();

        _isTemplateApplied = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty)
        {
            OnIsOpenChanged();
        }
        else if (change.Property == TargetProperty)
        {
            var (oldVal, newVal) = change.GetOldAndNewValue<Control>();
            if (oldVal != null)
            {
                oldVal.Unloaded -= ClosePopupOnUnloadEvent;
            }

            if (newVal != null)
            {
                newVal.Unloaded += ClosePopupOnUnloadEvent;
            }
            OnTargetChanged();
        }
        else if (change.Property == PlacementMarginProperty)
        {
            OnPlacementMarginChanged();
        }
        else if (change.Property == IsLightDismissEnabledProperty)
        {
            OnIsLightDismissEnabledChanged();
        }
        else if (change.Property == ShouldConstrainToRootBoundsProperty)
        {
            OnShouldConstrainToRootBoundsChanged();
        }
        else if (change.Property == TailVisibilityProperty)
        {
            OnTailVisibilityChanged();
        }
        else if (change.Property == PreferredPlacementProperty)
        {
            if (IsOpen)
                PositionPopup();
        }
        else if (change.Property == HeroContentPlacementProperty)
        {
            OnHeroContentPlacementChanged();
        }
        else if (change.Property == IconSourceProperty)
        {
            OnIconSourceChanged();
        }
        else if (change.Property == TitleProperty)
        {
            SetPopupAutomationProperties();
            ToggleVisibilityForEmptyContent(s_TitleTextVisibleStateName, change.GetNewValue<string>());
        }
        else if (change.Property == SubtitleProperty)
        {
            ToggleVisibilityForEmptyContent(s_TitleTextVisibleStateName, change.GetNewValue<string>());
        }
        else if (change.Property == ActionButtonContentProperty)
        {
            UpdateButtonsState();
            UpdateButtonAutomationProperties(_actionButton, change.NewValue);
        }
        else if (change.Property == CloseButtonContentProperty)
        {
            UpdateButtonsState();
            UpdateButtonAutomationProperties(_closeButton, change.NewValue);
        }
        else if (change.Property == ContentProperty)
        {
            PseudoClasses.Set(":content", change.NewValue != null);
        }
    }

    private void UpdateButtonAutomationProperties(Button button, object obj)
    {
        if (button == null)
            return;

        // SharedHelpers::TryGetStringRepresentationFromObjet(content)
        var nameHString = obj is string s ? s : string.Empty;
        AutomationProperties.SetName(button, nameHString);
    }

    private bool ToggleVisibilityForEmptyContent(string visibleStatename, string content)
    {
        bool visible = !string.IsNullOrEmpty(content);
        PseudoClasses.Set(visibleStatename, visible);

        return visible;
    }

    private void SetPopupAutomationProperties()
    {
        if (_popup != null)
        {
            var name = AutomationProperties.GetName(this);
            if (string.IsNullOrEmpty(name))
            {
                name = Title;
            }
            AutomationProperties.SetName(_popup, name);

            AutomationProperties.SetAutomationId(_popup, AutomationProperties.GetAutomationId(this));
        }
    }

    // Playing a closing animation when the Teaching Tip is closed via light dismiss requires this work around.
    // This is because there is no event that occurs when a popup is closing due to light dismiss so we have no way to intercept
    // the close and play our animation first. To work around this we've created a second popup which has no content and sits
    // underneath the teaching tip and is put into light dismiss mode instead of the primary popup. Then when this popup closes
    // due to light dismiss we know we are supposed to close the primary popup as well. To ensure that this popup does not block
    // interaction to the primary popup we need to make sure that the LightDismissIndicatorPopup is always opened first, so that
    // it is Z ordered underneath the primary popup.
    private void CreateLightDismissIndiatorPopup()
    {
        var popup = new Popup
        {
            // A Popup needs contents to open, so set a child that doesn't do anything.
            Child = new Panel(),
            WindowManagerAddShadowHint = false,
            IsLightDismissEnabled = true,
            PlacementTarget = (Control)VisualRoot
        };

        _lightDismissIndicatorPopup = popup;
    }

    private bool UpdateTail()
    {
        // An effective placement of auto indicates that no tail should be shown.
        var (placement, tipDoesNotFit) = DetermineEffectivePlacement();
        _currentEffectiveTailPlacementMode = placement;
        var tailVisiblity = TailVisibility;

        if (tailVisiblity == TeachingTipTailVisibility.Collapsed ||
            (_target == null && tailVisiblity != TeachingTipTailVisibility.Visible))
        {
            _currentEffectiveTailPlacementMode = TeachingTipPlacementMode.Auto;
        }

        if (placement != _currentEffectiveTipPlacementMode)
        {
            _currentEffectiveTipPlacementMode = placement;
        }

        var height = _tailOcclusionGrid?.Bounds.Height ?? 0;
        var width = _tailOcclusionGrid?.Bounds.Width ?? 0;

        getColumnWidths(_tailOcclusionGrid, out var firstColumnWidth, out var secondColumnWidth,
            out var nextToLastColumnWidth, out var lastColumnWidth);
        getRowWidths(_tailOcclusionGrid, out var firstRowHeight, out var secondRowHeight,
            out var nextToLastRowHeight, out var lastRowHeight);

        UpdateSizeBasedTemplateSettings();

        switch (_currentEffectiveTailPlacementMode)
        {
            case TeachingTipPlacementMode.Auto:
                TrySetCenterPoint(_tailOcclusionGrid, width / 2, height / 2);
                UpdateDynamicHeroContentPlacementToTop();
                GoToState((TeachingTipPlacementMode)(-1));
                break;

            case TeachingTipPlacementMode.Top:
                TrySetCenterPoint(_tailOcclusionGrid, width / 2, height - lastRowHeight);
                //TrySetCenterPoint(_tailEdgeBorder, width / 2 - firstColumnWidth, 0);
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.Top);
                break;

            case TeachingTipPlacementMode.Bottom:
                TrySetCenterPoint(_tailOcclusionGrid, width / 2, firstRowHeight);
                //TrySetCenterPoint(_tailEdgeBorder, width / 2 - firstColumnWidth, 0);
                UpdateDynamicHeroContentPlacementToBottom();
                GoToState(TeachingTipPlacementMode.Bottom);
                break;

            case TeachingTipPlacementMode.Left:
                TrySetCenterPoint(_tailOcclusionGrid, width - lastColumnWidth, height / 2);
                //TrySetCenterPoint(_tailEdgeBorder, 0, height / 2 - firstRowHeight);
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.Left);
                break;

            case TeachingTipPlacementMode.Right:
                TrySetCenterPoint(_tailOcclusionGrid, firstColumnWidth, height / 2);
                //TrySetCenterPoint(_tailEdgeBorder, 0, height / 2 - firstRowHeight);
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.Right);
                break;

            case TeachingTipPlacementMode.TopRight:
                TrySetCenterPoint(_tailOcclusionGrid, firstColumnWidth + secondColumnWidth + 1, height - lastRowHeight);
                //TrySetCenterPoint(_tailEdgeBorder, secondColumnWidth, 0);
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.TopRight);
                break;

            case TeachingTipPlacementMode.TopLeft:
                TrySetCenterPoint(_tailOcclusionGrid, width - (nextToLastColumnWidth + lastColumnWidth + 1), height - lastRowHeight);
                //TrySetCenterPoint(_tailEdgeBorder, width - (nextToLastColumnWidth + firstColumnWidth + lastColumnWidth), 0);
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.TopLeft);
                break;

            case TeachingTipPlacementMode.BottomRight:
                TrySetCenterPoint(_tailOcclusionGrid, firstColumnWidth + secondColumnWidth + 1, firstRowHeight);
                //TrySetCenterPoint(_tailEdgeBorder, secondColumnWidth, 0);
                UpdateDynamicHeroContentPlacementToBottom();
                GoToState(TeachingTipPlacementMode.BottomRight);
                break;

            case TeachingTipPlacementMode.BottomLeft:
                TrySetCenterPoint(_tailOcclusionGrid, width - (nextToLastColumnWidth + lastColumnWidth + 1), firstRowHeight);
                //TrySetCenterPoint(_tailEdgeBorder, width - (nextToLastColumnWidth + firstColumnWidth + lastColumnWidth), 0);
                UpdateDynamicHeroContentPlacementToBottom();
                GoToState(TeachingTipPlacementMode.BottomLeft);
                break;

            case TeachingTipPlacementMode.LeftTop:
                TrySetCenterPoint(_tailOcclusionGrid, width - lastColumnWidth, height - (nextToLastRowHeight + lastRowHeight + 1));
               // TrySetCenterPoint(_tailEdgeBorder, 0, height - (nextToLastRowHeight + firstRowHeight + lastRowHeight));
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.LeftTop);
                break;

            case TeachingTipPlacementMode.LeftBottom:
                TrySetCenterPoint(_tailOcclusionGrid, width - lastColumnWidth, firstRowHeight + secondRowHeight + 1);
                //TrySetCenterPoint(_tailEdgeBorder, 0, secondRowHeight);
                UpdateDynamicHeroContentPlacementToBottom();
                GoToState(TeachingTipPlacementMode.LeftBottom);
                break;

            case TeachingTipPlacementMode.RightTop:
                TrySetCenterPoint(_tailOcclusionGrid, firstColumnWidth, height - (nextToLastRowHeight + lastRowHeight + 1));
                //TrySetCenterPoint(_tailEdgeBorder, 0, height - (nextToLastRowHeight + firstRowHeight + lastRowHeight));
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.RightTop);
                break;

            case TeachingTipPlacementMode.RightBottom:
                TrySetCenterPoint(_tailOcclusionGrid, firstColumnWidth, firstRowHeight + secondRowHeight + 1);
                //TrySetCenterPoint(_tailEdgeBorder, 0, secondRowHeight);
                UpdateDynamicHeroContentPlacementToBottom();
                GoToState(TeachingTipPlacementMode.RightBottom);
                break;

            case TeachingTipPlacementMode.Center:
                TrySetCenterPoint(_tailOcclusionGrid, width / 2, height - lastRowHeight);
                //TrySetCenterPoint(_tailEdgeBorder, width / 2 - firstColumnWidth, 0);
                UpdateDynamicHeroContentPlacementToTop();
                GoToState(TeachingTipPlacementMode.Center);
                break;

            default:
                break;

        }

        return tipDoesNotFit;


        static void getColumnWidths(Grid g, out double firstColumnWidth,
            out double secondColumnWidth, out double nextToLastColumnWidth,
            out double lastColumnWidth)
        {
            firstColumnWidth = secondColumnWidth = nextToLastColumnWidth = lastColumnWidth = 0;
            if (g == null || g.ColumnDefinitions == null)
                return;
            var numColDefs = g.ColumnDefinitions.Count;

            firstColumnWidth = numColDefs > 0 ? g.ColumnDefinitions[0].ActualWidth : 0;
            secondColumnWidth = numColDefs > 1 ? g.ColumnDefinitions[1].ActualWidth : 0;
            nextToLastColumnWidth = numColDefs > 1 ? g.ColumnDefinitions[numColDefs - 2].ActualWidth : 0;
            lastColumnWidth = numColDefs > 0 ? g.ColumnDefinitions[numColDefs - 1].ActualWidth : 0;
        }

        static void getRowWidths(Grid g, out double firstRowHeight,
            out double secondRowHeight, out double nextToLastRowHeight,
            out double lastRowHeight)
        {
            firstRowHeight = secondRowHeight = nextToLastRowHeight = lastRowHeight = 0;
            if (g == null || g.RowDefinitions == null)
                return;
            var numColDefs = g.RowDefinitions.Count;

            firstRowHeight = numColDefs > 0 ? g.RowDefinitions[0].ActualHeight : 0;
            secondRowHeight = numColDefs > 1 ? g.RowDefinitions[1].ActualHeight : 0;
            nextToLastRowHeight = numColDefs > 1 ? g.RowDefinitions[numColDefs - 2].ActualHeight : 0;
            lastRowHeight = numColDefs > 0 ? g.RowDefinitions[numColDefs - 1].ActualHeight : 0;
        }

        void GoToState(TeachingTipPlacementMode mode)
        {
            // Unfortunately this gets ugly because we don't have a VisualStateManager like WinUI
            // We can't just use combinations of pseudoclasses here because we'd have no way to
            // differentiate between LeftTop and TopLeft visual states, for example
            if ((int)mode == -1) // Untargeted, remove all position pseudoclasses
            {                
                PseudoClasses.Set(s_pcTop, false);
                PseudoClasses.Set(s_pcBottom, false);
                PseudoClasses.Set(s_pcLeft, false);
                PseudoClasses.Set(s_pcRight, false);
                PseudoClasses.Set(s_pcCenter, false);
                PseudoClasses.Set(s_pcTopRight, false);
                PseudoClasses.Set(s_pcTopLeft, false);
                PseudoClasses.Set(s_pcBottomLeft, false);
                PseudoClasses.Set(s_pcBottomRight, false);
                PseudoClasses.Set(s_pcLeftTop, false);
                PseudoClasses.Set(s_pcLeftBottom, false);
                PseudoClasses.Set(s_pcRightBottom, false);
                PseudoClasses.Set(s_pcRightTop, false);
                return;
            }

            switch (mode)
            {
                case TeachingTipPlacementMode.Top:
                    {
                        PseudoClasses.Set(s_pcTop, true);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.Bottom:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, true);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.Left:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, true);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.Right:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, true);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.TopRight:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, true);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.TopLeft:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, true);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.BottomRight:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, true);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.BottomLeft:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, true);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.LeftTop:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, true);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.LeftBottom:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, true);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.RightTop:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, true);
                    }
                    break;

                case TeachingTipPlacementMode.RightBottom:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, false);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, true);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;

                case TeachingTipPlacementMode.Center:
                    {
                        PseudoClasses.Set(s_pcTop, false);
                        PseudoClasses.Set(s_pcBottom, false);
                        PseudoClasses.Set(s_pcLeft, false);
                        PseudoClasses.Set(s_pcRight, false);
                        PseudoClasses.Set(s_pcCenter, true);
                        PseudoClasses.Set(s_pcTopRight, false);
                        PseudoClasses.Set(s_pcTopLeft, false);
                        PseudoClasses.Set(s_pcBottomLeft, false);
                        PseudoClasses.Set(s_pcBottomRight, false);
                        PseudoClasses.Set(s_pcLeftTop, false);
                        PseudoClasses.Set(s_pcLeftBottom, false);
                        PseudoClasses.Set(s_pcRightBottom, false);
                        PseudoClasses.Set(s_pcRightTop, false);
                    }
                    break;
            }
        }
    }

    private void PositionPopup()
    {
        bool tipDoesNotFit = false;
        if (_target != null)
        {
            tipDoesNotFit = PositionTargetedPopup();
        }
        else
        {
            tipDoesNotFit = PositionUntargetedPopup();
        }

        if (tipDoesNotFit)
        {
            IsOpen = false;
        }
    }

    private bool PositionTargetedPopup()
    {
        bool tipDoesNotFit = UpdateTail();
        var offset = PlacementMargin;

        var (tipHeight, tipWidth) = _tailOcclusionGrid != null ?
            (_tailOcclusionGrid.Bounds.Height, _tailOcclusionGrid.Bounds.Width) : (0, 0);

        if (_popup != null)
        {
            // Depending on the effective placement mode of the tip we use a combination of the tip's size, the target's position within the app, the target's
            // size, and the target offset property to determine the appropriate vertical and horizontal offsets of the popup that the tip is contained in.
            switch (_currentEffectiveTipPlacementMode)
            {
                case TeachingTipPlacementMode.Top:
                    _popup.VerticalOffset = _currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top;
                    _popup.HorizontalOffset = (((_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0);
                    break;

                case TeachingTipPlacementMode.Bottom:
                    _popup.VerticalOffset = _currentTargetBoundsInCoreWindowSpace.Y + _currentTargetBoundsInCoreWindowSpace.Height + offset.Bottom;
                    _popup.HorizontalOffset = (((_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f);
                    break;

                case TeachingTipPlacementMode.Left:
                    _popup.VerticalOffset = ((_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Height - tipHeight) / 2.0f;
                    _popup.HorizontalOffset = _currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left;
                    break;

                case TeachingTipPlacementMode.Right:
                    _popup.VerticalOffset = ((_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Height - tipHeight) / 2.0f;
                    _popup.HorizontalOffset = _currentTargetBoundsInCoreWindowSpace.X + _currentTargetBoundsInCoreWindowSpace.Width + offset.Right;
                    break;

                case TeachingTipPlacementMode.TopRight:
                    _popup.VerticalOffset = _currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top;
                    _popup.HorizontalOffset = ((((_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - MinimumTipEdgeToTailCenter());
                    break;

                case TeachingTipPlacementMode.TopLeft:
                    _popup.VerticalOffset = _currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top;
                    _popup.HorizontalOffset = ((((_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - tipWidth + MinimumTipEdgeToTailCenter());
                    break;

                case TeachingTipPlacementMode.BottomRight:
                    _popup.VerticalOffset = _currentTargetBoundsInCoreWindowSpace.Y + _currentTargetBoundsInCoreWindowSpace.Height + offset.Bottom;
                    _popup.HorizontalOffset = ((((_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - MinimumTipEdgeToTailCenter());
                    break;

                case TeachingTipPlacementMode.BottomLeft:
                    _popup.VerticalOffset = _currentTargetBoundsInCoreWindowSpace.Y + _currentTargetBoundsInCoreWindowSpace.Height + offset.Bottom;
                    _popup.HorizontalOffset = ((((_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - tipWidth + MinimumTipEdgeToTailCenter());
                    break;

                case TeachingTipPlacementMode.LeftTop:
                    _popup.VerticalOffset = (((_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - tipHeight + MinimumTipEdgeToTailCenter();
                    _popup.HorizontalOffset = _currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left;
                    break;

                case TeachingTipPlacementMode.LeftBottom:
                    _popup.VerticalOffset = (((_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - MinimumTipEdgeToTailCenter();
                    _popup.HorizontalOffset = _currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left;
                    break;

                case TeachingTipPlacementMode.RightTop:
                    _popup.VerticalOffset = (((_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - tipHeight + MinimumTipEdgeToTailCenter();
                    _popup.HorizontalOffset = _currentTargetBoundsInCoreWindowSpace.X + _currentTargetBoundsInCoreWindowSpace.Width + offset.Right;
                    break;

                case TeachingTipPlacementMode.RightBottom:
                    _popup.VerticalOffset = (((_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - MinimumTipEdgeToTailCenter();
                    _popup.HorizontalOffset = _currentTargetBoundsInCoreWindowSpace.X + _currentTargetBoundsInCoreWindowSpace.Width + offset.Right;
                    break;

                case TeachingTipPlacementMode.Center:
                    _popup.VerticalOffset = _currentTargetBoundsInCoreWindowSpace.Y + (_currentTargetBoundsInCoreWindowSpace.Height / 2.0f) - tipHeight - offset.Top;
                    _popup.HorizontalOffset = (((_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + _currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f);
                    break;

                default:
                    throw new Exception("Invalid TeachingTipPlacementMode");
            }
        }

        return tipDoesNotFit;
    }

    private bool PositionUntargetedPopup()
    {
        var windowBoundsInCoreWindowSpace = GetEffectiveWindowBoundsInCoreWindowSpace(GetWindowBounds());

        var (finalTipHeight, finalTipWidth) = _tailOcclusionGrid != null ?
            (_tailOcclusionGrid.Bounds.Height, _tailOcclusionGrid.Bounds.Width) : (0, 0);

        bool tipDoesNotFit = UpdateTail();

        var offset = PlacementMargin;

        // Depending on the effective placement mode of the tip we use a combination of the tip's size, the window's size, and the target
        // offset property to determine the appropriate vertical and horizontal offsets of the popup that the tip is contained in.
        if (_popup != null)
        {
            switch (GetFlowDirectionAdjustedPlacement(PreferredPlacement))
            {
                case TeachingTipPlacementMode.Auto:
                case TeachingTipPlacementMode.Bottom:
                    _popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right);
                    break;

                case TeachingTipPlacementMode.Top:
                    _popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                    _popup.HorizontalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right);
                    break;

                case TeachingTipPlacementMode.Left:
                    _popup.VerticalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                    break;

                case TeachingTipPlacementMode.Right:
                    _popup.VerticalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                    break;

                case TeachingTipPlacementMode.TopRight:
                    _popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                    _popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                    break;

                case TeachingTipPlacementMode.TopLeft:
                    _popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                    _popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                    break;

                case TeachingTipPlacementMode.BottomRight:
                    _popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                    break;

                case TeachingTipPlacementMode.BottomLeft:
                    _popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                    break;

                case TeachingTipPlacementMode.LeftTop:
                    _popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                    _popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                    break;

                case TeachingTipPlacementMode.LeftBottom:
                    _popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                    break;

                case TeachingTipPlacementMode.RightTop:
                    _popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                    _popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                    break;

                case TeachingTipPlacementMode.RightBottom:
                    _popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                    break;

                case TeachingTipPlacementMode.Center:
                    _popup.VerticalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom);
                    _popup.HorizontalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right);
                    break;

                default:
                    throw new Exception("Invalid TeachingTipPlacementMode");
            }
        }

        return tipDoesNotFit;
    }

    private void UpdateSizeBasedTemplateSettings()
    {
        var templateSettings = TemplateSettings;

        var (width, height) = _contentRootGrid != null ?
            (_contentRootGrid.Bounds.Width, _contentRootGrid.Bounds.Height) : (0, 0);

        switch (_currentEffectiveTailPlacementMode)
        {
            case TeachingTipPlacementMode.Top:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.Bottom:
                templateSettings.TopRightHighlightMargin = BottomPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = BottomPlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.Left:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = LeftEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.Right:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = RightEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.TopLeft:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.TopRight:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.BottomLeft:
                templateSettings.TopRightHighlightMargin = BottomLeftPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = BottomLeftPlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.BottomRight:
                templateSettings.TopRightHighlightMargin = BottomRightPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = BottomRightPlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.LeftTop:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = LeftEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.LeftBottom:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = LeftEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.RightTop:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = RightEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.RightBottom:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = RightEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.Auto:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                break;
            case TeachingTipPlacementMode.Center:
                templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                break;
        }
    }

    private void UpdateButtonsState()
    {
        // WinUI:
        // if (actionContent && closeContent)
        //    BothButtonsVisible, FooterCloseButton
        // else if (actionContent && isLightDismiss)
        //    ActionButtonVisible, FooterCloseButton
        // else if (actionContent)
        //    ActionButtonVisible, HeaderCloseButton
        // else if (closeContent)
        //    CloseButtonVisible, FooterCloseButton
        // else if (isLightDismiss)
        //    NoButtonsVisible, FooterCloseButton
        // else
        //    NoButtonsVisible, HeaderCloseButton

        // We use pseudoclass combination here, so :actionButton:closeButton = BothButtonsVisible VSM state
        // NoButtonsVisible VSM state is the default state here

        var actionContent = ActionButtonContent;
        var closeContent = CloseButtonContent;
        var isLightDismiss = IsLightDismissEnabled;

        PseudoClasses.Set(":actionButton", actionContent != null);
        PseudoClasses.Set(":closeButton", closeContent != null);

        // HeaderCloseButton is the default state
        PseudoClasses.Set(":footerClose", isLightDismiss || closeContent != null);
    }

    private void UpdateDynamicHeroContentPlacementToTop()
    {
        if (HeroContentPlacement == TeachingTipHeroContentPlacementMode.Auto)
        {
            UpdateDynamicHeroContentPlacementToTopImpl();
        }
    }

    private void UpdateDynamicHeroContentPlacementToTopImpl()
    {
        PseudoClasses.Set(":heroContentTop", true);
        PseudoClasses.Set(":heroContentBottom", false);

        if (_currentHeroContentEffectivePlacementMode != TeachingTipHeroContentPlacementMode.Top)
        {
            _currentHeroContentEffectivePlacementMode = TeachingTipHeroContentPlacementMode.Top;
        }
    }

    private void UpdateDynamicHeroContentPlacementToBottom()
    {
        if (HeroContentPlacement == TeachingTipHeroContentPlacementMode.Auto)
        {
            UpdateDynamicHeroContentPlacementToBottomImpl();
        }
    }

    private void UpdateDynamicHeroContentPlacementToBottomImpl()
    {
        PseudoClasses.Set(":heroContentTop", false);
        PseudoClasses.Set(":heroContentBottom", true);

        if (_currentHeroContentEffectivePlacementMode != TeachingTipHeroContentPlacementMode.Bottom)
        {
            _currentHeroContentEffectivePlacementMode = TeachingTipHeroContentPlacementMode.Bottom;
        }
    }

    private void OnIsOpenChanged()
    {
        if (_ignoreNextIsOpenChanged)
        {
            _ignoreNextIsOpenChanged = false;
        }
        else
        {
            // SharedHelpers::QueueCallbackForCompositionRendering
            Dispatcher.UIThread.Post(() =>
            {
                if (_isIdle)
                {
                    if (IsOpen)
                    {
                        IsOpenChangedToOpen();
                    }
                    else
                    {
                        IsOpenChangedToClose();
                    }
                }
                else
                {
                    _ignoreNextIsOpenChanged = true;
                    IsOpen = !IsOpen;
                }
            }, DispatcherPriority.Render);
        }
    }

    private void IsOpenChangedToOpen()
    {
        //Reset the close reason to the default value of programmatic.
        _lastCloseReason = TeachingTipCloseReason.Programmatic;

        _currentBoundsInCoreWindowSpace = new Rect(Bounds.Size).TransformToAABB(this.TransformToVisual(VisualRoot) ?? Matrix.Identity);

        if (_target != null)
        {
            SetViewportChangedEvent(_target);
            _currentTargetBoundsInCoreWindowSpace = new Rect(_target.Bounds.Size).TransformToAABB(_target.TransformToVisual(_target.GetVisualRoot()) ?? Matrix.Identity);
        }
        else
        {
            _currentTargetBoundsInCoreWindowSpace = Rect.Empty;
        }

        if (_lightDismissIndicatorPopup == null)
        {
            CreateLightDismissIndiatorPopup();
        }
        OnIsLightDismissEnabledChanged();

        if (_contractAnimation == null)
        {
            CreateContractAnimation();
        }
        if (_expandAnimation == null)
        {
            CreateExpandAnimation();
        }

        // If the developer defines their TeachingTip in a resource dictionary it is possible that it's template will have never been applied
        if (!_isTemplateApplied)
        {
            ApplyTemplate();
        }

        if (_popup == null || _createNewPopupOnOpen)
        {
            CreateNewPopup();
        }

        // If the tip is not going to open because it does not fit we need to make sure that
        // the open, closing, closed life cycle still fires so that we don't cause apps to leak
        // that depend on this sequence.
        var (ignored, tipDoesNotFit) = DetermineEffectivePlacement();
        if (tipDoesNotFit)
        {
            RaiseClosingEvent(false);
            var closedArgs = new TeachingTipClosedEventArgs(_lastCloseReason);
            Closed?.Invoke(this, closedArgs);
            IsOpen = false;
        }
        else
        {
            if (_popup != null)
            {
                // We have to do this so styles inherit 
                ((ISetLogicalParent)_popup).SetParent(_target ?? (Control)VisualRoot);

                if (!_popup.IsOpen)
                {
                    // We are about to begin the process of trying to open the teaching tip, so notify that we are no longer idle.
                    SetIsIdle(false);
                    // UpdatePopupRequestedTheme(); // TODO:??
                    _popup.Child = _rootElement;

                    if (_lightDismissIndicatorPopup != null)
                    {
                        _lightDismissIndicatorPopup.IsOpen = true;
                    }

                    _popup.IsOpen = true;
                }
                else
                {
                    // We have become Open but our popup was already open. This can happen when a close is canceled by the closing event, so make sure the idle status is correct.
                    if (!_isExpandAnimationPlaying && !_isContractAnimationPlaying)
                    {
                        SetIsIdle(true);
                    }
                }
            }
        }

        if (VisualRoot != null)
        {
            _acceleratorKeyActivatedRevoker = (VisualRoot as Interactive).AddDisposableHandler(KeyDownEvent, OnF6PreviewKeyDownClicked, RoutingStrategies.Tunnel);
        }

        // Make sure we are in the correct VSM state after ApplyTemplate and moving the template content from the Control to the Popup:
        OnIsLightDismissEnabledChanged();
    }

    private void IsOpenChangedToClose()
    {
        if (_popup != null)
        {
            if (_popup.IsOpen)
            {
                // We are about to begin the process of trying to close the teaching tip, so notify that we are no longer idle.
                SetIsIdle(false);
                RaiseClosingEvent(true);
            }
            else
            {
                // We have become not Open but our popup was already not open. Lets make sure the idle status is correct.
                if (!_isExpandAnimationPlaying && !_isContractAnimationPlaying)
                {
                    SetIsIdle(true);
                }
            }
        }

        _acceleratorKeyActivatedRevoker?.Dispose();
        _currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
    }

    private void CreateNewPopup()
    {
        if (_popup != null)
        {
            _popup.Opened -= OnPopupOpened;
            _popup.Closed -= OnPopupClosed;
        }

        var popup = new Popup
        {
            WindowManagerAddShadowHint = false,
            IsLightDismissEnabled = false,
            PlacementTarget = (Control)VisualRoot,
            // Raw Popups in WinUI don't have placement methods like we have and always positioned at <0,0> in the Window
            // so we mimic that here so that the remaining positioning logic elsewhere in this code still works
            PlacementMode = PlacementMode.AnchorAndGravity,
            PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.TopLeft,
            PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.BottomRight
        };

        popup.Opened += OnPopupOpened;
        popup.Closed += OnPopupClosed;

        _popup = popup;
        SetPopupAutomationProperties();
        _createNewPopupOnOpen = false;
    }

    private void OnTailVisibilityChanged()
    {
        UpdateTail();
    }

    private void OnIconSourceChanged()
    {
        var ts = TemplateSettings;
        var ico = IconSource;

        if (ico != null)
        {
            ts.IconElement = IconHelpers.CreateFromUnknown(ico);
        }
        else
        {
            ts.IconElement = null;
        }

        PseudoClasses.Set(":icon", ico != null);
    }

    private void OnPlacementMarginChanged()
    {
        if (IsOpen)
        {
            PositionPopup();
        }
    }

    private void OnIsLightDismissEnabledChanged()
    {
        bool ld = IsLightDismissEnabled;
        PseudoClasses.Set(":lightDismiss", ld);

        if (ld)
        {
            if (_lightDismissIndicatorPopup != null)
            {
                _lightDismissIndicatorPopup.IsLightDismissEnabled = true;
                _lightDismissIndicatorPopup.Closed += OnLightDismissIndicatorPopupClosed;
            }
        }
        else
        {
            if (_lightDismissIndicatorPopup != null)
            {
                _lightDismissIndicatorPopup.IsLightDismissEnabled = false;
                _lightDismissIndicatorPopup.Closed -= OnLightDismissIndicatorPopupClosed;
            }
        }

        UpdateButtonsState();
    }

    private void OnShouldConstrainToRootBoundsChanged()
    {
        // FA: This kind of requires support from the underlying Popup API rather than try to hack this together
        // here. So this property is unsupported for now.

        throw new NotSupportedException("ShouldConstrainToRootBounds property is not supported at this time");

        // ShouldConstrainToRootBounds is a property that can only be set on a popup before it is opened.
        // If we have opened the tip's popup and then this property changes we will need to discard the old popup
        // and replace it with a new popup.  This variable indicates this state.

        //The underlying popup api is only available on 19h1 plus, if we aren't on that no opt.
        // ...
    }

    private void OnHeroContentPlacementChanged()
    {
        switch (HeroContentPlacement)
        {
            case TeachingTipHeroContentPlacementMode.Auto:
                break;

            case TeachingTipHeroContentPlacementMode.Top:
                UpdateDynamicHeroContentPlacementToTopImpl();
                break;

            case TeachingTipHeroContentPlacementMode.Bottom:
                UpdateDynamicHeroContentPlacementToBottomImpl();
                break;
        }

        // Setting m_currentEffectiveTipPlacementMode to auto ensures that the next time position popup is called we'll rerun the DetermineEffectivePlacement
        // algorithm. If we did not do this and the popup was opened the algorithm would maintain the current effective placement mode, which we don't want
        // since the hero content placement contributes to the choice of tip placement mode.
        _currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
        if (IsOpen)
        {
            PositionPopup();
        }
    }

    private void OnContentSizeChanged(Rect rc)
    {
        UpdateSizeBasedTemplateSettings();
        // Reset the currentEffectivePlacementMode so that the tail will be updated for the new size as well.
        _currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;

        if (IsOpen)
        {
            PositionPopup();
        }

        if (_expandAnimation != null)
        {
            _expandAnimation.SetScalarParameter("Width", (float)rc.Width);
            _expandAnimation.SetScalarParameter("Height", (float)rc.Height);
        }
        if (_contractAnimation != null)
        {
            _contractAnimation.SetScalarParameter("Width", (float)rc.Width);
            _contractAnimation.SetScalarParameter("Height", (float)rc.Height);
        }
    }

    private void OnF6PreviewKeyDownClicked(object sender, KeyEventArgs args)
    {
        if (!args.Handled && IsOpen && args.Key == Key.F6)
        {
            args.Handled = HandleF6Clicked();
        }
    }

    private bool HandleF6Clicked(bool fromPopup = false)
    {
        bool hasFocusInSubtree()
        {
            if (_rootElement != null)
            {
                IVisual current = FocusManager.Instance.Current;

                while (current != null)
                {
                    if (current == _rootElement)
                        return true;

                    current = current.VisualParent;
                }
            }

            return false;
        }

        if (hasFocusInSubtree() && fromPopup)
        {
            if (_previouslyFocusedElement != null)
            {
                FocusManager.Instance.Focus(_previouslyFocusedElement, NavigationMethod.Unspecified);
                _previouslyFocusedElement = null;

                return FocusManager.Instance.Current == _previouslyFocusedElement;
            }
        }
        else if (!hasFocusInSubtree() && !fromPopup)
        {
            var (firstButton, secondButton) = (_closeButton, _alternateCloseButton);
            Button f6Button = null;
            //Prefer the close button to the alternate, except when there is no content.
            if (CloseButtonContent == null)
            {
                (firstButton, secondButton) = (_alternateCloseButton, _closeButton);
            }

            if (firstButton != null && firstButton.IsVisible)
            {
                f6Button = firstButton;
            }
            else if (secondButton != null && secondButton.IsVisible)
            {
                f6Button = secondButton;
            }

            if (f6Button != null)
            {
                _previouslyFocusedElement = FocusManager.Instance.Current;
                FocusManager.Instance.Focus(f6Button, NavigationMethod.Directional);
                return FocusManager.Instance.Current == f6Button;
            }
        }

        return false;
    }

    private void OnAutomationNameChanged(AvaloniaPropertyChangedEventArgs args)
    {
        SetPopupAutomationProperties();
    }

    private void OnAutomationIdChanged(AvaloniaPropertyChangedEventArgs args)
    {
        SetPopupAutomationProperties();
    }

    private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
    {
        CloseButtonClick?.Invoke(this, EventArgs.Empty);
        _lastCloseReason = TeachingTipCloseReason.CloseButton;
        IsOpen = false;
    }

    private void OnActionButtonClicked(object sender, RoutedEventArgs e)
    {
        ActionButtonClick?.Invoke(this, EventArgs.Empty);
    }

    private void OnPopupOpened(object sender, EventArgs args)
    {
        _currentXamlRootSize = VisualRoot.ClientSize;
        if (VisualRoot is Control c)
        {
            _xamlRootChangedRevoker = c.GetObservable(BoundsProperty).Subscribe(XamlRootChanged);
        }

        if (FAUISettings.AreAnimationsEnabled())
            StartExpandToOpen();
        else
            SetIsIdle(true);

        // TODO: Automation stuff...
    }

    private void OnPopupClosed(object sender, EventArgs args)
    {
        _xamlRootChangedRevoker?.Dispose();

        if (_lightDismissIndicatorPopup != null)
            _lightDismissIndicatorPopup.IsOpen = false;

        if (_popup != null)
        {
            _popup.Child = null;
        }

        var cArgs = new TeachingTipClosedEventArgs(_lastCloseReason);
        Closed?.Invoke(this, cArgs);

        //If we were closed by the close button and we have tracked a previously focused element because F6 was used
        //To give the tip focus, then we return focus when the popup closes.
        if (_lastCloseReason == TeachingTipCloseReason.CloseButton)
        {
            FocusManager.Instance.Focus(_previouslyFocusedElement, NavigationMethod.Unspecified);
        }
        _previouslyFocusedElement = null;

        // TODO: AutomationPeer stuff
    }

    private void ClosePopupOnUnloadEvent(object sender, RoutedEventArgs e)
    {
        IsOpen = false;
        ClosePopup();
    }

    private void OnLightDismissIndicatorPopupClosed(object sender, EventArgs e)
    {
        if (IsOpen)
        {
            _lastCloseReason = TeachingTipCloseReason.LightDismiss;
        }
        IsOpen = false;
    }
   
    private void RaiseClosingEvent(bool attachDeferralCompletedHandler)
    {
        var args = new TeachingTipClosingEventArgs(_lastCloseReason);

        if (attachDeferralCompletedHandler)
        {
            var deferral = new Deferral(() =>
            {
                Dispatcher.UIThread.VerifyAccess();
                if (!args.Cancel)
                {
                    ClosePopupWithAnimationIfAvailable();
                }
                else
                {
                    // The developer has changed the Cancel property to true, indicating that they wish to Cancel the
                    // closing of this tip, so we need to revert the IsOpen property to true.
                    IsOpen = true;
                }
            });

            args.SetDeferral(deferral);

            args.IncrementDeferralCount();
            Closing?.Invoke(this, args);
            args.DecrementDeferralCount();
        }
        else
        {
            Closing?.Invoke(this, args);
        }
    }

    private void ClosePopupWithAnimationIfAvailable()
    {
        if (_popup?.IsOpen == true)
        {
            if (FAUISettings.AreAnimationsEnabled())
                StartContractToClose();
            else
                ClosePopup();

            // Under normal circumstances we would have launched an animation just now, if we did not then we should make sure
            // that the idle state is correct.
            if (!_isContractAnimationPlaying && !_isExpandAnimationPlaying)
            {
                SetIsIdle(true);
            }
        }
    }

    private void ClosePopup()
    {
        if (_popup != null)
            _popup.IsOpen = false;

        if (_lightDismissIndicatorPopup != null)
            _lightDismissIndicatorPopup.IsOpen = false;

        if (_tailOcclusionGrid != null)
        {
            // A previous close animation may have left the rootGrid's scale at a very small value and if this teaching tip
            // is shown again then its text would be rasterized at this small scale and blown up ~20x. To fix this we have to
            // reset the scale after the popup has closed so that if the teaching tip is re-shown the render pass does not use the
            // small scale.

            var ev = ElementComposition.GetElementVisual(_tailOcclusionGrid);
            if (ev != null)
            {
                ev.Scale = Vector3.One;
            }
        }
    }

    private TeachingTipPlacementMode GetFlowDirectionAdjustedPlacement(TeachingTipPlacementMode pm)
    {
        if (FlowDirection == Avalonia.Media.FlowDirection.LeftToRight)
        {
            return pm;
        }
        else
        {
            switch (pm)
            {
                case TeachingTipPlacementMode.Left:
                    return TeachingTipPlacementMode.Right;

                case TeachingTipPlacementMode.Right:
                    return TeachingTipPlacementMode.Left;

                case TeachingTipPlacementMode.LeftBottom:
                    return TeachingTipPlacementMode.RightBottom;

                case TeachingTipPlacementMode.LeftTop:
                    return TeachingTipPlacementMode.RightTop;

                case TeachingTipPlacementMode.TopLeft:
                    return TeachingTipPlacementMode.TopRight;

                case TeachingTipPlacementMode.TopRight:
                    return TeachingTipPlacementMode.TopLeft;

                case TeachingTipPlacementMode.RightTop:
                    return TeachingTipPlacementMode.LeftTop;

                case TeachingTipPlacementMode.RightBottom:
                    return TeachingTipPlacementMode.LeftBottom;

                case TeachingTipPlacementMode.BottomRight:
                    return TeachingTipPlacementMode.BottomLeft;

                case TeachingTipPlacementMode.BottomLeft:
                    return TeachingTipPlacementMode.BottomRight;

                default:
                    return pm;
            }
        }
    }

    private void OnTargetChanged()
    {
        if (_target != null)
        {
            _target.Loaded -= OnTargetLoaded;
            _target.EffectiveViewportChanged -= OnTargetLayoutUpdated;
        }

        _target = Target;

        if (_target != null)
        {
            _target.Loaded += OnTargetLoaded;
        }

        if (IsOpen)
        {
            if (_target != null)
            {
                _currentTargetBoundsInCoreWindowSpace = new Rect(_target.Bounds.Size)
                    .TransformToAABB(_target.TransformToVisual(VisualRoot).Value);

                SetViewportChangedEvent(_target);
            }
            PositionPopup();
        }
    }

    private void SetViewportChangedEvent(Control target)
    {
        // This seems to only be used in the TeachingTipTestHooks stuff from WinUI so this is always false
        // in normal operation I guess??

        //if (_tipFollowsTarget)
        //{
        //    if (target != null)
        //        target.EffectiveViewportChanged += OnTargetLayoutUpdated;

        //    _effectiveViewportChangedRevoker = new EffectiveViewportRevoker(this, OnTargetLayoutUpdated);
        //}
    }

    private void RevokeViewportChangedEvent()
    {
        if (_target != null)
            _target.EffectiveViewportChanged -= OnTargetLayoutUpdated;

        _effectiveViewportChangedRevoker?.Revoke();
    }

    private void XamlRootChanged(Rect rc)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _currentXamlRootSize = VisualRoot.ClientSize;
            RepositionPopup();
        }, DispatcherPriority.Render);
    }

    private void RepositionPopup()
    {
        if (IsOpen)
        {
            var newTargetBounds = _target != null ?
                new Rect(_target.Bounds.Size).TransformToAABB(_target.TransformToVisual(VisualRoot).Value) : Rect.Empty;

            var newCurrentBounds = new Rect(Bounds.Size).TransformToAABB(this.TransformToVisual(VisualRoot).Value);

            if (newTargetBounds != _currentTargetBoundsInCoreWindowSpace ||
                newCurrentBounds != _currentBoundsInCoreWindowSpace)
            {
                _currentBoundsInCoreWindowSpace = newCurrentBounds;
                _currentTargetBoundsInCoreWindowSpace = newTargetBounds;
                PositionPopup();
            }
        }
    }

    private void OnTargetLoaded(object sender, RoutedEventArgs args)
    {
        RepositionPopup();
    }

    private void OnTargetLayoutUpdated(object sender, EffectiveViewportChangedEventArgs e)
    {
        RepositionPopup();
    }

    private void CreateExpandAnimation()
    {
        // WinUI uses winrt::Window::Current().Compositor()
        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;

        if (compositor == null)
            return;

        _expandAnimation = compositor.CreateVector3KeyFrameAnimation();

        if (_tailOcclusionGrid != null)
        {
            _expandAnimation.SetScalarParameter("Width", (float)_tailOcclusionGrid.Bounds.Width);
            _expandAnimation.SetScalarParameter("Height", (float)_tailOcclusionGrid.Bounds.Height);
        }
        else
        {
            _expandAnimation.SetScalarParameter("Width", s_defaultTipHeightAndWidth);
            _expandAnimation.SetScalarParameter("Height", s_defaultTipHeightAndWidth);
        }
        
        _expandEasingFunction = new SplineEasing(0.1, 0.9, 0.2, 1);

        _expandAnimation.InsertExpressionKeyFrame(0.0f, "Vector3(Min(0.01, 20.0 / Width), Min(0.01, 20.0 / Height), 1.0)");
        (_expandAnimation as Vector3KeyFrameAnimation).InsertKeyFrame(1f, Vector3.One, _expandEasingFunction);
        _expandAnimation.Duration = _expandAnimationDuration;
        _expandAnimation.Target = s_ScaleTargetName;

        // TODO: This doesn't work because "Translation" is not a valid Target in Avalonia (yet)
        // Also probably not needed since this is likely related to shadows - which we don't have
        //_expandElevationAnimation = compositor.CreateVector3KeyFrameAnimation();
        //_expandElevationAnimation.InsertExpressionKeyFrame(1.0f, "Vector3(this.Target.Translation.X, this.Target.Translation.Y, contentElevation)", (Easing)_expandEasingFunction);
        //_expandElevationAnimation.SetScalarParameter("contentElevation", _contentElevation);
        //_expandElevationAnimation.Duration = _expandAnimationDuration;
        //_expandElevationAnimation.Target = s_translationTargetName;
    }

    private void CreateContractAnimation()
    {
        // WinUI uses winrt::Window::Current().Compositor()
        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;

        if (compositor == null)
            return;

        _contractEasingFunction = new SplineEasing(0.1, 0.9, 0.2, 1);

        _contractAnimation = compositor.CreateVector3KeyFrameAnimation();

        if (_tailOcclusionGrid != null)
        {
            _contractAnimation.SetScalarParameter("Width", (float)_tailOcclusionGrid.Bounds.Width);
            _contractAnimation.SetScalarParameter("Height", (float)_tailOcclusionGrid.Bounds.Height);
        }
        else
        {
            _contractAnimation.SetScalarParameter("Width", s_defaultTipHeightAndWidth);
            _contractAnimation.SetScalarParameter("Height", s_defaultTipHeightAndWidth);
        }

        (_contractAnimation as Vector3KeyFrameAnimation).InsertKeyFrame(0f, Vector3.One);
        _contractAnimation.InsertExpressionKeyFrame(1.0f, "Vector3(20.0 / Width, 20.0 / Height, 1.0)", (Easing)_contractEasingFunction);        
        _contractAnimation.Duration = _contractAnimationDuration;
        _contractAnimation.Target = s_ScaleTargetName;

        // TODO: This doesn't seem to work, the expression is throwing an error - it doesn't like the 'f' in 0.01f
        // But like ExpandAnimation, we don't need this
        //_contractElevationAnimation = compositor.CreateVector3KeyFrameAnimation();
        //_contractElevationAnimation.InsertExpressionKeyFrame(1.0f, "Vector3(this.Target.Translation.X, this.Target.Translation.Y, 0.01f)", (Easing)_contractEasingFunction);
        //_contractElevationAnimation.SetScalarParameter("contentElevation", _contentElevation);
        //_contractElevationAnimation.Duration = _expandAnimationDuration;
        //_contractElevationAnimation.Target = s_translationTargetName;
    }

    private void StartExpandToOpen()
    {
        if (_expandAnimation == null)
        {
            CreateExpandAnimation();
        }

        // TODO: We really need ScopedBatch animations to do this right

        // HACK: CenterPoint on CompositionVisual resets after animation probably because the element is in a popup
        // and is completely removed upon closing. So we call UpdateTail here to reset the CenterPoint so the opening
        // animation is correct if the TeachingTip is opened more than once. This does not happen in WinUI
        UpdateTail();

        if (_expandAnimation != null)
        {
            if (_tailOcclusionGrid != null)
            {
                ElementComposition.GetElementVisual(_tailOcclusionGrid)?.StartAnimationGroup(_expandAnimation);
                _isExpandAnimationPlaying = true;
            }
            //if (_tailEdgeBorder != null)
            //{
            //    ElementComposition.GetElementVisual(_tailEdgeBorder)?.StartAnimationGroup(_expandAnimation);
            //    _isExpandAnimationPlaying = true;
            //}
        }

        //if (_expandElevationAnimation != null)
        //{
        //    if (_contentRootGrid != null)
        //    {
        //        ElementComposition.GetElementVisual(_contentRootGrid)?.StartAnimationGroup(_expandElevationAnimation);
        //        _isExpandAnimationPlaying = true;
        //    }
        //}

        _scopedBatch.Completed += () =>
        {
            // Upon ScopedBatch::Completed
            _isExpandAnimationPlaying = false;
            if (!_isContractAnimationPlaying)
                SetIsIdle(true);
            _expandAnimation = null;
        };

        // Since we don't have ScopedBatch animation yet, we have to be hacky to await the
        // animation to finish before we continue
        if (_isExpandAnimationPlaying)
            _scopedBatch.Start(_expandAnimationDuration);
        
        // Under normal circumstances we would have launched an animation just now, if we did not then we should make sure that the idle state is correct
        if (!_isExpandAnimationPlaying && !_isContractAnimationPlaying)
        {
            SetIsIdle(true);
        }
    }

    private void StartContractToClose()
    {
        if (_contractAnimation == null)
        {
            CreateContractAnimation();
        }
        
        // TODO: Need ScopedBatch to do this right
        if (_contractAnimation != null)
        {
            if (_tailOcclusionGrid != null)
            {
                ElementComposition.GetElementVisual(_tailOcclusionGrid)?.StartAnimationGroup(_contractAnimation);
                _isContractAnimationPlaying = true;
            }
            //if (_tailEdgeBorder != null)
            //{
            //    ElementComposition.GetElementVisual(_tailEdgeBorder)?.StartAnimationGroup(_contractAnimation);
            //    _isContractAnimationPlaying = true;
            //}
        }
        //if (_contractElevationAnimation != null)
        //{
        //    if (_contentRootGrid != null)
        //    {
        //        ElementComposition.GetElementVisual(_contentRootGrid)?.StartAnimationGroup(_contractElevationAnimation);
        //        _isContractAnimationPlaying = true;
        //    }
        //}

        _scopedBatch.Completed += () =>
        {
            // Upon ScopedBatch::Completed
            _isContractAnimationPlaying = false;
            ClosePopup();
            if (!_isExpandAnimationPlaying)
                SetIsIdle(true);
            _contractAnimation = null;
        };

        // Since we don't have ScopedBatch animation yet, we have to be hacky to await the
        // animation to finish before we continue
        if (_isContractAnimationPlaying)
            _scopedBatch.Start(_contractAnimationDuration);
                
        // Under normal circumstances we would have launched an animation just now, if we did not then we should make sure that the idle state is correct
        if (!_isExpandAnimationPlaying && !_isContractAnimationPlaying)
        {
            SetIsIdle(true);
        }
    }

    private (TeachingTipPlacementMode, bool) DetermineEffectivePlacement()
    {
        // WinUI: Because we do not have access to APIs to give us details about multi monitor scenarios we do not have the ability to correctly
        // Place the tip in scenarios where we have an out of root bounds tip. Since this is the case we have decided to do no special
        // calculations and return the provided value or top if auto was set. This behavior can be removed via the
        // SetReturnTopForOutOfWindowBounds test hook.

        if (!ShouldConstrainToRootBounds && _returnTopForOutOfWindowPlacement)
        {
            var placement = GetFlowDirectionAdjustedPlacement(PreferredPlacement);
            if (placement == TeachingTipPlacementMode.Auto)
            {
                return (TeachingTipPlacementMode.Top, false);
            }

            return (placement, false);
        }

        if (IsOpen && _currentEffectiveTipPlacementMode != TeachingTipPlacementMode.Auto)
        {
            return (_currentEffectiveTipPlacementMode, false);
        }

        var (contentHeight, contentWidth) = _tailOcclusionGrid != null ?
            (_tailOcclusionGrid.Bounds.Height, _tailOcclusionGrid.Bounds.Width) : (0, 0);

        if (_target != null)
        {
            return DetermineEffectivePlacementTargeted(contentHeight, contentWidth);
        }
        else
        {
            return DetermineEffectivePlacementUntargeted(contentHeight, contentWidth);
        }
    }

    private (TeachingTipPlacementMode, bool) DetermineEffectivePlacementTargeted(double contentHeight, double contentWidth)
    {
        // These variables will track which positions the tip will fit in. They all start true and are
        // flipped to false when we find a display condition that is not met.
        // Not porting enum_array, instead indices of Span will match the TeachingTipPlacementMode enum
        Span<bool> availability = stackalloc[]
        {
            false, /*Auto*/
            true, /*Top*/
            true, /*Bottom*/
            true, /*Right*/
            true, /*Left*/
            true, /*TopLeft*/
            true, /*TopRight*/
            true, /*BottomLeft*/
            true, /*BottomRight*/
            true, /*LeftTop*/
            true, /*LeftButtom*/
            true, /*RightTop*/
            true, /*RightBottom*/
            true, /*Center*/
        };

        var tipHeight = contentHeight + TailShortSideLength();
        var tipWidth = contentWidth + TailShortSideLength();

        // We try to avoid having the tail touch the HeroContent so rule out positions where this would be required
        if (HeroContent != null)
        {
            if (_heroContentBorder != null)
            {
                if (_nonHeroContentRootGrid != null)
                {
                    if (_heroContentBorder.Bounds.Height > _nonHeroContentRootGrid.Bounds.Height - TailLongSideActualLength())
                    {
                        availability[(int)TeachingTipPlacementMode.Left] = false;
                        availability[(int)TeachingTipPlacementMode.Right] = false;
                    }
                }
            }

            switch (HeroContentPlacement)
            {
                case TeachingTipHeroContentPlacementMode.Bottom:
                    availability[(int)TeachingTipPlacementMode.Top] = false;
                    availability[(int)TeachingTipPlacementMode.TopRight] = false;
                    availability[(int)TeachingTipPlacementMode.TopLeft] = false;
                    availability[(int)TeachingTipPlacementMode.RightTop] = false;
                    availability[(int)TeachingTipPlacementMode.LeftTop] = false;
                    availability[(int)TeachingTipPlacementMode.Center] = false;
                    break;

                case TeachingTipHeroContentPlacementMode.Top:
                    availability[(int)TeachingTipPlacementMode.Bottom] = false;
                    availability[(int)TeachingTipPlacementMode.BottomLeft] = false;
                    availability[(int)TeachingTipPlacementMode.BottomRight] = false;
                    availability[(int)TeachingTipPlacementMode.RightBottom] = false;
                    availability[(int)TeachingTipPlacementMode.LeftBottom] = false;
                    break;
            }
        }

        // When ShouldConstrainToRootBounds is true clippedTargetBounds == availableBoundsAroundTarget
        // We have to separate them because there are checks which care about both.
        var (clippedTargetBounds, availableBoundsAroundTarget) = DetermineSpaceAroundTarget();

        // If the edge of the target isn't in the window.
        if (clippedTargetBounds.Left < 0)
        {
            availability[(int)TeachingTipPlacementMode.LeftBottom] = false;
            availability[(int)TeachingTipPlacementMode.Left] = false;
            availability[(int)TeachingTipPlacementMode.LeftTop] = false;
        }
        // If the right edge of the target isn't in the window.
        if (clippedTargetBounds.Right < 0)
        {
            availability[(int)TeachingTipPlacementMode.RightBottom] = false;
            availability[(int)TeachingTipPlacementMode.Right] = false;
            availability[(int)TeachingTipPlacementMode.RightTop] = false;
        }
        // If the top edge of the target isn't in the window.
        if (clippedTargetBounds.Top < 0)
        {
            availability[(int)TeachingTipPlacementMode.TopLeft] = false;
            availability[(int)TeachingTipPlacementMode.Top] = false;
            availability[(int)TeachingTipPlacementMode.TopRight] = false;
        }
        // If the bottom edge of the target isn't in the window
        if (clippedTargetBounds.Bottom < 0)
        {
            availability[(int)TeachingTipPlacementMode.BottomLeft] = false;
            availability[(int)TeachingTipPlacementMode.Bottom] = false;
            availability[(int)TeachingTipPlacementMode.BottomRight] = false;
        }

        // If the horizontal midpoint is out of the window.
        if (clippedTargetBounds.Left < -_currentTargetBoundsInCoreWindowSpace.Width / 2 ||
            clippedTargetBounds.Right < -_currentTargetBoundsInCoreWindowSpace.Width / 2)
        {
            availability[(int)TeachingTipPlacementMode.TopLeft] = false;
            availability[(int)TeachingTipPlacementMode.Top] = false;
            availability[(int)TeachingTipPlacementMode.TopRight] = false;
            availability[(int)TeachingTipPlacementMode.BottomLeft] = false;
            availability[(int)TeachingTipPlacementMode.Bottom] = false;
            availability[(int)TeachingTipPlacementMode.BottomRight] = false;
            availability[(int)TeachingTipPlacementMode.Center] = false;
        }

        // If the vertical midpoint is out of the window.
        if (clippedTargetBounds.Top < -_currentTargetBoundsInCoreWindowSpace.Height / 2 ||
            clippedTargetBounds.Bottom < -_currentTargetBoundsInCoreWindowSpace.Height / 2)
        {
            availability[(int)TeachingTipPlacementMode.LeftBottom] = false;
            availability[(int)TeachingTipPlacementMode.Left] = false;
            availability[(int)TeachingTipPlacementMode.LeftTop] = false;
            availability[(int)TeachingTipPlacementMode.RightBottom] = false;
            availability[(int)TeachingTipPlacementMode.Right] = false;
            availability[(int)TeachingTipPlacementMode.RightTop] = false;
            availability[(int)TeachingTipPlacementMode.Center] = false;
        }

        // If the tip is too tall to fit between the top of the target and the top edge of the window or screen.
        if (tipHeight > availableBoundsAroundTarget.Top)
        {
            availability[(int)TeachingTipPlacementMode.Top] = false;
            availability[(int)TeachingTipPlacementMode.TopRight] = false;
            availability[(int)TeachingTipPlacementMode.TopLeft] = false;
        }
        // If the total tip is too tall to fit between the center of the target and the top of the window.
        if (tipHeight > availableBoundsAroundTarget.Top + (_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
        {
            availability[(int)TeachingTipPlacementMode.Center] = false;
        }
        // If the tip is too tall to fit between the center of the target and the top edge of the window.
        if (contentHeight - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Top + (_currentTargetBoundsInCoreWindowSpace.Height / 2.0))
        {
            availability[(int)TeachingTipPlacementMode.RightTop] = false;
            availability[(int)TeachingTipPlacementMode.LeftTop] = false;
        }
        // If the tip is too tall to fit in the window when the tail is centered vertically on the target and the tip.
        if (contentHeight / 2.0f > availableBoundsAroundTarget.Top + (_currentTargetBoundsInCoreWindowSpace.Height / 2.0f) ||
            contentHeight / 2.0f > availableBoundsAroundTarget.Bottom + (_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
        {
            availability[(int)TeachingTipPlacementMode.Right] = false;
            availability[(int)TeachingTipPlacementMode.Left] = false;
        }
        // If the tip is too tall to fit between the center of the target and the bottom edge of the window.
        if (contentHeight - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Bottom + (_currentTargetBoundsInCoreWindowSpace.Height / 2.0))
        {
            availability[(int)TeachingTipPlacementMode.RightBottom] = false;
            availability[(int)TeachingTipPlacementMode.LeftBottom] = false;
        }
        // If the tip is too tall to fit between the bottom of the target and the bottom edge of the window.
        if (tipHeight > availableBoundsAroundTarget.Bottom)
        {
            availability[(int)TeachingTipPlacementMode.Bottom] = false;
            availability[(int)TeachingTipPlacementMode.BottomLeft] = false;
            availability[(int)TeachingTipPlacementMode.BottomRight] = false;
        }

        // If the tip is too wide to fit between the left edge of the target and the left edge of the window.
        if (tipWidth > availableBoundsAroundTarget.Left)
        {
            availability[(int)TeachingTipPlacementMode.Left] = false;
            availability[(int)TeachingTipPlacementMode.LeftTop] = false;
            availability[(int)TeachingTipPlacementMode.LeftBottom] = false;
        }
        // If the tip is too wide to fit between the center of the target and the left edge of the window.
        if (contentWidth - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Left + (_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
        {
            availability[(int)TeachingTipPlacementMode.TopLeft] = false;
            availability[(int)TeachingTipPlacementMode.BottomLeft] = false;
        }
        // If the tip is too wide to fit in the window when the tail is centered horizontally on the target and the tip.
        if (contentWidth / 2.0f > availableBoundsAroundTarget.Left + (_currentTargetBoundsInCoreWindowSpace.Width / 2.0f) ||
            contentWidth / 2.0f > availableBoundsAroundTarget.Right + (_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
        {
            availability[(int)TeachingTipPlacementMode.Top] = false;
            availability[(int)TeachingTipPlacementMode.Bottom] = false;
            availability[(int)TeachingTipPlacementMode.Center] = false;
        }
        // If the tip is too wide to fit between the center of the target and the right edge of the window.
        if (contentWidth - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Right + (_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
        {
            availability[(int)TeachingTipPlacementMode.TopRight] = false;
            availability[(int)TeachingTipPlacementMode.BottomRight] = false;
        }
        // If the tip is too wide to fit between the right edge of the target and the right edge of the window.
        if (tipWidth > availableBoundsAroundTarget.Right)
        {
            availability[(int)TeachingTipPlacementMode.Right] = false;
            availability[(int)TeachingTipPlacementMode.RightTop] = false;
            availability[(int)TeachingTipPlacementMode.RightBottom] = false;
        }

        var wantedDirection = GetFlowDirectionAdjustedPlacement(PreferredPlacement);
        Span<byte> priorities = stackalloc byte[13];
        GetPlacementFallbackOrder(wantedDirection, ref priorities);

        foreach (var mode in priorities)
        {
            if (availability[mode])
            {
                return ((TeachingTipPlacementMode)mode, false);
            }
        }

        // The teaching tip wont fit anywhere, set tipDoesNotFit to indicate that we should not open.
        return (TeachingTipPlacementMode.Top, true);
    }

    private (TeachingTipPlacementMode, bool) DetermineEffectivePlacementUntargeted(double contentHeight, double contentWidth)
    {
        var windowBounds = GetWindowBounds();
        if (!ShouldConstrainToRootBounds)
        {
            var screenBoundsInCoreWindowSpace = GetEffectiveScreenBoundsInCoreWindowSpace(windowBounds);
            if (screenBoundsInCoreWindowSpace.Height > contentHeight && screenBoundsInCoreWindowSpace.Width > contentWidth)
            {
                return (TeachingTipPlacementMode.Bottom, false);
            }
        }
        else
        {
            var windowBoundsInCoreWindowSpace = GetEffectiveWindowBoundsInCoreWindowSpace(windowBounds);
            if (windowBoundsInCoreWindowSpace.Height > contentHeight && windowBoundsInCoreWindowSpace.Width > contentWidth)
            {
                return (TeachingTipPlacementMode.Bottom, false);
            }
        }

        // The teaching tip doesn't fit in the window/screen set tipDoesNotFit to indicate that we should not open.
        return (TeachingTipPlacementMode.Top, true);
    }

    private (Thickness, Thickness) DetermineSpaceAroundTarget()
    {
        var shouldConstrainToRootBounds = ShouldConstrainToRootBounds;

        var windowBounds = GetWindowBounds();
        var (windowBoundsInCoreWindowSpace, screenBoundsInCoreWindowSpace) =
            (GetEffectiveWindowBoundsInCoreWindowSpace(windowBounds),
             GetEffectiveScreenBoundsInCoreWindowSpace(windowBounds));

        var windowSpaceAroundTarget = new Thickness(
            _currentTargetBoundsInCoreWindowSpace.X - /* 0 except with test window bounds */ windowBoundsInCoreWindowSpace.X,
            _currentTargetBoundsInCoreWindowSpace.Y - /* 0 except with test window bounds */ windowBoundsInCoreWindowSpace.Y,
            // Window.Right - Target.Right
            (windowBoundsInCoreWindowSpace.X + windowBoundsInCoreWindowSpace.Width) - (_currentTargetBoundsInCoreWindowSpace.X + _currentTargetBoundsInCoreWindowSpace.Width),
            // Screen.Right - Target.Right
            (windowBoundsInCoreWindowSpace.Y + windowBoundsInCoreWindowSpace.Height) - (_currentTargetBoundsInCoreWindowSpace.Y + _currentTargetBoundsInCoreWindowSpace.Height));

        Thickness screenSpaceAroundTarget;
        if (!shouldConstrainToRootBounds)
        {
            screenSpaceAroundTarget = new Thickness(
                // Target.Left - Screen.Left
                _currentTargetBoundsInCoreWindowSpace.X - screenBoundsInCoreWindowSpace.X,
                // Target.Top - Screen.Top
                _currentTargetBoundsInCoreWindowSpace.Y - screenBoundsInCoreWindowSpace.Y,
                // Screen.Right - Target.Right
                (screenBoundsInCoreWindowSpace.X + screenBoundsInCoreWindowSpace.Width) - (_currentTargetBoundsInCoreWindowSpace.X + _currentTargetBoundsInCoreWindowSpace.Width),
                // Screen.Bottom - Target.Bottom
                (screenBoundsInCoreWindowSpace.Y + screenBoundsInCoreWindowSpace.Height) - (_currentTargetBoundsInCoreWindowSpace.Y + _currentTargetBoundsInCoreWindowSpace.Height));

        }
        else
        {
            screenSpaceAroundTarget = windowSpaceAroundTarget;
        }

        return (windowSpaceAroundTarget, screenSpaceAroundTarget);
    }

    private Rect GetEffectiveWindowBoundsInCoreWindowSpace(Rect windowBounds)
    {
        return new Rect(windowBounds.Size);
    }

    private Rect GetEffectiveScreenBoundsInCoreWindowSpace(Rect windowBounds)
    {
        if (!ShouldConstrainToRootBounds)
        {
            // For Avalonia, screen only matters for windowed systems. Since WinUI doesn't have this concept
            // we'll return a normal rect like GetEffectiveWindowBoundsInCoreWindowSpace does
            if (VisualRoot is Window w)
            {
                var displayInfo = w.Screens.ScreenFromWindow(w.PlatformImpl);
                var scaleFactor = displayInfo.PixelDensity;

                return new Rect(-w.Position.X, -w.Position.Y,
                    displayInfo.Bounds.Height / scaleFactor,
                    displayInfo.Bounds.Width / scaleFactor);
            }
        }

        return new Rect(windowBounds.Size);
    }

    private Rect GetWindowBounds()
    {
        return new Rect(VisualRoot?.Bounds.Size ?? Size.Empty);
    }

    private void GetPlacementFallbackOrder(TeachingTipPlacementMode preferredPlacement,
        ref Span<byte> priorityList)
    {
        priorityList[0] = (byte)TeachingTipPlacementMode.Top;
        priorityList[1] = (byte)TeachingTipPlacementMode.Bottom;
        priorityList[2] = (byte)TeachingTipPlacementMode.Left;
        priorityList[3] = (byte)TeachingTipPlacementMode.Right;
        priorityList[4] = (byte)TeachingTipPlacementMode.TopLeft;
        priorityList[5] = (byte)TeachingTipPlacementMode.TopRight;
        priorityList[6] = (byte)TeachingTipPlacementMode.BottomLeft;
        priorityList[7] = (byte)TeachingTipPlacementMode.BottomRight;
        priorityList[8] = (byte)TeachingTipPlacementMode.LeftTop;
        priorityList[9] = (byte)TeachingTipPlacementMode.LeftBottom;
        priorityList[10] = (byte)TeachingTipPlacementMode.RightTop;
        priorityList[11] = (byte)TeachingTipPlacementMode.RightBottom;
        priorityList[12] = (byte)TeachingTipPlacementMode.Center;

        if (IsPlacementBottom(preferredPlacement))
        {
            // Swap to bottom > top
            (priorityList[0], priorityList[1]) = (priorityList[1], priorityList[0]);
            (priorityList[4], priorityList[6]) = (priorityList[6], priorityList[4]);
            (priorityList[5], priorityList[7]) = (priorityList[7], priorityList[5]);
        }
        else if (IsPlacementLeft(preferredPlacement))
        {
            // swap to lateral > vertical
            (priorityList[0], priorityList[2]) = (priorityList[2], priorityList[0]);
            (priorityList[1], priorityList[3]) = (priorityList[3], priorityList[1]);
            (priorityList[4], priorityList[8]) = (priorityList[8], priorityList[4]);
            (priorityList[5], priorityList[9]) = (priorityList[9], priorityList[5]);
            (priorityList[6], priorityList[10]) = (priorityList[10], priorityList[6]);
            (priorityList[7], priorityList[11]) = (priorityList[11], priorityList[7]);
        }
        else if (IsPlacementRight(preferredPlacement))
        {
            // swap to lateral > vertical
            (priorityList[0], priorityList[2]) = (priorityList[2], priorityList[0]);
            (priorityList[1], priorityList[3]) = (priorityList[3], priorityList[1]);
            (priorityList[4], priorityList[8]) = (priorityList[8], priorityList[4]);
            (priorityList[5], priorityList[9]) = (priorityList[9], priorityList[5]);
            (priorityList[6], priorityList[10]) = (priorityList[10], priorityList[6]);
            (priorityList[7], priorityList[11]) = (priorityList[11], priorityList[7]);

            // swap to right > left
            (priorityList[0], priorityList[1]) = (priorityList[1], priorityList[0]);
            (priorityList[4], priorityList[6]) = (priorityList[6], priorityList[4]);
            (priorityList[5], priorityList[7]) = (priorityList[7], priorityList[5]);
        }

        //Switch the preferred placement to first.
        int pivot = -1;
        for (int i = 0; i < priorityList.Length; i++)
        {
            if (priorityList[i] == (byte)preferredPlacement)
            {
                pivot = i;
                break;
            }
        }

        for (int i = pivot; i > 0; i--)
        {
            priorityList[i] = priorityList[i - 1];
        }
        priorityList[0] = (byte)preferredPlacement;
    }

    private void EstablishShadows()
    {
        // TODO: Don't really have a nice way to do shadows right now
    }

    private void TrySetCenterPoint(Control element, double x, double y)
    {
        if (element == null)
            return;

        // TODO:
        var visual = ElementComposition.GetElementVisual(element);
        if (visual != null)
            visual.CenterPoint = new Vector3((float)x, (float)y, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double TailLongSideActualLength() =>
        _tailPolygon != null ? Math.Max(_tailPolygon.Bounds.Height, _tailPolygon.Bounds.Width) : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double TailLongSideLength() =>
        TailLongSideActualLength() - (2 * s_tailOcclusionAmount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double TailShortSideLength() =>
        _tailPolygon != null ? Math.Min(_tailPolygon.Bounds.Height, _tailPolygon.Bounds.Width) : 0;

    private double MinimumTipEdgeToTailEdgeMargin()
    {
        if (_tailOcclusionGrid != null)
        {
            return _tailOcclusionGrid.ColumnDefinitions.Count > 1 ?
                _tailOcclusionGrid.ColumnDefinitions[1].ActualWidth + s_tailOcclusionAmount
                : 0;
        }

        return 0;
    }

    private double MinimumTipEdgeToTailCenter()
    {
        if (_tailOcclusionGrid != null && _tailPolygon != null)
        {
            if (_tailOcclusionGrid.ColumnDefinitions.Count > 1)
            {
                return _tailOcclusionGrid.ColumnDefinitions[0].ActualWidth +
                    _tailOcclusionGrid.ColumnDefinitions[1].ActualWidth +
                    (Math.Max(_tailPolygon.Bounds.Height, _tailPolygon.Bounds.Width) / 2);
            }
        }

        return 0;
    }


    private CornerRadius GetTeachingTipCornerRadius() => CornerRadius;

    private void SetIsIdle(bool idle) => _isIdle = idle;


    double TopLeftCornerRadius() => GetTeachingTipCornerRadius().TopLeft;

    double TopRightCornerRadius() => GetTeachingTipCornerRadius().TopRight;



    private IDisposable _contentSizeChangedRevoker;
    private IDisposable _acceleratorKeyActivatedRevoker;
    private EffectiveViewportRevoker _effectiveViewportChangedRevoker;
    private IDisposable _xamlRootChangedRevoker;

    private Border _container;
    private Popup _popup;
    private Popup _lightDismissIndicatorPopup;
    // [Unused]  private ContentControl _popupContentControl;

    private Control _rootElement;
    private Grid _tailOcclusionGrid;
    private Grid _contentRootGrid;
    private Grid _nonHeroContentRootGrid;
    private Border _heroContentBorder;
    private Button _actionButton;
    private Button _alternateCloseButton;
    private Button _closeButton;
    private Path _tailPolygon;
    // [Unused] private Grid _tailEdgeBorder;
    // [Unused] private Control _titleTextBlock;
    // [Unused] private Control _subTitleTextBlock;

    private IInputElement _previouslyFocusedElement;

    private KeyFrameAnimation _expandAnimation;
    private KeyFrameAnimation _contractAnimation;
    // [Unused] private KeyFrameAnimation _expandElevationAnimation;
    // [Unused] private KeyFrameAnimation _contractElevationAnimation;
    private IEasing _expandEasingFunction;
    private IEasing _contractEasingFunction;
    private readonly ScopedBatchHelper _scopedBatch = new ScopedBatchHelper();

    private TeachingTipPlacementMode _currentEffectiveTipPlacementMode;
    private TeachingTipPlacementMode _currentEffectiveTailPlacementMode;
    private TeachingTipHeroContentPlacementMode _currentHeroContentEffectivePlacementMode;

    private Rect _currentBoundsInCoreWindowSpace;
    private Rect _currentTargetBoundsInCoreWindowSpace;

    private Size _currentXamlRootSize;

    private bool _ignoreNextIsOpenChanged;
    private bool _isTemplateApplied;
    private bool _createNewPopupOnOpen;

    private bool _isExpandAnimationPlaying;
    private bool _isContractAnimationPlaying;

    // [Unused] private bool _hasF6BeenInvoked;

    // [Unused] private bool _useTextWindowBounds;
    // [Unused] private Rect _testWindowBoundsInCoreWindowSpace;
    // [Unused] private bool _useTestScreenBounds;
    // [Unused] private Rect _testScreenBoundsInCoreWindowSpace;

    // [Unused] private bool _tipShouldHaveShadow = true;

    // [Unused] private bool _tipFollowsTarget;
    private bool _returnTopForOutOfWindowPlacement = true;

    // [Unused] private float _contentElevation = 32f;
    // [Unused] private float _tailElevation = 0f;
    // [Unused] private bool _tailShadowTargetsShadowTarget;

    private TimeSpan _expandAnimationDuration = TimeSpan.FromMilliseconds(300);
    private TimeSpan _contractAnimationDuration = TimeSpan.FromMilliseconds(200);

    private TeachingTipCloseReason _lastCloseReason = TeachingTipCloseReason.Programmatic;

    private bool _isIdle = true;
    private Control _target;

    // Helper functions
    static bool IsPlacementTop(TeachingTipPlacementMode p) =>
        p == TeachingTipPlacementMode.Top ||
        p == TeachingTipPlacementMode.TopLeft ||
        p == TeachingTipPlacementMode.TopRight;

    static bool IsPlacementBottom(TeachingTipPlacementMode p) =>
        p == TeachingTipPlacementMode.Bottom ||
        p == TeachingTipPlacementMode.BottomLeft ||
        p == TeachingTipPlacementMode.BottomRight;

    static bool IsPlacementLeft(TeachingTipPlacementMode p) =>
        p == TeachingTipPlacementMode.Left ||
        p == TeachingTipPlacementMode.TopLeft ||
        p == TeachingTipPlacementMode.TopRight;

    static bool IsPlacementRight(TeachingTipPlacementMode p) =>
        p == TeachingTipPlacementMode.Right ||
        p == TeachingTipPlacementMode.RightTop ||
        p == TeachingTipPlacementMode.RightBottom;

    // These values are shifted by one because this is the 1px highlight that sits adjacent to the tip border.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness BottomPlacementTopRightHighlightMargin(double width, double height) =>
        new Thickness(width / 2 + (TailShortSideLength() - 1f), 0, TopRightCornerRadius() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness BottomRightPlacementTopRightHighlightMargin(double width, double height) =>
        new Thickness(MinimumTipEdgeToTailEdgeMargin() + (TailLongSideLength() - 1f), 0, TopRightCornerRadius() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness BottomLeftPlacementTopRightHighlightMargin(double width, double height) =>
        new Thickness(width - (MinimumTipEdgeToTailEdgeMargin() + 1f), 0, TopRightCornerRadius() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness OtherPlacementTopRightHighlightMargin(double width, double height) => new Thickness();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness BottomPlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1, 0, (width / 2) + (TailShortSideLength() - 1f), 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness BottomRightPlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 0, width - (MinimumTipEdgeToTailEdgeMargin() + 1f), 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness BottomLeftPlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 0, MinimumTipEdgeToTailEdgeMargin() + TailLongSideLength() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness TopEdgePlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 1, TopRightCornerRadius() - 1f, 0);

    // Shifted by one since the tail edge's border is not accounted for automatically.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness LeftEdgePlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 1, TopRightCornerRadius() - 2f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Thickness RightEdgePlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 2f, 1, TopRightCornerRadius() - 1f, 0);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double UntargetedTipFarPlacementOffset(double farWindowCoordinateInCoreWindowSpace, double tipSize, double offset) =>
        farWindowCoordinateInCoreWindowSpace - (tipSize + s_untargetedTipWindowEdgeMargin + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double UntargetedTipCenterPlacementOffset(double nearWindowCoordinateInCoreWindowSpace, double farWindowCoordinateInCoreWindowSpace,
        double tipSize, double nearOffset, double farOffset) =>
        ((nearWindowCoordinateInCoreWindowSpace + farWindowCoordinateInCoreWindowSpace) / 2) - (tipSize / 2) + nearOffset - farOffset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double UntargetedTipNearPlacementOffset(double nearWindowCoordinateInCoreWindowSpace, double offset) =>
        s_untargetedTipWindowEdgeMargin + nearWindowCoordinateInCoreWindowSpace + offset;


    private static readonly string s_ScaleTargetName = "Scale";
   // [Unused] private static readonly string s_translationTargetName = "Translation";

    // [Unused] private static readonly string s_teachingTipHighlightBrushName = "TeachingTipTopHighlightBrush";

    //It is possible this should be exposed as a property, but you can adjust what it does with margin.
    private static readonly float s_untargetedTipWindowEdgeMargin = 24;
    private static readonly float s_defaultTipHeightAndWidth = 320;

    //Ideally this would be computed from layout but it is difficult to do.
    private static readonly float s_tailOcclusionAmount = 2;

    private static readonly string s_TitleTextVisibleStateName = ":showTitle";
    private static readonly string s_SubTitleTextVisibleStateName = ":showSubtitle";

    private static readonly string SR_TeachingTipAlternateCloseButtonName = "TeachingTipAlternateCloseButtonName";
    private static readonly string SR_TeachingTipAlternateCloseButtonTooltip = "TeachingTipAlternateCloseButtonTooltip";

    private const string s_pcTop = ":top";
    private const string s_pcLeft = ":left";
    private const string s_pcRight = ":right";
    private const string s_pcBottom = ":bottom";
    private const string s_pcTopLeft = ":topLeft";
    private const string s_pcTopRight = ":topRight";
    private const string s_pcBottomLeft = ":bottomLeft";
    private const string s_pcBottomRight = ":bottomRight";
    private const string s_pcLeftTop = ":leftTop";
    private const string s_pcRightTop = ":rightTop";
    private const string s_pcLeftBottom = ":leftBottom";
    private const string s_pcRightBottom = ":rightBottom";
    private const string s_pcCenter = ":center";

    private class ScopedBatchHelper
    {
        public Action Completed { get; set; }

        public void Start(TimeSpan duration)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer(duration, DispatcherPriority.Background, Tick);
            }

            _timer.Start();
        }

        private void Tick(object sender, EventArgs args)
        {
            _timer.Stop();
            Completed?.Invoke();
            Completed = null;
        }

        private DispatcherTimer _timer;
    }
}

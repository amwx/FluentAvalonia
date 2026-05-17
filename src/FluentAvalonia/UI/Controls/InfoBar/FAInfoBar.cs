using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

// InfoBar up to date with WinUI as of 5/9/26

public partial class FAInfoBar : ContentControl
{    
    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _appliedTemplate = false;
        _closeButton?.Click -= OnCloseButtonClick;

        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>(s_tpCloseButton);
        if (_closeButton != null)
        {
            _closeButton.Click += OnCloseButtonClick;

            ToolTip.SetTip(_closeButton, FALocalizationHelper.Instance.GetLocalizedStringResource(SR_InfoBarCloseButtonTooltip));

            if (AutomationProperties.GetName(_closeButton) == null)
            {
                var closeButtonName = FALocalizationHelper.Instance.GetLocalizedStringResource(SR_InfoBarCloseButtonName);
                AutomationProperties.SetName(_closeButton, closeButtonName);
            }
        }

        var iconTextBlock = e.NameScope.Find<FAFontIcon>(s_tpStandardIcon);
        if (iconTextBlock != null)
        {
            _standardIcon = iconTextBlock;
            AutomationProperties.SetName(iconTextBlock,
                FALocalizationHelper.Instance.GetLocalizedStringResource(GetIconSeverityLevelResourceName(Severity)));
        }

        _appliedTemplate = true;

        UpdateVisibility(_notifyOpen, true);
        _notifyOpen = false;

        UpdateSeverity();
        UpdateIcon();
        UpdateIconVisibility();
        UpdateCloseButton();
        UpdateForeground();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOpenProperty)
        {
            if (change.GetNewValue<bool>())
            {
                _lastCloseReason = FAInfoBarCloseReason.Programmatic;
                UpdateVisibility();

                RaiseOpenedEvent();
            }
            else
            {
                RaiseClosingEvent();
            }
        }
        else if (change.Property == SeverityProperty)
        {
            UpdateSeverity();
        }
        else if (change.Property == IconSourceProperty)
        {
            UpdateIcon();
            UpdateIconVisibility();
        }
        else if (change.Property == IsIconVisibleProperty)
        {
            UpdateIconVisibility();
        }
        else if (change.Property == IsClosableProperty)
        {
            UpdateCloseButton();
        }
        else if (change.Property == TextElement.ForegroundProperty)
        {
            UpdateForeground();
        }
        else if (change.Property == TitleProperty)
        {
            UpdateContentPosition();
        }
        else if (change.Property == MessageProperty)
        {
            UpdateContentPosition();
        }
        else if (change.Property == ActionButtonProperty)
        {
            UpdateContentPosition();
        }
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name == "ContentPresenter")
            return true;

        return base.RegisterContentPresenter(presenter);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new FAInfoBarAutomationPeer(this);
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        CloseButtonClick?.Invoke(this, EventArgs.Empty);
        _lastCloseReason = FAInfoBarCloseReason.CloseButton;
        IsOpen = false;
    }

    private void RaiseClosingEvent()
    {
        var args = new FAInfoBarClosingEventArgs(_lastCloseReason);

        Closing?.Invoke(this, args);

        if (!args.Cancel)
        {
            UpdateVisibility();
            RaiseClosedEvent();
        }
        else
        {
            // The developer has changed the Cancel property to true,
            // so we need to revert the IsOpen property to true.
            IsOpen = true;
        }
    }

    private void RaiseClosedEvent()
    {
        var args = new FAInfoBarClosedEventArgs(_lastCloseReason);
        Closed?.Invoke(this, args);
    }

    private void RaiseOpenedEvent()
    {
        Opened?.Invoke(this, new FAInfoBarOpenedEventArgs());
    }

    private void UpdateVisibility(bool notify = true, bool force = true)
    {
        if (!_appliedTemplate)
        {
            _notifyOpen = true;
        }
        else
        {
            var peer = ControlAutomationPeer.FromElement(this);
            if (force || IsOpen != _isVisible)
            {
                if (IsOpen)
                {
                    _isVisible = true;
                    PseudoClasses.Set(FASharedPseudoclasses.s_pcHidden, false);

                    if (notify && peer is FAInfoBarAutomationPeer p)
                    {
                        var local = FALocalizationHelper.Instance;
                        var notificationString = $"{local.GetLocalizedStringResource(SR_InfoBarOpenedNotification)}" +
                            $"{local.GetLocalizedStringResource(GetIconSeverityLevelResourceName(Severity))}" +
                            $"{Title} {Message}";

                        p.RaiseOpenedEvent(Severity, notificationString);
                    }

                    AutomationProperties.SetAccessibilityView(this, AccessibilityView.Control);
                }
                else
                {
                    if (notify && peer is FAInfoBarAutomationPeer p)
                    {
                        var notificationString = FALocalizationHelper.Instance
                            .GetLocalizedStringResource(SR_InfoBarClosedNotification);

                        p.RaiseClosedEvent(Severity, notificationString);
                    }

                    _isVisible = false;
                    PseudoClasses.Set(FASharedPseudoclasses.s_pcHidden, true);
                    AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
                }
            }
        }
    }

    private void UpdateSeverity()
    {
        if (!_appliedTemplate)
            return; //Template not applied yet

        var severity = Severity;

        switch (severity)
        {
            case FAInfoBarSeverity.Success:
                PseudoClasses.Set(s_pcSuccess, true);
                PseudoClasses.Set(s_pcWarning, false);
                PseudoClasses.Set(s_pcError, false);
                PseudoClasses.Set(s_pcInformational, false);
                break;

            case FAInfoBarSeverity.Warning:
                PseudoClasses.Set(s_pcSuccess, false);
                PseudoClasses.Set(s_pcWarning, true);
                PseudoClasses.Set(s_pcError, false);
                PseudoClasses.Set(s_pcInformational, false);
                break;

            case FAInfoBarSeverity.Error:
                PseudoClasses.Set(s_pcSuccess, false);
                PseudoClasses.Set(s_pcWarning, false);
                PseudoClasses.Set(s_pcError, true);
                PseudoClasses.Set(s_pcInformational, false);
                break;

            default: // default to informational
                PseudoClasses.Set(s_pcSuccess, false);
                PseudoClasses.Set(s_pcWarning, false);
                PseudoClasses.Set(s_pcError, false);
                PseudoClasses.Set(s_pcInformational, true);
                break;
        }

        if (_standardIcon != null)
        {
            AutomationProperties.SetName(_standardIcon,
                FALocalizationHelper.Instance.GetLocalizedStringResource(GetIconSeverityLevelResourceName(severity)));
        }
    }

    private void UpdateIcon()
    {
        // Skip this logic - used an IconSourceElement in the template instead
        // which automatically handles IconSource -> IconElement for us
    }

    private void UpdateIconVisibility()
    {
        if (!IsIconVisible)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, false);
            PseudoClasses.Set(s_pcStandardIcon, false);
        }
        else
        {
            bool hasUserIcon = IconSource != null;
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, hasUserIcon);
            PseudoClasses.Set(s_pcStandardIcon, !hasUserIcon);
        }
    }

    private void UpdateCloseButton()
    {
        PseudoClasses.Set(s_pcCloseHidden, !IsClosable);
    }

    private void UpdateForeground()
    {
        PseudoClasses.Set(s_pcForegroundSet, this.GetValue(TextElement.ForegroundProperty) != AvaloniaProperty.UnsetValue);
    }

    private void UpdateContentPosition()
    {
        var title = Title;
        var message = Message;
        var ab = ActionButton;
        PseudoClasses.Set(s_pcNoBannerContent, string.IsNullOrEmpty(title) &&
            string.IsNullOrEmpty(message) && ab == null);
    }

    private static string GetSeverityLevelResourceName(FAInfoBarSeverity severity)
    {
        return severity switch
        {
            FAInfoBarSeverity.Success => SR_InfoBarSeveritySuccessName,
            FAInfoBarSeverity.Warning => SR_InfoBarSeverityWarningName,
            FAInfoBarSeverity.Error => SR_InfoBarSeverityErrorName,
            _ => SR_InfoBarSeverityInformationalName,
        };
    }

    private static string GetIconSeverityLevelResourceName(FAInfoBarSeverity severity)
    {
        return severity switch
        {
            FAInfoBarSeverity.Success => SR_InfoBarIconSeveritySuccessName,
            FAInfoBarSeverity.Warning => SR_InfoBarIconSeverityWarningName,
            FAInfoBarSeverity.Error => SR_InfoBarIconSeverityErrorName,
            _ => SR_InfoBarIconSeverityInformationalName,
        };
    }

    private Button _closeButton;
    private FAFontIcon _standardIcon;

    private bool _appliedTemplate;
    private bool _notifyOpen;
    private bool _isVisible;

    private FAInfoBarCloseReason _lastCloseReason;
}

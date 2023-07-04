using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public partial class InfoBar : ContentControl
{    
    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _appliedTemplate = false;
        if (_closeButton != null)
        {
            _closeButton.Click -= OnCloseButtonClick;
        }

        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>(s_tpCloseButton);
        if (_closeButton != null)
        {
            _closeButton.Click += OnCloseButtonClick;

            ToolTip.SetTip(_closeButton, FALocalizationHelper.Instance.GetLocalizedStringResource(SR_InfoBarCloseButtonTooltip));
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
                _lastCloseReason = InfoBarCloseReason.Programmatic;
                UpdateVisibility();
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
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name == "ContentPresenter")
            return true;

        return base.RegisterContentPresenter(presenter);
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        CloseButtonClick?.Invoke(this, EventArgs.Empty);
        _lastCloseReason = InfoBarCloseReason.CloseButton;
        IsOpen = false;
    }

    private void RaiseClosingEvent()
    {
        var args = new InfoBarClosingEventArgs(_lastCloseReason);

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
        var args = new InfoBarClosedEventArgs(_lastCloseReason);
        Closed?.Invoke(this, args);
    }

    private void UpdateVisibility(bool notify = true, bool force = true)
    {
        if (!_appliedTemplate)
        {
            _notifyOpen = true;
        }
        else
        {
            if (force || IsOpen != _isVisible)
            {
                if (IsOpen)
                {
                    _isVisible = true;
                    PseudoClasses.Set(SharedPseudoclasses.s_pcHidden, false);
                }
                else
                {
                    _isVisible = false;
                    PseudoClasses.Set(SharedPseudoclasses.s_pcHidden, true);
                }
            }
        }
    }

    private void UpdateSeverity()
    {
        if (!_appliedTemplate)
            return; //Template not applied yet

        switch (Severity)
        {
            case InfoBarSeverity.Success:
                PseudoClasses.Set(s_pcSuccess, true);
                PseudoClasses.Set(s_pcWarning, false);
                PseudoClasses.Set(s_pcError, false);
                PseudoClasses.Set(s_pcInformational, false);
                break;

            case InfoBarSeverity.Warning:
                PseudoClasses.Set(s_pcSuccess, false);
                PseudoClasses.Set(s_pcWarning, true);
                PseudoClasses.Set(s_pcError, false);
                PseudoClasses.Set(s_pcInformational, false);
                break;

            case InfoBarSeverity.Error:
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
            PseudoClasses.Set(SharedPseudoclasses.s_pcIcon, false);
            PseudoClasses.Set(s_pcStandardIcon, false);
        }
        else
        {
            bool hasUserIcon = IconSource != null;
            PseudoClasses.Set(SharedPseudoclasses.s_pcIcon, hasUserIcon);
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

    private Button _closeButton;

    private bool _appliedTemplate;
    private bool _notifyOpen;
    private bool _isVisible;

    private InfoBarCloseReason _lastCloseReason;
}

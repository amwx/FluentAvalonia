using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;

namespace FluentAvalonia.UI.Controls
{
	public partial class InfoBar : ContentControl
	{
		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			_appliedTemplate = false;
			if (_closeButton != null)
			{
				_closeButton.Click -= OnCloseButtonClick;
			}

			base.OnApplyTemplate(e);

			_closeButton = e.NameScope.Find<Button>("CloseButton");
			if (_closeButton != null)
			{
				_closeButton.Click += OnCloseButtonClick;

				ToolTip.SetTip(_closeButton, "Close");
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
			else if (change.Property == TextBlock.ForegroundProperty)
			{
				UpdateForeground();
			}
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
						PseudoClasses.Set(":hidden", false);
					}
					else
					{
						_isVisible = false;
						PseudoClasses.Set(":hidden", true);
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
					PseudoClasses.Set(":success", true);
					PseudoClasses.Set(":warning", false);
					PseudoClasses.Set(":error", false);
					PseudoClasses.Set(":informational", false);
					break;

				case InfoBarSeverity.Warning:
					PseudoClasses.Set(":success", false);
					PseudoClasses.Set(":warning", true);
					PseudoClasses.Set(":error", false);
					PseudoClasses.Set(":informational", false);
					break;

				case InfoBarSeverity.Error:
					PseudoClasses.Set(":success", false);
					PseudoClasses.Set(":warning", false);
					PseudoClasses.Set(":error", true);
					PseudoClasses.Set(":informational", false);
					break;

				default: // default to informational
					PseudoClasses.Set(":success", false);
					PseudoClasses.Set(":warning", false);
					PseudoClasses.Set(":error", false);
					PseudoClasses.Set(":informational", true);
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
				PseudoClasses.Set(":icon", false);
				PseudoClasses.Set(":standardIcon", false);
			}
			else
			{
				bool hasUserIcon = IconSource != null;
				PseudoClasses.Set(":icon", hasUserIcon);
				PseudoClasses.Set(":standardIcon", !hasUserIcon);
			}			
		}

		private void UpdateCloseButton()
		{
			PseudoClasses.Set(":closehidden", !IsClosable);
		}

		private void UpdateForeground()
		{
			PseudoClasses.Set(":foregroundset", this.GetValue(TextBlock.ForegroundProperty) != AvaloniaProperty.UnsetValue);
		}

		private Button _closeButton;

		private bool _appliedTemplate;
		private bool _notifyOpen;
		private bool _isVisible;

		private InfoBarCloseReason _lastCloseReason;
	}
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
	public class InfoBar : ContentControl
	{
		public static readonly DirectProperty<InfoBar, bool> IsOpenProperty =
			AvaloniaProperty.RegisterDirect<InfoBar, bool>(nameof(IsOpen), 
				x => x.IsOpen, (x, v) => x.IsOpen = v);

		public static readonly StyledProperty<string> TitleProperty =
			AvaloniaProperty.Register<InfoBar, string>(nameof(Title));

		public static readonly StyledProperty<string> MessageProperty =
			AvaloniaProperty.Register<InfoBar, string>(nameof(Message));

		public static readonly StyledProperty<InfoBarSeverity> SeverityProperty =
			AvaloniaProperty.Register<InfoBar, InfoBarSeverity>(nameof(Severity));

		public static readonly StyledProperty<IconSource> IconSourceProperty =
			AvaloniaProperty.Register<InfoBar, IconSource>(nameof(IconSource));

		public static readonly StyledProperty<bool> IsIconVisibleProperty =
			AvaloniaProperty.Register<InfoBar, bool>(nameof(IsIconVisible), true);

		public static readonly StyledProperty<bool> IsClosableProperty =
			AvaloniaProperty.Register<InfoBar, bool>(nameof(IsClosable), true);

		public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
			AvaloniaProperty.Register<InfoBar, ICommand>(nameof(CloseButtonCommand));

		public static readonly DirectProperty<InfoBar, object> CloseButtonCommandParameterProperty =
			AvaloniaProperty.RegisterDirect<InfoBar, object>(nameof(CloseButtonCommandParameter), 
				x => x.CloseButtonCommandParameter, (x, v) => x.CloseButtonCommandParameter = v);

		public static readonly StyledProperty<IControl> ActionButtonProperty =
			AvaloniaProperty.Register<InfoBar, IControl>(nameof(ActionButton));

		public bool IsOpen
		{
			get => _isOpen;
			set => SetAndRaise(IsOpenProperty, ref _isOpen, value);
		}

		public string Title
		{
			get => GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public string Message
		{
			get => GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}

		public InfoBarSeverity Severity
		{
			get => GetValue(SeverityProperty);
			set => SetValue(SeverityProperty, value);
		}

		public IconSource IconSource
		{
			get => GetValue(IconSourceProperty);
			set => SetValue(IconSourceProperty, value);
		}

		public bool IsIconVisible
		{
			get => GetValue(IsIconVisibleProperty);
			set => SetValue(IsIconVisibleProperty, value);
		}

		public bool IsClosable
		{
			get => GetValue(IsClosableProperty);
			set => SetValue(IsClosableProperty, value);
		}

		public ICommand CloseButtonCommand
		{
			get => GetValue(CloseButtonCommandProperty);
			set => SetValue(CloseButtonCommandProperty, value);
		}

		public object CloseButtonCommandParameter
		{
			get => _closeButtonCommandParameter;
			set => SetAndRaise(CloseButtonCommandParameterProperty, ref _closeButtonCommandParameter, value);
		}

		public IControl ActionButton
		{
			get => GetValue(ActionButtonProperty);
			set => SetValue(ActionButtonProperty, value);
		}

		public event TypedEventHandler<InfoBar, object> CloseButtonClick;
		public event TypedEventHandler<InfoBar, InfoBarClosingEventArgs> Closing;
		public event TypedEventHandler<InfoBar, InfoBarClosedEventArgs> Closed;

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

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IsOpenProperty)
			{
				if (change.NewValue.GetValueOrDefault<bool>())
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
			CloseButtonClick?.Invoke(this, null);
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
			// WinUI sets an IconElement in TemplateSetings
			// Why tho...Just use an IconSourceElement and
			// set the icon there, why must they make everything
			// so complicated
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

		private bool _isOpen;
		private object _closeButtonCommandParameter;
		private InfoBarCloseReason _lastCloseReason;
	}
}

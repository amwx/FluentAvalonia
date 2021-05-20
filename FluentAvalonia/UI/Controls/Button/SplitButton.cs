using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FluentAvalonia.Core;
using System;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
	public class SplitButton : ContentControl
	{

		#region AvaloniaProperties

		public static readonly DirectProperty<SplitButton, ICommand> CommandProperty =
			Button.CommandProperty.AddOwner<SplitButton>(x => x.Command, (x, v) => x.Command = v);

		public static readonly StyledProperty<object> CommandParameterProperty =
			Button.CommandParameterProperty.AddOwner<SplitButton>();

		public static readonly DirectProperty<SplitButton, FlyoutBase> FlyoutProperty =
			AvaloniaProperty.RegisterDirect<SplitButton, FlyoutBase>("Flyout",
				x => x.Flyout, (x, v) => x.Flyout = v);

		#endregion

		#region CLR Properties

		public ICommand Command
		{
			get => _command;
			set => SetAndRaise(CommandProperty, ref _command, value);
		}

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public FlyoutBase Flyout
		{
			get => _flyout;
			set => SetAndRaise(FlyoutProperty, ref _flyout, value);
		}

		internal virtual bool InternalIsChecked => false;

		#endregion

		#region Events

		public event TypedEventHandler<SplitButton, SplitButtonClickEventArgs> Click;

		#endregion

		#region Override Methods

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			UnregisterEvents();

			base.OnApplyTemplate(e);

			_primaryButton = e.NameScope.Find<Button>("PrimaryButton");
			_secondaryButton = e.NameScope.Find<Button>("SecondaryButton");

			if (_primaryButton != null)
			{
				_primaryButton.Click += OnClickPrimary;

				_primaryDisposable = new CompositeDisposable(
					_primaryButton.GetPropertyChangedObservable(Button.IsPressedProperty).Subscribe(OnVisualPropertyChanged),
					_primaryButton.GetPropertyChangedObservable(Button.IsPointerOverProperty).Subscribe(OnVisualPropertyChanged));

				// Register for pointer events so we can keep track of last used pointer type
				_primaryButton.PointerEnter += OnPointerEvent;
				_primaryButton.PointerLeave += OnPointerEvent;
				_primaryButton.PointerPressed += OnPointerEvent;
				_primaryButton.PointerReleased += OnPointerEvent;
				//Dont have PointerCanceled
				_primaryButton.PointerCaptureLost += OnPointerLostEvent;
			}

			if (_secondaryButton != null)
			{
				_secondaryButton.Click += OnClickSecondary;

				_secondaryDisposable = new CompositeDisposable(
					_secondaryButton.GetPropertyChangedObservable(Button.IsPressedProperty).Subscribe(OnVisualPropertyChanged),
					_secondaryButton.GetPropertyChangedObservable(Button.IsPointerOverProperty).Subscribe(OnVisualPropertyChanged));

				_secondaryButton.PointerEnter += OnPointerEvent;
				_secondaryButton.PointerLeave += OnPointerEvent;
				_secondaryButton.PointerPressed += OnPointerEvent;
				_secondaryButton.PointerReleased += OnPointerEvent;
				//Dont have PointerCanceled
				_secondaryButton.PointerCaptureLost += OnPointerLostEvent;
			}

			RegisterFlyoutEvents();

			UpdateVisualStates();

			_hasLoaded = true;
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == FlyoutProperty)
			{
				//Must unregister events here while we have ref to old flyout
				if (change.OldValue.GetValueOrDefault() is FlyoutBase f)
				{
					f.Opened -= OnFlyoutOpened;
					f.Closed -= OnFlyoutClosed;
					_flyoutPlacementDisposable?.Dispose();
					_flyoutPlacementDisposable = null;
				}

				OnFlyoutChanged();
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Space:
				case Key.Enter:
					_isKeyDown = true;
					UpdateVisualStates();
					break;
			}
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Space:
				case Key.Enter:
					_isKeyDown = false;
					UpdateVisualStates();

					if (IsEnabled)
					{
						OnClickPrimary(null, null);
						e.Handled = true;
					}
					break;

				case Key.Down:
					//WinUI: VirtualKey::Menu + Down = open flyout
					//Will just use down key

					if (IsEnabled)
					{
						OpenFlyout();
						e.Handled = true;
					}
					break;

				case Key.F4:
					OpenFlyout();
					e.Handled = true;
					break;
			}
			base.OnKeyUp(e);
		}

		#endregion

		#region Public Methods

		private void OpenFlyout()
		{
			if (Flyout != null)
			{
				Flyout.ShowAt(this);
			}
		}

		private void CloseFlyout()
		{
			if (Flyout != null)
			{
				Flyout.Hide();
			}
		}

		#endregion

		#region Private Methods

		private void UnregisterEvents()
		{
			if (_primaryButton == null || _secondaryButton == null)
				return;

			_primaryButton.Click -= OnClickPrimary;
			_secondaryButton.Click -= OnClickSecondary;

			_primaryButton.PointerEnter -= OnPointerEvent;
			_primaryButton.PointerLeave -= OnPointerEvent;
			_primaryButton.PointerPressed -= OnPointerEvent;
			_primaryButton.PointerReleased -= OnPointerEvent;
			_primaryButton.PointerCaptureLost -= OnPointerLostEvent;
			_primaryDisposable.Dispose();
			_primaryDisposable = null;

			_secondaryButton.PointerEnter -= OnPointerEvent;
			_secondaryButton.PointerLeave -= OnPointerEvent;
			_secondaryButton.PointerPressed -= OnPointerEvent;
			_secondaryButton.PointerReleased -= OnPointerEvent;
			_secondaryButton.PointerCaptureLost -= OnPointerLostEvent;
			_secondaryDisposable.Dispose();
			_secondaryDisposable = null;
		}

		protected virtual void OnClickPrimary(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var ea = new SplitButtonClickEventArgs();
			Click?.Invoke(this, ea);
		}

		protected virtual void OnClickSecondary(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			OpenFlyout();
		}

		private void OnPointerEvent(object sender, PointerEventArgs e)
		{
			if (_lastPointerDeviceType != e.Pointer.Type)
			{
				_lastPointerDeviceType = e.Pointer.Type;
				UpdateVisualStates();
			}
		}

		//Separate event for different args in Avalonia
		private void OnPointerLostEvent(object sender, PointerCaptureLostEventArgs e)
		{
			if (_lastPointerDeviceType != e.Pointer.Type)
			{
				_lastPointerDeviceType = e.Pointer.Type;
				UpdateVisualStates();
			}
		}

		private void OnFlyoutChanged()
		{
			RegisterFlyoutEvents();
			UpdateVisualStates();
		}

		private void RegisterFlyoutEvents()
		{
			//Note, this is called AFTER flyout changes, so we must dispose of
			//the old handlers in the PropertyChanged method, unlike WinUI

			if (_flyout != null)
			{
				_flyout.Opened += OnFlyoutOpened;
				_flyout.Closed += OnFlyoutClosed;
				_flyoutPlacementDisposable = _flyout.GetPropertyChangedObservable(FlyoutBase.PlacementProperty).Subscribe(OnFlyoutPlacementChanged);
			}
		}

		protected void UpdateVisualStates()
		{
			if (_lastPointerDeviceType == PointerType.Touch || _isKeyDown)
			{
				PseudoClasses.Set(":secondarybuttonspan", true);
				PseudoClasses.Set(":secondarybuttonright", false);
			}
			else
			{
				PseudoClasses.Set(":secondarybuttonspan", false);
				PseudoClasses.Set(":secondarybuttonright", true);
			}

			if (_primaryButton != null && _secondaryButton != null)
			{
				if (_isFlyoutOpen)
				{
					PseudoClasses.Set(":flyoutopen", _isFlyoutOpen);
				}
				// SplitButton and ToggleSplitButton share a template -- this section is driving the checked states for ToggleSplitButton.
				else if (InternalIsChecked)
				{
					if (_lastPointerDeviceType == PointerType.Touch || _isKeyDown)
					{
						if (_primaryButton.IsPressed || _secondaryButton.IsPressed || _isKeyDown)
						{
							TempVisualStateManager.GoToState(this, ":checkedtouchpressed");
						}
						else
						{
							TempVisualStateManager.GoToState(this, ":checked");
						}
					}
					else if (_primaryButton.IsPressed)
					{
						TempVisualStateManager.GoToState(this, ":checkedprimarypressed");
					}
					else if (_primaryButton.IsPointerOver)
					{
						TempVisualStateManager.GoToState(this, ":checkedprimarypointerover");
					}
					else if (_secondaryButton.IsPressed)
					{
						TempVisualStateManager.GoToState(this, ":checkedsecondarypressed");
					}
					else if (_secondaryButton.IsPointerOver)
					{
						TempVisualStateManager.GoToState(this, ":checkedsecondarypointerover");
					}
					else
					{
						TempVisualStateManager.GoToState(this, ":checked");
					}
				}
				else
				{
					if (_lastPointerDeviceType == PointerType.Touch || _isKeyDown)
					{
						if (_primaryButton.IsPressed || _secondaryButton.IsPressed || _isKeyDown)
						{
							TempVisualStateManager.GoToState(this, ":touchpressed");
						}
						else
						{
							TempVisualStateManager.GoToState(this, null); //"Normal"
						}
					}
					else if (_primaryButton.IsPressed)
					{
						TempVisualStateManager.GoToState(this, ":primarypressed");
					}
					else if (_primaryButton.IsPointerOver)
					{
						TempVisualStateManager.GoToState(this, ":primarypointerover");
					}
					else if (_secondaryButton.IsPressed)
					{
						TempVisualStateManager.GoToState(this, ":secondarypressed");
					}
					else if (_secondaryButton.IsPointerOver)
					{
						TempVisualStateManager.GoToState(this, ":secondarypointerover");
					}
					else
					{
						TempVisualStateManager.GoToState(this, null); //"Normal"
					}
				}
			}

		}

		private void OnVisualPropertyChanged(AvaloniaPropertyChangedEventArgs args)
		{
			UpdateVisualStates();
		}

		private void OnFlyoutOpened(object sender, object args)
		{
			_isFlyoutOpen = true;
			UpdateVisualStates();
		}

		private void OnFlyoutClosed(object sender, object args)
		{
			_isFlyoutOpen = false;
			UpdateVisualStates();
		}

		private void OnFlyoutPlacementChanged(AvaloniaPropertyChangedEventArgs args)
		{
			UpdateVisualStates();
		}

		#endregion

		protected bool _hasLoaded;
		private FlyoutBase _flyout;
		private ICommand _command;
		private Button _primaryButton;
		private Button _secondaryButton;
		private IDisposable _primaryDisposable;
		private IDisposable _secondaryDisposable;
		private IDisposable _flyoutPlacementDisposable;
		private PointerType _lastPointerDeviceType;
		private bool _isKeyDown;
		private bool _isFlyoutOpen;
	}

	public class SplitButtonClickEventArgs { }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents a button with two parts that can be invoked separately. One part behaves like a standard button and the other part invokes a flyout.
	/// </summary>
	public partial class SplitButton : ContentControl
	{		
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
				_primaryButton.PointerEntered += OnPointerEvent;
				_primaryButton.PointerExited += OnPointerEvent;
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

				_secondaryButton.PointerEntered += OnPointerEvent;
				_secondaryButton.PointerExited += OnPointerEvent;
				_secondaryButton.PointerPressed += OnPointerEvent;
				_secondaryButton.PointerReleased += OnPointerEvent;
				//Dont have PointerCanceled
				_secondaryButton.PointerCaptureLost += OnPointerLostEvent;
			}

			RegisterFlyoutEvents();

			UpdateVisualStates();

			_hasLoaded = true;
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == FlyoutProperty)
			{
				//Must unregister events here while we have ref to old flyout
				if (change.OldValue is FlyoutBase f)
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

		private void OpenFlyout()
		{
			if (Flyout != null)
			{
				Flyout.ShowAt(this);
			}
		}

		private void UnregisterEvents()
		{
			if (_primaryButton == null || _secondaryButton == null)
				return;

			_primaryButton.Click -= OnClickPrimary;
			_secondaryButton.Click -= OnClickSecondary;

			_primaryButton.PointerEntered -= OnPointerEvent;
			_primaryButton.PointerExited -= OnPointerEvent;
			_primaryButton.PointerPressed -= OnPointerEvent;
			_primaryButton.PointerReleased -= OnPointerEvent;
			_primaryButton.PointerCaptureLost -= OnPointerLostEvent;
			_primaryDisposable.Dispose();
			_primaryDisposable = null;

			_secondaryButton.PointerEntered -= OnPointerEvent;
			_secondaryButton.PointerExited -= OnPointerEvent;
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

		// Separate event for different args in Avalonia
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
			// Note, this is called AFTER flyout changes, so we must dispose of
			// the old handlers in the PropertyChanged method, unlike WinUI

			if (_flyout != null)
			{
				_flyout.Opened += OnFlyoutOpened;
				_flyout.Closed += OnFlyoutClosed;
				_flyoutPlacementDisposable = _flyout.GetPropertyChangedObservable(FlyoutBase.PlacementProperty).Subscribe(OnFlyoutPlacementChanged);
			}
		}

		protected virtual void UpdateVisualStates()
		{
			// This is really messy *SIGH*

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
					PseudoClasses.Set(":flyoutopen", true);
				}
				else
				{
					PseudoClasses.Set(":flyoutopen", false);
		
					if (InternalIsChecked) // SplitToggleButton only
					{
						// Clear non-checked states
						PseudoClasses.Set(":touchpressed", false);
						PseudoClasses.Set(":primarypressed", false);
						PseudoClasses.Set(":primarypointerover", false);
						PseudoClasses.Set(":secondarypressed", false);
						PseudoClasses.Set(":secondarypointerover", false);

						if (_lastPointerDeviceType == PointerType.Touch || _isKeyDown)
						{
							if (_primaryButton.IsPressed || _secondaryButton.IsPressed || _isKeyDown)
							{
								PseudoClasses.Set(":checkedtouchpressed", true);
								PseudoClasses.Set(":checked", false);
								PseudoClasses.Set(":checkedprimarypressed", false);
								PseudoClasses.Set(":checkedprimarypointerover", false);
								PseudoClasses.Set(":checksecondarypressed", false);
								PseudoClasses.Set(":checkedsecondarypointerover", false);
							}
							else
							{
								PseudoClasses.Set(":checked", true);
								PseudoClasses.Set(":checkedtouchpressed", false);
								PseudoClasses.Set(":checkedprimarypressed", false);
								PseudoClasses.Set(":checkedprimarypointerover", false);
								PseudoClasses.Set(":checksecondarypressed", false);
								PseudoClasses.Set(":checkedsecondarypointerover", false);
							}
						}
						else if (_primaryButton.IsPressed)
						{
							PseudoClasses.Set(":checkedtouchpressed", false);
							PseudoClasses.Set(":checked", false);
							PseudoClasses.Set(":checkedprimarypressed", true);
							PseudoClasses.Set(":checkedprimarypointerover", false);
							PseudoClasses.Set(":checkedsecondarypressed", false);
							PseudoClasses.Set(":checkedsecondarypointerover", false);
						}
						else if (_primaryButton.IsPointerOver)
						{
							PseudoClasses.Set(":checkedtouchpressed", false);
							PseudoClasses.Set(":checked", false);
							PseudoClasses.Set(":checkedprimarypressed", false);
							PseudoClasses.Set(":checkedprimarypointerover", true);
							PseudoClasses.Set(":checkedsecondarypressed", false);
							PseudoClasses.Set(":checkedsecondarypointerover", false);
						}
						else if (_secondaryButton.IsPressed)
						{
							PseudoClasses.Set(":checkedtouchpressed", false);
							PseudoClasses.Set(":checked", false);
							PseudoClasses.Set(":checkedprimarypressed", false);
							PseudoClasses.Set(":checkedprimarypointerover", false);
							PseudoClasses.Set(":checkedsecondarypressed", true);
							PseudoClasses.Set(":checkedsecondarypointerover", false);
						}
						else if (_secondaryButton.IsPointerOver)
						{
							PseudoClasses.Set(":checkedtouchpressed", false);
							PseudoClasses.Set(":checked", false);
							PseudoClasses.Set(":checkedprimarypressed", false);
							PseudoClasses.Set(":checkedprimarypointerover", false);
							PseudoClasses.Set(":checkedsecondarypressed", false);
							PseudoClasses.Set(":checkedsecondarypointerover", true);
						}
						else // Checked
						{
							PseudoClasses.Set(":checkedtouchpressed", false);
							PseudoClasses.Set(":checked", true);
							PseudoClasses.Set(":checkedprimarypressed", false);
							PseudoClasses.Set(":checkedprimarypointerover", false);
							PseudoClasses.Set(":checkedsecondarypressed", false);
							PseudoClasses.Set(":checkedsecondarypointerover", false);
						}
					}
					else
					{
						// Clear any checked states, if needed
						if (this is ToggleSplitButton)
						{
							PseudoClasses.Set(":checkedtouchpressed", false);
							PseudoClasses.Set(":checked", false);
							PseudoClasses.Set(":checkedprimarypressed", false);
							PseudoClasses.Set(":checkedprimarypointerover", false);
							PseudoClasses.Set(":checkedsecondarypressed", false);
							PseudoClasses.Set(":checkedsecondarypointerover", false);
						}

						if (_lastPointerDeviceType == PointerType.Touch || _isKeyDown)
						{
							if (_primaryButton.IsPressed || _secondaryButton.IsPressed || _isKeyDown)
							{
								PseudoClasses.Set(":touchpressed", true);
								PseudoClasses.Set(":primarypressed", false);
								PseudoClasses.Set(":primarypointerover", false);
								PseudoClasses.Set(":secondarypressed", false);
								PseudoClasses.Set(":secondarypointerover", false);
							}
							else
							{
								PseudoClasses.Set(":touchpressed", false);
								PseudoClasses.Set(":primarypressed", false);
								PseudoClasses.Set(":primarypointerover", false);
								PseudoClasses.Set(":secondarypressed", false);
								PseudoClasses.Set(":secondarypointerover", false);
							}
						}
						else if (_primaryButton.IsPressed)
						{
							PseudoClasses.Set(":touchpressed", false);
							PseudoClasses.Set(":primarypressed", true);
							PseudoClasses.Set(":primarypointerover", false);
							PseudoClasses.Set(":secondarypressed", false);
							PseudoClasses.Set(":secondarypointerover", false);
						}
						else if (_primaryButton.IsPointerOver)
						{
							PseudoClasses.Set(":touchpressed", false);
							PseudoClasses.Set(":primarypressed", false);
							PseudoClasses.Set(":primarypointerover", true);
							PseudoClasses.Set(":secondarypressed", false);
							PseudoClasses.Set(":secondarypointerover", false);
						}
						else if (_secondaryButton.IsPressed)
						{
							PseudoClasses.Set(":touchpressed", false);
							PseudoClasses.Set(":primarypressed", false);
							PseudoClasses.Set(":primarypointerover", false);
							PseudoClasses.Set(":secondarypressed", true);
							PseudoClasses.Set(":secondarypointerover", false);
						}
						else if (_secondaryButton.IsPointerOver)
						{
							PseudoClasses.Set(":touchpressed", false);
							PseudoClasses.Set(":primarypressed", false);
							PseudoClasses.Set(":primarypointerover", false);
							PseudoClasses.Set(":secondarypressed", false);
							PseudoClasses.Set(":secondarypointerover", true);
						}
						else
						{
							PseudoClasses.Set(":touchpressed", false);
							PseudoClasses.Set(":primarypressed", false);
							PseudoClasses.Set(":primarypointerover", false);
							PseudoClasses.Set(":secondarypressed", false);
							PseudoClasses.Set(":secondarypointerover", false);
						}
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

		protected bool _hasLoaded;		
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
}

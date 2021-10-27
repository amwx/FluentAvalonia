using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Presents a asyncronous dialog to the user.
	/// Note, Tab navigation currently doesn't work
	/// </summary>
	public class ContentDialog : ContentControl, ICustomKeyboardNavigation
	{
		public ContentDialog()
		{
			PseudoClasses.Add(":hidden");
		}

		static ContentDialog()
		{
			FullSizeDesiredProperty.Changed.AddClassHandler<ContentDialog>((x, v) => x.OnFullSizedDesiredChanged(v));
		}

		public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
			AvaloniaProperty.Register<ContentDialog, ICommand>(nameof(CloseButtonCommand));

		public static readonly StyledProperty<object> CloseButtonCommandParameterProperty =
			AvaloniaProperty.Register<ContentDialog, object>(nameof(CloseButtonCommandParameter));

		public static readonly StyledProperty<string> CloseButtonTextProperty =
			AvaloniaProperty.Register<ContentDialog, string>(nameof(CloseButtonText));

		public static readonly StyledProperty<ContentDialogButton> DefaultButtonProperty =
			AvaloniaProperty.Register<ContentDialog, ContentDialogButton>(nameof(DefaultButton), ContentDialogButton.None);

		public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
			AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);

		public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
			AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);

		public static readonly StyledProperty<ICommand> PrimaryButtonCommandProperty =
			AvaloniaProperty.Register<ContentDialog, ICommand>(nameof(PrimaryButtonCommand));

		public static readonly StyledProperty<object> PrimaryButtonCommandParameterProperty =
			AvaloniaProperty.Register<ContentDialog, object>(nameof(PrimaryButtonCommandParameter));

		public static readonly StyledProperty<string> PrimaryButtonTextProperty =
			AvaloniaProperty.Register<ContentDialog, string>(nameof(PrimaryButtonText));

		public static readonly StyledProperty<ICommand> SecondaryButtonCommandProperty =
			AvaloniaProperty.Register<ContentDialog, ICommand>(nameof(SecondaryButtonCommand));

		public static readonly StyledProperty<object> SecondaryButtonCommandParameterProperty =
			AvaloniaProperty.Register<ContentDialog, object>(nameof(SecondaryButtonCommandParameter));

		public static readonly StyledProperty<string> SecondaryButtonTextProperty =
			AvaloniaProperty.Register<ContentDialog, string>(nameof(SecondaryButtonText));

		public static readonly StyledProperty<object> TitleProperty =
			AvaloniaProperty.Register<ContentDialog, object>(nameof(Title), "");

		public static readonly StyledProperty<IDataTemplate> TitleTemplateProperty =
			AvaloniaProperty.Register<ContentDialog, IDataTemplate>(nameof(TitleTemplate));

		public static readonly StyledProperty<bool> FullSizeDesiredProperty =
			AvaloniaProperty.Register<ContentDialog, bool>(nameof(FullSizeDesired));

		/// <summary>
		/// Command to execute when the close button is clicked
		/// </summary>
		public ICommand CloseButtonCommand
		{
			get
			{
				return GetValue(CloseButtonCommandProperty);
			}
			set
			{
				SetValue(CloseButtonCommandProperty, value);
			}
		}

		/// <summary>
		/// CommandParameter for the close button
		/// </summary>
		public object CloseButtonCommandParameter
		{
			get
			{
				return GetValue(CloseButtonCommandParameterProperty);
			}
			set
			{
				SetValue(CloseButtonCommandParameterProperty, value);
			}
		}

		/// <summary>
		/// Close Button Text
		/// </summary>
		public string CloseButtonText
		{
			get
			{
				return GetValue(CloseButtonTextProperty);
			}
			set
			{
				SetValue(CloseButtonTextProperty, value);
			}
		}

		/// <summary>
		/// Sets which button is the Default, closing the dialog when enter/space is pressed
		/// </summary>
		public ContentDialogButton DefaultButton
		{
			get
			{
				return GetValue(DefaultButtonProperty);
			}
			set
			{
				SetValue(DefaultButtonProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets primary button enabled
		/// </summary>
		public bool IsPrimaryButtonEnabled
		{
			get
			{
				return GetValue(IsPrimaryButtonEnabledProperty);
			}
			set
			{
				SetValue(IsPrimaryButtonEnabledProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets secondary button enabled
		/// </summary>
		public bool IsSecondaryButtonEnabled
		{
			get
			{
				return GetValue(IsSecondaryButtonEnabledProperty);
			}
			set
			{
				SetValue(IsSecondaryButtonEnabledProperty, value);
			}
		}

		/// <summary>
		/// Command for the Primary Button
		/// </summary>
		public ICommand PrimaryButtonCommand
		{
			get
			{
				return GetValue(PrimaryButtonCommandProperty);
			}
			set
			{
				SetValue(PrimaryButtonCommandProperty, value);
			}
		}

		/// <summary>
		/// CommandParamter for the PrimaryButton
		/// </summary>
		public object PrimaryButtonCommandParameter
		{
			get
			{
				return GetValue(PrimaryButtonCommandParameterProperty);
			}
			set
			{
				SetValue(PrimaryButtonCommandParameterProperty, value);
			}
		}

		/// <summary>
		/// Primary Button Text
		/// </summary>
		public string PrimaryButtonText
		{
			get
			{
				return GetValue(PrimaryButtonTextProperty);
			}
			set
			{
				SetValue(PrimaryButtonTextProperty, value);
			}
		}

		/// <summary>
		/// Command for the secondary button
		/// </summary>
		public ICommand SecondaryButtonCommand
		{
			get
			{
				return GetValue(SecondaryButtonCommandProperty);
			}
			set
			{
				SetValue(SecondaryButtonCommandProperty, value);
			}
		}

		/// <summary>
		/// CommandParameter for the Secondary button
		/// </summary>
		public object SecondaryButtonCommandParameter
		{
			get
			{
				return GetValue(SecondaryButtonCommandParameterProperty);
			}
			set
			{
				SetValue(SecondaryButtonCommandParameterProperty, value);
			}
		}

		/// <summary>
		/// SecondaryButtonText
		/// </summary>
		public string SecondaryButtonText
		{
			get
			{
				return GetValue(SecondaryButtonTextProperty);
			}
			set
			{
				SetValue(SecondaryButtonTextProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the Dialog Title
		/// </summary>
		public object Title
		{
			get
			{
				return GetValue(TitleProperty);
			}
			set
			{
				SetValue(TitleProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the template for the Title
		/// </summary>
		public IDataTemplate TitleTemplate
		{
			get
			{
				return GetValue(TitleTemplateProperty);
			}
			set
			{
				SetValue(TitleTemplateProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets whether the Dialog should show full screen
		/// On WinUI3, at least desktop, this just show the dialog at 
		/// the maximum size of a contentdialog.
		/// </summary>
		public bool FullSizeDesired
		{
			get => GetValue(FullSizeDesiredProperty);
			set => SetValue(FullSizeDesiredProperty, value);
		}

		public event TypedEventHandler<ContentDialog, object> Opening;
		public event TypedEventHandler<ContentDialog, object> Opened;
		public event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs> Closing;
		public event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> Closed;
		public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> PrimaryButtonClick;
		public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> SecondaryButtonClick;
		public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> CloseButtonClick;

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			if (_primaryButton != null)
				_primaryButton.Click -= OnButtonClick;
			if (_secondaryButton != null)
				_secondaryButton.Click -= OnButtonClick;
			if (_closeButton != null)
				_closeButton.Click -= OnButtonClick;

			base.OnApplyTemplate(e);

			_primaryButton = e.NameScope.Get<Button>("PrimaryButton");
			_primaryButton.Click += OnButtonClick;
			_secondaryButton = e.NameScope.Get<Button>("SecondaryButton");
			_secondaryButton.Click += OnButtonClick;
			_closeButton = e.NameScope.Get<Button>("CloseButton");
			_closeButton.Click += OnButtonClick;
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.Handled)
			{
				base.OnKeyUp(e);
				return;
			}

			switch (e.Key)
			{
				case Key.Escape:
					HideCore();
					e.Handled = true;
					break;

				case Key.Enter:
					var curFocus = FocusManager.Instance?.Current;
					if (curFocus != null)
					{
						if (curFocus == _primaryButton)
						{
							OnButtonClick(_primaryButton, null);
						}
						else if (curFocus == _secondaryButton)
						{
							OnButtonClick(_secondaryButton, null);
						}
						else if (curFocus == _closeButton)
						{
							OnButtonClick(_closeButton, null);
						}
						else if (Content is IControl c && c.Focusable && c.IsFocused)
						{
							//Assume primary button is "OK"
							OnButtonClick(_primaryButton, null);
						}
						e.Handled = true;
					}
					break;
			}
			base.OnKeyUp(e);
		}

		protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnAttachedToVisualTree(e);

			//Failsafe incase this wasn't able to be called in ShowAsync b/c template
			//hadn't been applied yet
			SetupDialog();
		}

		public async Task<ContentDialogResult> ShowAsync(ContentDialogPlacement placement = ContentDialogPlacement.Popup)
		{
			if (placement == ContentDialogPlacement.InPlace)
				throw new NotImplementedException("InPlace not implemented yet");
			tcs = new TaskCompletionSource<ContentDialogResult>();

			OnOpening();

			if (this.Parent != null)
			{
				_originalHost = Parent;
				if (_originalHost is Panel p)
				{
					_originalHostIndex = p.Children.IndexOf(this);
					p.Children.Remove(this);
				}
				else if (_originalHost is Decorator d)
				{
					d.Child = null;
				}
				else if (_originalHost is IContentControl cc)
				{
					cc.Content = null;
				}
				else if (_originalHost is IContentPresenter cp)
				{
					cp.Content = null;
				}
			}

			if (_host == null)
			{
				_host = new DialogHost();
			}

			_host.Content = this;

			_lastFocus = FocusManager.Instance.Current;

			if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime al)
			{
				Window activeWindow = null;
				foreach (var item in al.Windows)
				{
					if (item.IsActive)
					{
						activeWindow = item;
						break;
					}
				}

				//Fallback, just in case
				if (activeWindow == null)
					activeWindow = al.MainWindow;

				var ol = OverlayLayer.GetOverlayLayer(activeWindow);
				if (ol == null)
					throw new InvalidOperationException();
				
				ol.Children.Add(_host);
			}
			else if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime sl)
			{
				var ol = OverlayLayer.GetOverlayLayer(sl.MainView);
				if (ol == null)
					throw new InvalidOperationException();

				ol.Children.Add(_host);
			}

			IsVisible = true;
			ShowCore();
			SetupDialog();
			return await tcs.Task;
		}

		public void Hide()
		{
			result = ContentDialogResult.None;
			HideCore();
		}

		internal void CompleteButtonClickDeferral()
		{
			IsEnabled = true;
			HideCore();
		}

		internal void CompleteClosingDeferral()
		{
			//Don't call HideCore() here, that could send us on an infinite loop
			//and will send out multiple Closing Events, re-enable & final close
			IsEnabled = true;
			FinalCloseDialog();
		}

		protected virtual void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
		{
			PrimaryButtonClick?.Invoke(this, args);
		}

		protected virtual void OnSecondaryButtonClick(ContentDialogButtonClickEventArgs args)
		{
			SecondaryButtonClick?.Invoke(this, args);
		}

		protected virtual void OnCloseButtonClick(ContentDialogButtonClickEventArgs args)
		{
			CloseButtonClick?.Invoke(this, args);
		}

		protected virtual void OnOpening()
		{
			Opening?.Invoke(this, null);
		}

		protected virtual void OnOpened()
		{
			Opened?.Invoke(this, null);
		}

		protected virtual void OnClosing(ContentDialogClosingEventArgs args)
		{
			Closing?.Invoke(this, args);
		}

		protected virtual void OnClosed(ContentDialogClosedEventArgs args)
		{
			Closed?.Invoke(this, args);
		}

		private void ShowCore()
		{
			IsVisible = true;
			PseudoClasses.Set(":hidden", false);
			PseudoClasses.Set(":open", true);

			OnOpened();
		}

		private void HideCore()
		{
			var ea = new ContentDialogClosingEventArgs(this, result);
			OnClosing(ea);

			if (ea.Cancel)
				return;

			if (!ea.IsDeferred)
			{
				FinalCloseDialog();
			}
			else
			{
				IsEnabled = false;
			}
		}

		private void SetupDialog()
		{
			if (_primaryButton == null)
				ApplyTemplate();

			PseudoClasses.Set(":primary", !string.IsNullOrEmpty(PrimaryButtonText));
			PseudoClasses.Set(":secondary", !string.IsNullOrEmpty(SecondaryButtonText));
			PseudoClasses.Set(":close", !string.IsNullOrEmpty(CloseButtonText));

			if (this.FindAncestorOfType<DialogHost>() != null)
			{
				switch (DefaultButton)
				{
					case ContentDialogButton.Primary:
						if (!_primaryButton.IsVisible)
							break;

						_primaryButton.Classes.Add("accent");
						_secondaryButton.Classes.Remove("accent");
						_closeButton.Classes.Remove("accent");
						if (Content is IControl cp && cp.Focusable)
						{
							cp.Focus();
						}
						else
						{
							_primaryButton.Focus();
						}

						break;

					case ContentDialogButton.Secondary:
						if (!_secondaryButton.IsVisible)
							break;

						_secondaryButton.Classes.Add("accent");
						_primaryButton.Classes.Remove("accent");
						_closeButton.Classes.Remove("accent");
						if (Content is IControl cs && cs.Focusable)
						{
							cs.Focus();
						}
						else
						{
							_secondaryButton.Focus();
						}

						break;

					case ContentDialogButton.Close:
						if (!_closeButton.IsVisible)
							break;

						_closeButton.Classes.Add("accent");
						_primaryButton.Classes.Remove("accent");
						_secondaryButton.Classes.Remove("accent");
						if (Content is IControl cc && cc.Focusable)
						{
							cc.Focus();
						}
						else
						{
							_closeButton.Focus();
						}

						break;

					default:
						_closeButton.Classes.Remove("accent");
						_primaryButton.Classes.Remove("accent");
						_secondaryButton.Classes.Remove("accent");

						if (Content is IControl cd && cd.Focusable)
						{
							cd.Focus();
						}
						else if (_primaryButton.IsVisible)
						{
							_primaryButton.Focus();
						}
						else if (_secondaryButton.IsVisible)
						{
							_secondaryButton.Focus();
						}
						else if (_closeButton.IsVisible)
						{
							_closeButton.Focus();
						}
						else
						{
							Focus();
						}

						break;
				}
			}
		}

		//This is the exit point for the ContentDialog
		//This method MUST be called to finalize everything
		private async void FinalCloseDialog()
		{
			//Prevent interaction when closing...double/mutliple clicking on the buttons to close
			//the dialog was calling this multiple times, which would cause the OverlayLayer check
			//below to fail (as this would be removed from the tree). This is a simple workaround
			//to make sure we don't error out
			this.IsHitTestVisible = false;

			//For a better experience when animating closed, we need to make sure the
			//focus adorner is not showing (if using keyboard) otherwise that will hang
			//around and not fade out and it just looks weird. So focus this to force the
			//adorner to hide, then continue forward.
			Focus();

			PseudoClasses.Set(":hidden", true);
			PseudoClasses.Set(":open", false);

			// Let the close animation finish (now 0.167s in new WinUI update...)
			// We'll wait just a touch longer to be sure
			await Task.Delay(200);

			//Re-enable interaction in case we reuse the dialog
			this.IsHitTestVisible = true;

			OnClosed(new ContentDialogClosedEventArgs(result));

			if (_lastFocus != null)
			{
				FocusManager.Instance.Focus(_lastFocus, NavigationMethod.Unspecified);
				_lastFocus = null;
			}
						
			var ol = OverlayLayer.GetOverlayLayer(_host);
			// If OverlayLayer isn't found here, this may be a reentrant call (hit ESC multiple times quickly, etc)
			// Don't fail, and return. If this isn't reentrant, there's bigger issues...
			if (ol == null)
				return;

			ol.Children.Remove(_host);

			_host.Content = null;

			if (_originalHost != null)
			{
				if (_originalHost is Panel p)
				{
					p.Children.Insert(_originalHostIndex, this);
				}
				else if (_originalHost is Decorator d)
				{
					d.Child = this;
				}
				else if (_originalHost is IContentControl cc)
				{
					cc.Content = this;
				}
				else if (_originalHost is IContentPresenter cp)
				{
					cp.Content = this;
				}
			}

			tcs.TrySetResult(result);
		}

		private void OnButtonClick(object sender, RoutedEventArgs e)
		{
			if (sender == _primaryButton)
			{
				HandlePrimaryClick();
			}
			else if (sender == _secondaryButton)
			{
				HandleSecondaryClick();
			}
			else if (sender == _closeButton)
			{
				HandleCloseClick();
			}
		}

		private void HandlePrimaryClick()
		{
			var ea = new ContentDialogButtonClickEventArgs(this);
			OnPrimaryButtonClick(ea);

			if (ea.Cancel)
				return;

			result = ContentDialogResult.Primary;
			if (!ea.IsDeferred)
			{
				if (PrimaryButtonCommand != null && PrimaryButtonCommand.CanExecute(PrimaryButtonCommandParameter))
				{
					PrimaryButtonCommand.Execute(PrimaryButtonCommandParameter);
				}
				HideCore();
			}
			else
			{
				IsEnabled = false;
			}
		}

		private void HandleSecondaryClick()
		{
			var ea = new ContentDialogButtonClickEventArgs(this);
			OnSecondaryButtonClick(ea);

			if (ea.Cancel)
				return;

			result = ContentDialogResult.Secondary;
			if (!ea.IsDeferred)
			{
				if (SecondaryButtonCommand != null && SecondaryButtonCommand.CanExecute(SecondaryButtonCommandParameter))
				{
					SecondaryButtonCommand.Execute(SecondaryButtonCommandParameter);
				}
				HideCore();
			}
			else
			{
				IsEnabled = false;
			}
		}

		private void HandleCloseClick()
		{
			var ea = new ContentDialogButtonClickEventArgs(this);
			OnCloseButtonClick(ea);

			if (ea.Cancel)
				return;

			result = ContentDialogResult.None;
			if (!ea.IsDeferred)
			{
				if (CloseButtonCommand != null && CloseButtonCommand.CanExecute(CloseButtonCommandParameter))
				{
					CloseButtonCommand.Execute(CloseButtonCommandParameter);
				}
				HideCore();
			}
			else
			{
				IsEnabled = false;
			}
		}

		private void OnFullSizedDesiredChanged(AvaloniaPropertyChangedEventArgs e)
		{
			bool newVal = (bool)e.NewValue;
			PseudoClasses.Set(":fullsize", newVal);
		}

		public (bool handled, IInputElement next) GetNext(IInputElement element, NavigationDirection direction)
		{
			var children = this.GetVisualDescendants().OfType<IInputElement>()
				.Where(x => KeyboardNavigation.GetIsTabStop((InputElement)x) && x.Focusable &&
				x.IsEffectivelyVisible && IsEffectivelyEnabled).ToList();

			if (children.Count == 0)
				return (false, null);

			var current = FocusManager.Instance?.Current;
			if (current == null)
				return (false, null);

			if (direction == NavigationDirection.Next)
			{
				for (int i = 0; i < children.Count; i++)
				{
					if (children[i] == current)
					{
						if (i == children.Count - 1)
						{
							return (true, children[0]);
						}
						else
						{
							return (true, children[i + 1]);
						}
					}
				}
			}
			else if (direction == NavigationDirection.Previous)
			{
				for (int i = children.Count - 1; i >= 0; i--)
				{
					if (children[i] == current)
					{
						if (i == 0)
						{
							return (true, children[^1]);
						}
						else
						{
							return (true, children[i - 1]);
						}
					}
				}
			}

			return (false, null);
		}

		//Store the last element focused before showing the dialog, so we can
		//restore it when it closes
		private IInputElement _lastFocus;
		private IControl _originalHost;
		private int _originalHostIndex;
	    private DialogHost _host;
		private ContentDialogResult result;
		private TaskCompletionSource<ContentDialogResult> tcs;
		private Button _primaryButton;
		private Button _secondaryButton;
		private Button _closeButton;
	}

	public class DialogHost : ContentControl, IStyleable
	{
		public DialogHost()
		{
			Background = null;
			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
			VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
		}

		Type IStyleable.StyleKey => typeof(OverlayPopupHost);

		protected override Size MeasureOverride(Size availableSize)
		{
			_ = base.MeasureOverride(availableSize);

			if (this.VisualRoot is TopLevel tl)
			{
				return tl.ClientSize;
			}
			else if (VisualRoot is IControl c)
			{
				return c.Bounds.Size;
			}

			return Size.Empty;
		}

		protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnAttachedToVisualTree(e);
			if (e.Root is IControl wb)
			{
				// OverlayLayer is a Canvas, so we won't get a signal to resize if the window
				// bounds change. Subscribe to force update
				wb.GetObservable(BoundsProperty).Subscribe(_ => OnRootBoundsChanged());
			}
		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);
			_rootBoundsWatcher?.Dispose();
			_rootBoundsWatcher = null;
		}

		private void OnRootBoundsChanged()
		{
			InvalidateMeasure();
		}

		private IDisposable _rootBoundsWatcher;
	}
}

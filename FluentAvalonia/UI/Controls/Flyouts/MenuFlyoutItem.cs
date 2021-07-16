using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using FluentAvalonia.UI.Input;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
	public class MenuFlyoutItem : MenuFlyoutItemBase, IMenuItem, ICommandSource
	{
		public static readonly StyledProperty<string> TextProperty =
			AvaloniaProperty.Register<MenuFlyoutItem, string>(nameof(Text));

		public static readonly StyledProperty<IconElement> IconProperty =
			AvaloniaProperty.Register<MenuFlyoutItem, IconElement>(nameof(Icon));

		public static readonly DirectProperty<MenuFlyoutItem, ICommand> CommandProperty =
			Button.CommandProperty.AddOwner<MenuFlyoutItem>(x => x.Command,
				(x, v) => x.Command = v);

		public static readonly StyledProperty<object> CommandParameterProperty =
			Button.CommandParameterProperty.AddOwner<MenuFlyoutItem>();

		public static readonly StyledProperty<KeyGesture> HotKeyProperty =
			Button.HotKeyProperty.AddOwner<MenuFlyoutItem>();


		public string Text
		{
			get => GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public IconElement Icon
		{
			get => GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		public KeyGesture HotKey
		{
			get => GetValue(HotKeyProperty);
			set => SetValue(HotKeyProperty, value);
		}

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

		protected override bool IsEnabledCore => base.IsEnabledCore && _canExecute;

		public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
			RoutedEvent.Register<MenuFlyoutItem, RoutedEventArgs>(nameof(Click),
			  RoutingStrategies.Bubble);

		public event EventHandler<RoutedEventArgs> Click
		{
			add => AddHandler(ClickEvent, value);
			remove => RemoveHandler(ClickEvent, value);
		}


		bool IMenuItem.HasSubMenu => false;
		bool IMenuItem.IsPointerOverSubMenu => false;
		bool IMenuItem.IsSubMenuOpen { get => false; set { } }
		public bool IsTopLevel => false;
		IMenuItem IMenuElement.SelectedItem { get => null; set { } }
		IEnumerable<IMenuItem> IMenuElement.SubItems => null;
		IMenuElement IMenuItem.Parent
		{
			get
			{
				if (this.FindLogicalAncestorOfType<MenuFlyoutSubItem>() is MenuFlyoutSubItem mfsi)
				{
					return mfsi;
				}

				return Parent as IMenuElement;
			}
		}


		protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
		{
			if (_hotkey != null)
			{
				HotKey = _hotkey;
			}

			base.OnAttachedToLogicalTree(e);

			if (Command != null)
			{
				Command.CanExecuteChanged += CanExecuteChanged;
				CanExecuteChanged(this, null);
			}
		}

		protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
		{
			if (HotKey != null)
			{
				_hotkey = HotKey;
				HotKey = null;
			}

			base.OnDetachedFromLogicalTree(e);

			if (Command != null)
			{
				Command.CanExecuteChanged -= CanExecuteChanged;
			}
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == CommandProperty)
			{
				var oldCommand = change.OldValue.GetValueOrDefault() as ICommand;
				var newCommand = change.NewValue.GetValueOrDefault() as ICommand;

				if (oldCommand is XamlUICommand oldXaml)
				{
					if (Text == oldXaml.Label)
					{
						Text = null;
					}

					if (Icon is IconSourceElement ele && ele.IconSource == oldXaml.IconSource)
					{
						Icon = null;
					}

					if (HotKey == oldXaml.HotKey)
					{
						HotKey = null;
					}
				}

				if (newCommand is XamlUICommand newXaml)
				{
					if (string.IsNullOrEmpty(Text))
					{
						Text = newXaml.Label;
					}

					if (Icon == null)
					{
						Icon = new IconSourceElement { IconSource = newXaml.IconSource };
					}

					if (HotKey == null)
					{
						HotKey = newXaml.HotKey;
					}
				}

				if (((ILogical)this).IsAttachedToLogicalTree)
				{
					if (oldCommand != null)
					{
						oldCommand.CanExecuteChanged -= CanExecuteChanged;
					}

					if (newCommand != null)
					{
						newCommand.CanExecuteChanged += CanExecuteChanged;
					}
				}

				CanExecuteChanged(this, null);
			}
			else if (change.Property == CommandParameterProperty)
			{
				CanExecuteChanged(this, null);
			}
			else if (change.Property == HotKeyProperty)
			{
				PseudoClasses.Set(":hotkey", change.NewValue.GetValueOrDefault() != null);
			}
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
			if (e.InitialPressMouseButton == MouseButton.Left)
			{
				PseudoClasses.Set(":pressed", false);
			}
		}

		void IMenuElement.Close() { }
		void IMenuElement.Open() { }

		public bool MoveSelection(NavigationDirection direction, bool wrap)
		{
			return false;
		}

		public void RaiseClick()
		{
			OnClick();
		}

		protected virtual void OnClick()
		{
			RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

			if (Command?.CanExecute(CommandParameter) == true)
			{
				Command.Execute(CommandParameter);
			}
		}

		private void CanExecuteChanged(object sender, EventArgs e)
		{
			var canExec = Command == null || Command.CanExecute(CommandParameter);

			if (canExec != _canExecute)
			{
				_canExecute = canExec;
				UpdateIsEffectivelyEnabled();
			}
		}

		protected override void OnGotFocus(GotFocusEventArgs e)
		{
			base.OnGotFocus(e);
		}

		void ICommandSource.CanExecuteChanged(object sender, EventArgs e) => this.CanExecuteChanged(sender, e);

		private ICommand _command;
		private bool _canExecute = true;
		private KeyGesture _hotkey;
	}
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using FluentAvalonia.UI.Input;
using System;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents a command in a <see cref="MenuFlyout"/> control.
	/// </summary>
	public partial class MenuFlyoutItem : MenuFlyoutItemBase, IMenuItem, ICommandSource
	{
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

            // For this, we make assumption that if you set Hotkey or assign a XamlUICommand,
            // you won't also set the InputGesture to something different, so setting HotKey
            // or using a XamlUICommand will automatically set the InputGesture property

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

					if (InputGesture == oldXaml.HotKey)
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

					if (InputGesture == null)
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
			else if (change.Property == InputGestureProperty)
			{
				PseudoClasses.Set(":hotkey", change.NewValue.GetValueOrDefault() != null);
			}
            else if (change.Property == HotKeyProperty)
            {
                var kg = change.NewValue.GetValueOrDefault<KeyGesture>();
                InputGesture = kg;
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
				
		protected virtual void OnClick()
		{
			RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, this));

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

		bool IMenuElement.MoveSelection(NavigationDirection direction, bool wrap) => false;

		void IMenuItem.RaiseClick() => OnClick();

		void ICommandSource.CanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged(sender, e);
		
		void IMenuElement.Close() { }
		
		void IMenuElement.Open() { }

		private bool _canExecute = true;
		private KeyGesture _hotkey;
	}
}

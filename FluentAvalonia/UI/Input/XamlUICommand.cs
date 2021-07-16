using Avalonia;
using Avalonia.Input;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Windows.Input;

namespace FluentAvalonia.UI.Input
{
	public class XamlUICommand : AvaloniaObject, ICommand
	{
		public static readonly StyledProperty<ICommand> CommandProperty =
			AvaloniaProperty.Register<XamlUICommand, ICommand>(nameof(Command));

		public static readonly StyledProperty<string> DescriptionProperty =
			AvaloniaProperty.Register<XamlUICommand, string>(nameof(Description));

		public static readonly StyledProperty<IconSource> IconSourceProperty =
			AvaloniaProperty.Register<XamlUICommand, IconSource>(nameof(IconSource));

		public static readonly StyledProperty<KeyGesture> HotKeyProperty =
			AvaloniaProperty.Register<XamlUICommand, KeyGesture>(nameof(HotKey));

		public static readonly StyledProperty<string> LabelProperty =
			AvaloniaProperty.Register<XamlUICommand, string>(nameof(Label));

		public ICommand Command
		{
			get => GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public string Description
		{
			get => GetValue(DescriptionProperty);
			set => SetValue(DescriptionProperty, value);
		}

		public IconSource IconSource
		{
			get => GetValue(IconSourceProperty);
			set => SetValue(IconSourceProperty, value);
		}

		public KeyGesture HotKey
		{
			get => GetValue(HotKeyProperty);
			set => SetValue(HotKeyProperty, value);
		}

		public string Label
		{
			get => GetValue(LabelProperty);
			set => SetValue(LabelProperty, value);
		}


		public event EventHandler CanExecuteChanged;

		public event TypedEventHandler<XamlUICommand, CanExecuteRequestedEventArgs> CanExecuteRequested;
		public event TypedEventHandler<XamlUICommand, ExecuteRequestedEventArgs> ExecuteRequested;

		public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, null);

		public bool CanExecute(object param)
		{
			bool canExec = false;

			var args = new CanExecuteRequestedEventArgs(param);

			CanExecuteRequested?.Invoke(this, args);

			canExec = args.CanExecute;

			var command = Command;
			if (command != null)
			{
				bool canExecCommand = command.CanExecute(param);
				canExec = canExec && canExecCommand;
			}

			return canExec;
		}

		public void Execute(object param)
		{
			var args = new ExecuteRequestedEventArgs(param);

			ExecuteRequested?.Invoke(this, args);

			Command?.Execute(param);
		}
	}
}

using Avalonia;
using Avalonia.Controls.Primitives;
using FluentAvalonia.Core;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
	public partial class SplitButton
	{
		/// <summary>
		/// Defines the <see cref="Command"/> property
		/// </summary>
		public static readonly DirectProperty<SplitButton, ICommand> CommandProperty =
			Button.CommandProperty.AddOwner<SplitButton>(x => x.Command, (x, v) => x.Command = v);

		/// <summary>
		/// Defines the <see cref="CommandParameter"/> property
		/// </summary>
		public static readonly StyledProperty<object> CommandParameterProperty =
			Button.CommandParameterProperty.AddOwner<SplitButton>();

		/// <summary>
		/// Defines the <see cref="Flyout"/> property
		/// </summary>
		public static readonly DirectProperty<SplitButton, FlyoutBase> FlyoutProperty =
			AvaloniaProperty.RegisterDirect<SplitButton, FlyoutBase>(nameof(Flyout),
				x => x.Flyout, (x, v) => x.Flyout = v);

		/// <summary>
		/// Gets or sets the command to invoke when this button is pressed.
		/// </summary>
		public ICommand Command
		{
			get => _command;
			set => SetAndRaise(CommandProperty, ref _command, value);
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="Command"/> property.
		/// </summary>
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		/// <summary>
		/// Gets or sets the flyout associated with this button.
		/// </summary>
		public FlyoutBase Flyout
		{
			get => _flyout;
			set => SetAndRaise(FlyoutProperty, ref _flyout, value);
		}

		internal virtual bool InternalIsChecked => false;

		public event TypedEventHandler<SplitButton, SplitButtonClickEventArgs> Click;

		private FlyoutBase _flyout;
	}
}

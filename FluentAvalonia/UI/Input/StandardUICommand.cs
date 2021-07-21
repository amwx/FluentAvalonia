using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Input
{
	public enum StandardUICommandKind
	{
		None = 0,
		Cut = 1,
		Copy = 2,
		Paste = 3,
		SelectAll = 4,
		Delete = 5,
		Share = 6,
		Save = 7,
		Open = 8,
		Close = 9,
		Pause = 10,
		Play = 11,
		Stop = 12,
		Forward = 13,
		Backward = 14,
		Undo = 15,
		Redo = 16
	}

	public class StandardUICommand : XamlUICommand
	{
		public StandardUICommand() { }

		public StandardUICommand(StandardUICommandKind kind)
		{
			Kind = kind;

			SetupCommand();
		}

		public static readonly StyledProperty<StandardUICommandKind> KindProperty =
			AvaloniaProperty.Register<StandardUICommand, StandardUICommandKind>(nameof(Kind));

		public StandardUICommandKind Kind
		{
			get => GetValue(KindProperty);
			set => SetValue(KindProperty, value);
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == KindProperty)
			{
				SetupCommand();
			}
		}

		private void SetupCommand()
		{
			switch (Kind)
			{
				case StandardUICommandKind.None:
					Label = string.Empty;
					IconSource = null;
					Description = string.Empty;
					HotKey = null;
					break;

				case StandardUICommandKind.Cut:
					Label = "Cut";
					IconSource = new SymbolIconSource { Symbol = Symbol.Cut };
					Description = "Remove the selected content and put it on the clipboard";
					HotKey = new KeyGesture(Key.X, KeyModifiers.Control);
					break;

				case StandardUICommandKind.Copy:
					Label = "Copy";
					IconSource = new SymbolIconSource { Symbol = Symbol.Copy };
					Description = "Copy the selected content to the clipboard";
					HotKey = new KeyGesture(Key.C, KeyModifiers.Control);
					break;

				case StandardUICommandKind.Paste:
					Label = "Paste";
					IconSource = new SymbolIconSource { Symbol = Symbol.Paste };
					Description = "Insert the contents of the clipboard at the current location";
					HotKey = new KeyGesture(Key.V, KeyModifiers.Control);
					break;

				case StandardUICommandKind.SelectAll:
					Label = "Select All";
					IconSource = new SymbolIconSource { Symbol = Symbol.SelectAll };
					Description = "Select all content";
					HotKey = new KeyGesture(Key.A, KeyModifiers.Control);
					break;

				case StandardUICommandKind.Delete:
					Label = "Delete";
					IconSource = new SymbolIconSource { Symbol = Symbol.Delete };
					Description = "Delete the selected content";
					HotKey = new KeyGesture(Key.Delete);
					break;

				case StandardUICommandKind.Share:
					Label = "Share";
					IconSource = new SymbolIconSource { Symbol = Symbol.Share };
					Description = "Share the selected content";
					// No HotKey
					break;

				case StandardUICommandKind.Save:
					Label = "Save";
					IconSource = new SymbolIconSource { Symbol = Symbol.Save };
					Description = "Save your changes";
					HotKey = new KeyGesture(Key.S, KeyModifiers.Control);
					break;

				case StandardUICommandKind.Open:
					Label = Description = "Open";
					IconSource = new SymbolIconSource { Symbol = Symbol.Open };
					HotKey = new KeyGesture(Key.O, KeyModifiers.Control);
					break;

				case StandardUICommandKind.Close:
					Label = Description = "Close";
					IconSource = new SymbolIconSource { Symbol = Symbol.Dismiss };
					HotKey = new KeyGesture(Key.W, KeyModifiers.Control);
					break;

				case StandardUICommandKind.Pause:
					Label = Description = "Pause";
					IconSource = new SymbolIconSource { Symbol = Symbol.Pause };
					// No HotKey
					break;

				case StandardUICommandKind.Play:
					Label = Description = "Play";
					IconSource = new SymbolIconSource { Symbol = Symbol.Play };
					// No HotKey
					break;

				case StandardUICommandKind.Stop:
					Label = Description = "Stop";
					IconSource = new SymbolIconSource { Symbol = Symbol.Stop };
					// No HotKey
					break;

				case StandardUICommandKind.Forward:
					Label = "Forward";
					IconSource = new SymbolIconSource { Symbol = Symbol.Forward };
					Description = "Go to the next item";
					// No HotKey
					break;

				case StandardUICommandKind.Backward:
					Label = "Backward";
					IconSource = new SymbolIconSource { Symbol = Symbol.Back };
					Description = "Back";
					// No HotKey
					break;

				case StandardUICommandKind.Undo:
					Label = "Undo";
					IconSource = new SymbolIconSource { Symbol = Symbol.Undo };
					Description = "Reverse the most recent action";
					HotKey = new KeyGesture(Key.Z, KeyModifiers.Control);
					break;

				case StandardUICommandKind.Redo:
					Label = "Redo";
					IconSource = new SymbolIconSource { Symbol = Symbol.Redo };
					Description = "Repeat the most recently undone action";
					HotKey = new KeyGesture(Key.Y, KeyModifiers.Control);
					break;

				default:
					throw new NotImplementedException();
			}
		}
	}
}

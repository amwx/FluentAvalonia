using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace FluentAvalonia.Core.ApplicationModel
{
	public sealed class ApplicationViewTitleBar
	{
		static ApplicationViewTitleBar()
		{
			Instance = new ApplicationViewTitleBar();
		}

		public static ApplicationViewTitleBar Instance { get; }

		internal event EventHandler TitleBarPropertyChanged;

		public Color? BackgroundColor
		{
			get => _backgroundColor;
			set => RaiseAndSetIfChanged(ref _backgroundColor, value);
		}

		public Color? ButtonBackgroundColor
		{
			get => _buttonBackgroundColor;
			set => RaiseAndSetIfChanged(ref _buttonBackgroundColor, value);
		}

		public Color? ButtonForegroundColor
		{
			get => _buttonForegroundColor;
			set => RaiseAndSetIfChanged(ref _buttonForegroundColor, value);
		}

		public Color? ButtonHoverBackgroundColor
		{
			get => _buttonHoverBackgroundColor;
			set => RaiseAndSetIfChanged(ref _buttonHoverBackgroundColor, value);
		}

		public Color? ButtonHoverForegroundColor
		{
			get => _buttonHoverForegroundColor;
			set => RaiseAndSetIfChanged(ref _buttonHoverForegroundColor, value);
		}

		public Color? ButtonInactiveBackgroundColor
		{
			get => _buttonInactiveBackgroundColor;
			set => RaiseAndSetIfChanged(ref _buttonInactiveBackgroundColor, value);
		}

		public Color? ButtonInactiveForegroundColor
		{
			get => _buttonInactiveForegroundColor;
			set => RaiseAndSetIfChanged(ref _buttonInactiveForegroundColor, value);
		}

		public Color? ButtonPressedBackgroundColor
		{
			get => _buttonPressedBackgroundColor;
			set => RaiseAndSetIfChanged(ref _buttonPressedBackgroundColor, value);
		}

		public Color? ButtonPressedForegroundColor
		{
			get => _buttonPressedForegroundColor;
			set => RaiseAndSetIfChanged(ref _buttonPressedForegroundColor, value);
		}

		public Color? ForegroundColor
		{
			get => _foregroundColor;
			set => RaiseAndSetIfChanged(ref _foregroundColor, value);
		}

		public Color? InactiveBackgroundColor
		{
			get => _inactiveBackgroundColor;
			set => RaiseAndSetIfChanged(ref _inactiveBackgroundColor, value);
		}

		public Color? InactiveForegroundColor
		{
			get => _inactiveForegroundColor;
			set => RaiseAndSetIfChanged(ref _inactiveForegroundColor, value);
		}

		private void RaiseAndSetIfChanged<T>(ref T field, T value)
		{
			if (!EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;

				TitleBarPropertyChanged(this, EventArgs.Empty);
			}
		}

		private Color? _backgroundColor;
		private Color? _foregroundColor;

		private Color? _buttonBackgroundColor;
		private Color? _buttonForegroundColor;

		private Color? _buttonHoverBackgroundColor;
		private Color? _buttonHoverForegroundColor;

		private Color? _buttonInactiveBackgroundColor;
		private Color? _buttonInactiveForegroundColor;

		private Color? _buttonPressedBackgroundColor;
		private Color? _buttonPressedForegroundColor;
		
		private Color? _inactiveBackgroundColor;
		private Color? _inactiveForegroundColor;
	}
}

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using System;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// The FlyoutPresenter that is used within a <see cref="PickerFlyoutBase"/>
	/// </summary>
	public class PickerFlyoutPresenter : ContentControl
	{
		public PickerFlyoutPresenter()
		{
			PseudoClasses.Add(":acceptdismiss");
		}

		/// <summary>
		/// Raised when the Confirmed button is tapped indicating the new Color should be applied
		/// </summary>
		public event TypedEventHandler<PickerFlyoutPresenter, object> Confirmed;

		/// <summary>
		/// Raised when the Dismiss button is tapped, indicating the new color should not be applied
		/// </summary>
		public event TypedEventHandler<PickerFlyoutPresenter, object> Dismissed;

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			if (_acceptButton != null)
			{
				_acceptButton.Click -= OnAcceptClick;
			}
			if (_dismissButton != null)
			{
				_dismissButton.Click -= OnDismissClick;
			}

			base.OnApplyTemplate(e);

			_acceptButton = e.NameScope.Find<Button>("AcceptButton");
			if (_acceptButton != null)
			{
				_acceptButton.Click += OnAcceptClick;
			}
			_dismissButton = e.NameScope.Find<Button>("DismissButton");
			if (_dismissButton != null)
			{
				_dismissButton.Click += OnDismissClick;
			}
		}

		private void OnDismissClick(object sender, RoutedEventArgs e)
		{
			Dismissed?.Invoke(this, EventArgs.Empty);
		}

		private void OnAcceptClick(object sender, RoutedEventArgs e)
		{
			Confirmed?.Invoke(this, EventArgs.Empty);
		}

		internal void ShowHideButtons(bool show)
		{
			PseudoClasses.Set(":acceptdismiss", show);
		}

		private Button _acceptButton;
		private Button _dismissButton;
	}
}

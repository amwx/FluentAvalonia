using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls.Primitives
{
	/// <summary>
	/// The base class for a Flyout that allows confirming or dismissing
	/// </summary>
	public abstract class PickerFlyoutBase : FlyoutBase
	{
		/// <summary>
		/// Provides logic that should performed when the confirmed button is tapped
		/// </summary>
		protected abstract void OnConfirmed();

		/// <summary>
		/// Determines if the Accept and Dismiss buttons should be shown
		/// </summary>
		protected virtual bool ShouldShowConfirmationButtons() => true;

        protected override void OnOpening(CancelEventArgs args)
        {
            base.OnOpening(args);

            if (Popup.Child is PickerFlyoutPresenter pfp)
            {
                pfp.ShowHideButtons(ShouldShowConfirmationButtons());
            }
        }
    }
}

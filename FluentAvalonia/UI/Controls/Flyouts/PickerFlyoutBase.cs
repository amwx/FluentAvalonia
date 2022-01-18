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
	}
}

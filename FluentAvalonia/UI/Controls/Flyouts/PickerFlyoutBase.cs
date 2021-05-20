using Avalonia;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls.Primitives
{
	public abstract class PickerFlyoutBase : FlyoutBase
	{
		protected abstract void OnConfirmed();

		protected virtual bool ShouldShowConfirmationButtons() => true;
	}
}

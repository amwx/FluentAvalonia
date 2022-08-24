using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Special control to host a <see cref="ContentDialog"/> or <see cref="TaskDialog"/>
    /// </summary>
    /// <remarks>
    /// This class should generally not be used outside of FluentAvalonia, and is
    /// only public for Xaml styling support
    /// </remarks>
    public class DialogHost : ContentControl, IStyleable
	{
		public DialogHost()
		{
			Background = null;
			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
			VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
		}

		Type IStyleable.StyleKey => typeof(OverlayPopupHost);

		protected override Size MeasureOverride(Size availableSize)
		{
			_ = base.MeasureOverride(availableSize);

			if (VisualRoot is TopLevel tl)
			{
				return tl.ClientSize;
			}
			else if (VisualRoot is IControl c)
			{
				return c.Bounds.Size;
			}

			return Size.Empty;
		}

		protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnAttachedToVisualTree(e);
			if (e.Root is IControl wb)
			{
				// OverlayLayer is a Canvas, so we won't get a signal to resize if the window
				// bounds change. Subscribe to force update
				_rootBoundsWatcher = wb.GetObservable(BoundsProperty).Subscribe(_ => OnRootBoundsChanged());
			}
		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);
			_rootBoundsWatcher?.Dispose();
			_rootBoundsWatcher = null;
		}

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            e.Handled = true;
        }

        // TODO: This is a temporary fix for https://github.com/amwx/FluentAvalonia/issues/110
        // TODO: In long term we need to find a final fix for this
        // protected override void OnKeyDown(KeyEventArgs e)
        // {
        //     e.Handled = true;
        // }
        //
        // protected override void OnKeyUp(KeyEventArgs e)
        // {
        //     e.Handled = true;
        // }

        private void OnRootBoundsChanged()
		{
			InvalidateMeasure();
		}

		private IDisposable _rootBoundsWatcher;
	}
}

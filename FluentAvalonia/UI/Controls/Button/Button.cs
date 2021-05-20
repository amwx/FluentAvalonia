using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using System;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// No super special functionality, normal button but adds :pressed pseudoclass
	/// when interacting with keyboard via enter key
	/// </summary>
    public class Button : Avalonia.Controls.Button, IStyleable
    {
        public Button()
        {
            ClickMode = ClickMode.Release;
        }

        Type IStyleable.StyleKey => typeof(Button);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter | e.Key == Key.Space)
            {
                SetValue(IsPressedProperty, true);
                if (ClickMode == ClickMode.Press)
                {
                    OnClick();

                    e.Handled = true;
                }
                else
                {
                    e.Handled = true;
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter | e.Key == Key.Space)
            {
                SetValue(IsPressedProperty, false);
                PseudoClasses.Set(":pressed", false);
                PseudoClasses.Set(":pointerover", false);
                if (ClickMode == ClickMode.Release)
                {
                    OnClick();

                    e.Handled = true;
                }
                else
                {
                    e.Handled = true;
                }
            }
            base.OnKeyUp(e);
        }

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);
			// if enter is used, sometimes this can get stuck (popup etc)
			// Just make sure we reset it
			SetValue(IsPressedProperty, false);
			PseudoClasses.Set(":pressed", false);
			PseudoClasses.Set(":pointerover", false);
		}
	}
}

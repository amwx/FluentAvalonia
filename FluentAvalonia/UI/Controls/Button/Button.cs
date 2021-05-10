using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
    public class Button : Avalonia.Controls.Button, IStyleable
    {
        public Button()
        {
            ClickMode = Avalonia.Controls.ClickMode.Release;
        }

        #region AvaloniaProperties
        
        public static readonly DirectProperty<Button, FlyoutBase> FlyoutProperty = 
            AvaloniaProperty.RegisterDirect<Button, FlyoutBase>("Flyout",
            (s) => s.Flyout, (s, v) => s.Flyout = v);

        #endregion

        #region CLR Properties

        //Target Button so Button Styles apply to both derived & native button controls
        Type IStyleable.StyleKey => typeof(Avalonia.Controls.Button);

        public FlyoutBase Flyout
        {
            get => _flyout;
            set => SetAndRaise(FlyoutProperty, ref _flyout, value);
        }

        #endregion

        #region Override Methods

        protected override void OnKeyDown(KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter | e.Key == Key.Space)
            {
                SetValue(IsPressedProperty, true);
                if (ClickMode == Avalonia.Controls.ClickMode.Press)
                {
                    OnClick();
                    if (Flyout != null)
                        OpenFlyout();

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
                if (ClickMode == Avalonia.Controls.ClickMode.Release)
                {
                    OnClick();
                    if (Flyout != null)
                        OpenFlyout();

                    e.Handled = true;
                }
                else
                {
                    e.Handled = true;
                }
            }
            base.OnKeyUp(e);
        }


        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (ClickMode == Avalonia.Controls.ClickMode.Press)
                    if (Flyout != null)
                        OpenFlyout();
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if(e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                if(ClickMode == Avalonia.Controls.ClickMode.Release)
                {
                    if (Flyout != null)
                        OpenFlyout();
                }
            }
            base.OnPointerReleased(e);
        }

        #endregion

        #region Private Methods

        protected virtual void OpenFlyout()
        {
            _flyout.ShowAt(this);
        }

        #endregion


        private FlyoutBase _flyout;
    }
}

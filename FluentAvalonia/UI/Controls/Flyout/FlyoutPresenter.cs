//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia.Controls;
using Avalonia.Input;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Basic presenter for a Flyout. No special impl here
    /// Again, not in WinUI, so impl is my own
    /// </summary>
    public class FlyoutPresenter : ContentControl
    {
        //Prevent MouseWheel from propagating into the MainWindow
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            e.Handled = true;
        }
    }
}

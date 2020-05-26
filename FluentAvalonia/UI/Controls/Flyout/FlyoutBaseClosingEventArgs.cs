//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

namespace FluentAvalonia.UI.Controls
{
    public class FlyoutBaseClosingEventArgs
    {
        public bool Cancel { get; set; }

        public FlyoutBaseClosingEventArgs(bool cancel)
        {
            Cancel = cancel;
        }
    }
}

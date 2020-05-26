//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

namespace FluentAvalonia.UI.Controls
{
    public class SplitViewPaneClosingEventArgs
    {
        public bool Cancel { get; set; }

        public SplitViewPaneClosingEventArgs(bool cancel)
        {
            Cancel = cancel;
        }
    }
}

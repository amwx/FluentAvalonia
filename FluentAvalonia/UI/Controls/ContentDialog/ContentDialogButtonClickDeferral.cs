//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a deferral that can be used by an app to respond asyncronously to
    /// a button click event
    /// </summary>
    public sealed class ContentDialogButtonClickDeferral
    {
        internal ContentDialogButtonClickDeferral(ContentDialog owner)
        {
            Owner = owner;
        }

        internal bool IsDeferred { get; set; }

        public void Complete() 
        {
            Owner.CompleteButtonClickDeferral();
        }

        private ContentDialog Owner;
    }
}

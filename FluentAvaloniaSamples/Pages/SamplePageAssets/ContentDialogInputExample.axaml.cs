using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace FluentAvaloniaSamples.Pages.SamplePageAssets
{
    public partial class ContentDialogInputExample : UserControl
    {
        public ContentDialogInputExample()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputField_OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            // We will set the focus into our input field just after it got attached to the visual tree.
            if (sender is InputElement inputElement)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    KeyboardDevice.Instance.SetFocusedElement(inputElement, NavigationMethod.Unspecified,
                        KeyModifiers.None);
                });
            }
        }
    }
}

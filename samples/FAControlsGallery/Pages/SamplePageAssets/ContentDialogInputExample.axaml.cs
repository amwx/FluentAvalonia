using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace FAControlsGallery.Pages.SamplePageAssets;

public partial class ContentDialogInputExample : UserControl
{
    public ContentDialogInputExample()
    {
        InitializeComponent();
    }

    private void InputField_OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        // We will set the focus into our input field just after it got attached to the visual tree.
        if (sender is InputElement inputElement)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                inputElement.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            });
        }
    }
}

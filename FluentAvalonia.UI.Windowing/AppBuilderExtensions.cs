using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

namespace FluentAvalonia.UI.Windowing;

public static class AppBuilderExtensions
{
    public static AppBuilder UseFAWindowing(this AppBuilder ab)
    {
        AvaloniaLocator.CurrentMutable.Bind<IFAWindowProvider>().ToSingleton<FAWindowProvider>();
        return ab;
    }
}

internal class FAWindowProvider : IFAWindowProvider
{
    public Window CreateTaskDialogHost(TaskDialog dialog)
    {
        return new TaskDialogWindowHost(dialog);
    }

    private class TaskDialogWindowHost : AppWindow
    {
        public TaskDialogWindowHost(TaskDialog dialog)
        {
            CanResize = false;
            SizeToContent = Avalonia.Controls.SizeToContent.WidthAndHeight;
            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner;
            ShowAsDialog = true;

            Content = dialog;
            MinWidth = 100;
            MinHeight = 100;

#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Don't allow closing the window when a Deferral is active
            // Otherwise the deferral task will continue to run, but the 
            // window will be dismissed
            if (Content is TaskDialog td && td._hasDeferralActive)
                e.Cancel = true;
        }
    }
}

using System;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaTests.ControlTests;

internal class RepeaterTestContext : IDisposable
{
    public RepeaterTestContext()
    {
        ItemsRepeater = new FAItemsRepeater();
        ScrollViewer = new ScrollViewer
        {
            Content = ItemsRepeater
        };
        Window = new Window
        {
            Content = ScrollViewer
        };

        Window.Show();
        Window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    public Window Window { get; }

    public ScrollViewer ScrollViewer { get; }

    public FAItemsRepeater ItemsRepeater { get; }

    public void Dispose()
    {
        Window.Close();
    }
}

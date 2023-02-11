using System;
using FluentAvaloniaSamples.Services;
using FluentAvaloniaSamples.Utilities;

namespace FluentAvaloniaSamples.ViewModels;

public class CoreControlsGroupItem : ViewModelBase
{
    public CoreControlsGroupItem()
    {
        InvokeCommand = new FACommand(OnInvokeCommandExecute);
    }

    public string IconResourceKey { get; init; }

    public string Header { get; init; }

    public string Description { get; init; }

    public bool Navigates { get; init; }

    public string PageType { get; init; }

    public FACommand InvokeCommand { get; }

    private void OnInvokeCommandExecute(object parameter)
    {
        var type = Type.GetType($"FluentAvaloniaSamples.Pages.{PageType}");
        NavigationService.Instance.Navigate(type);
    }
}

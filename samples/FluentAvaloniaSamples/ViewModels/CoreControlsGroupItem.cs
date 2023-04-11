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

    public string IconResourceKey { get; set; }

    public string Header { get; set; }

    public string Description { get; set; }

    public bool Navigates { get; set; }

    public string PageType { get; set; }

    public FACommand InvokeCommand { get; }

    private void OnInvokeCommandExecute(object parameter)
    {
        var type = Type.GetType($"FluentAvaloniaSamples.Pages.{PageType}");
        NavigationService.Instance.Navigate(type);
    }
}

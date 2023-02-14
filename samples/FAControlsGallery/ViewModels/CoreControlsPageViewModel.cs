using System.Text.Json;
using FAControlsGallery.Services;

namespace FAControlsGallery.ViewModels;

public class CoreControlsPageViewModel : MainPageViewModelBase
{
    public CoreControlsPageViewModel(string coreControlsJson)
    {
        NoteText = "Fluent v2 styling on Avalonia controls. Only controls that have styling changes" +
            "are shown here for demo purposes, though all controls (except ContextMenu) are supported.";

        CoreControlGroups = JsonSerializer.Deserialize<List<PageBaseViewModel>>(coreControlsJson);

        for (int i = 0, ct = CoreControlGroups.Count; i < ct; i++)
        {
            CoreControlGroups[i].Parent = this;
        }
    }

    public List<PageBaseViewModel> CoreControlGroups { get; }

    public string NoteText { get; }
}


public class CoreControlsGroupItem : ViewModelBase
{
    public CoreControlsGroupItem()
    {
        InvokeCommand = new FACommand(OnInvokeCommandExecute);
    }

    public string IconResourceKey { get; init; }

    public string Header { get; init; }

    public string Description { get; init; }

    public string PageType { get; init; }

    public FACommand InvokeCommand { get; }

    private void OnInvokeCommandExecute(object parameter)
    {
        var type = Type.GetType($"FAControlsGallery.Pages.{PageType}");
        NavigationService.Instance.Navigate(type);
    }
}

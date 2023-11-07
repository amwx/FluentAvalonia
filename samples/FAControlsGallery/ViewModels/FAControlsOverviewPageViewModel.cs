using System.Text.Json;

namespace FAControlsGallery.ViewModels;

public class FAControlsOverviewPageViewModel : MainPageViewModelBase
{
    public FAControlsOverviewPageViewModel(string controlsJson)
    {
        ControlGroups = JsonSerializer.Deserialize(controlsJson, FAControlsJsonSerializerContext.Default.ListFAControlsGroupItem);

        for (int i = 0; i < ControlGroups.Count; i++)
        {
            for (int j = 0; j < ControlGroups[i].Controls.Count; j++)
            {
                ControlGroups[i].Controls[j].Parent = this;
            }
        }
    }

    public List<FAControlsGroupItem> ControlGroups { get; }
}

public class FAControlsGroupItem
{
    public string Header { get; set; }

    public List<FAControlsPageItem> Controls { get; init; }
}

public class FAControlsPageItem : PageBaseViewModel
{
    public string Namespace { get; init; }

    public string WinUINamespace { get; init; }

    public string WinUIDocsLink { get; init; }

    public string WinUIGuidelinesLink { get; init; }
}

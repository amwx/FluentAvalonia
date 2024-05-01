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

    public List<FAControlsPageItem> Controls { get; set; }
}

public class FAControlsPageItem : PageBaseViewModel
{
    public string Namespace { get; set; }

    public string WinUINamespace { get; set; }

    public string WinUIDocsLink { get; set; }

    public string WinUIGuidelinesLink { get; set; }
}

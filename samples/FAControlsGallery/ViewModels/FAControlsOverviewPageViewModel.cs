using System.Text.Json;
using FAControlsGallery.Services;

namespace FAControlsGallery.ViewModels;

public class FAControlsOverviewPageViewModel : MainPageViewModelBase
{
    public FAControlsOverviewPageViewModel()
    {
        ControlGroups = ControlInformation.GetFAControlInfo();

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

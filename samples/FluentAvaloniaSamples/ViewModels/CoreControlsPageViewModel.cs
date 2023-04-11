using System.Collections.Generic;
using System.Text.Json;

namespace FluentAvaloniaSamples.ViewModels;

public class CoreControlsPageViewModel : ViewModelBase
{
    public CoreControlsPageViewModel()
    {
        var coreControls = GetAssemblyResource("avares://FluentAvaloniaSamples/Assets/CoreControlsGroups.json");
        CoreControlGroups = JsonSerializer.Deserialize(coreControls, FASampleJsonContext.Default.ListCoreControlsGroupItem);
    }

    public string PageHeader => "Core Controls";

    public List<CoreControlsGroupItem> CoreControlGroups { get; }
}

using System.Collections.Generic;
using System.Text.Json;

namespace FluentAvaloniaSamples.ViewModels;

public class NewControlsPageViewModel : ViewModelBase
{
    public NewControlsPageViewModel()
    {
        var controls = GetAssemblyResource("avares://FluentAvaloniaSamples/Assets/FAControlsGroups.json");
        ControlGroups = JsonSerializer.Deserialize<List<FAControlsGroupItem>>(controls);
    }

    public string PageHeader => "New Controls in FluentAvalonia";

    public List<FAControlsGroupItem> ControlGroups { get; }
}

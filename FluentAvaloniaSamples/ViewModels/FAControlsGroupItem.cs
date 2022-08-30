using System.Collections.Generic;

namespace FluentAvaloniaSamples.ViewModels;

public class FAControlsGroupItem
{
    public string Header { get; set; }

    public List<FAControlsItem> Controls { get; init; }
}

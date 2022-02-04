using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
    public class CoreControlsPageViewModel : ViewModelBase
    {
        public CoreControlsPageViewModel()
        {
            var coreControls = GetAssemblyResource("avares://FluentAvaloniaSamples/Assets/CoreControlsGroups.json");
            CoreControlGroups = JsonSerializer.Deserialize<List<CoreControlsGroupItem>>(coreControls);
        }

        public string PageHeader => "Core Controls";

        public List<CoreControlsGroupItem> CoreControlGroups { get; }
    }
}

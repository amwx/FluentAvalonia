using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
    public class FAControlsGroupItem
    {
        public string Header { get; set; }

        public List<FAControlsItem> Controls { get; init; }
    }
}

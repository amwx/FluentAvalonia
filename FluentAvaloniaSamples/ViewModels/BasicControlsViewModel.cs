using System.Collections.Generic;

namespace FluentAvaloniaSamples.ViewModels
{
    public class BasicControlsViewModel : ViewModelBase
    {
        public BasicControlsViewModel()
        {
            ComboBoxItems = new List<string> { "Item 1", "Item 2", "Item 3" };            
        }

		public string CoreControlsHeader => DescriptionServiceProvider.Instance.GetInfo("Core Controls", "Header");

        public List<string> ComboBoxItems { get; }        
    }
}

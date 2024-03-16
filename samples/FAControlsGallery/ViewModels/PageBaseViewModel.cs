using FAControlsGallery.Services;

namespace FAControlsGallery.ViewModels;

public class PageBaseViewModel : ViewModelBase
{
    public PageBaseViewModel()
    {
        InvokeCommand = new FACommand(PageInvoked);
    }

    public MainPageViewModelBase Parent { get; set; }

    public string Header { get; set; }

    public string Description { get; set; }

    public string IconResourceKey { get; set; }

    public string PageKey { get; set; }

    public string[] SearchKeywords { get; set; }

    public FACommand InvokeCommand { get; }

    private void PageInvoked(object param)
    {
        NavigationService.Instance.NavigateFromContext(this);
    }
}

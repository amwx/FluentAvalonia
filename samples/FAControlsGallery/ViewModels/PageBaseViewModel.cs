using FAControlsGallery.Services;

namespace FAControlsGallery.ViewModels;

public class PageBaseViewModel : ViewModelBase
{
    public PageBaseViewModel()
    {
        InvokeCommand = new FACommand(PageInvoked);
    }

    public MainPageViewModelBase Parent { get; set; }

    public string Header { get; init; }

    public string Description { get; init; }

    public string IconResourceKey { get; init; }

    public string PageKey { get; init; }

    public string[] SearchKeywords { get; init; }

    public FACommand InvokeCommand { get; }

    private void PageInvoked(object param)
    {
        NavigationService.Instance.NavigateFromContext(this);
    }
}

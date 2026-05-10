namespace FAControlsGallery.ViewModels;

public class InfoBadgePageViewModel : ViewModelBase
{
    public bool IsInfoBadgeEnabled
    {
        get;
        set
        {
            if (RaiseAndSetIfChanged(ref field, value))
            {
                RaisePropertyChanged(nameof(InfoBadgeOpacity));
            }
        }
    } = true;

    public double InfoBadgeOpacity => IsInfoBadgeEnabled ? 1d : 0d;
}

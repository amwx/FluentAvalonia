namespace FAControlsGallery.ViewModels;

public class InfoBadgePageViewModel : ViewModelBase
{
    public bool IsInfoBadgeEnabled
    {
        get => _isEnabled;
        set
        {
            if (RaiseAndSetIfChanged(ref _isEnabled, value))
            {
                RaisePropertyChanged(nameof(InfoBadgeOpacity));
            }
        }
    }

    public double InfoBadgeOpacity => IsInfoBadgeEnabled ? 1d : 0d;

    private bool _isEnabled = true;
}

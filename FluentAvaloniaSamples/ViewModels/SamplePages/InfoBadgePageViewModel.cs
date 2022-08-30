namespace FluentAvaloniaSamples.ViewModels;

public class InfoBadgePageViewModel : ViewModelBase
{
    public bool IsInfoBarEnabled
    {
        get => _isEnabled;
        set
        {
            if (RaiseAndSetIfChanged(ref _isEnabled, value))
            {
                RaisePropertyChanged(nameof(InfoBarOpacity));
            }
        }
    }

    public double InfoBarOpacity => IsInfoBarEnabled ? 1d : 0d;

    private bool _isEnabled = true;
}

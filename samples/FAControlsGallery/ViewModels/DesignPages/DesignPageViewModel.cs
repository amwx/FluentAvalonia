namespace FAControlsGallery.ViewModels.DesignPages;

public sealed class DesignPageViewModel : MainPageViewModelBase
{
    public int LastSelectedIndex { get; set; } = -1;

    public int CurrentIndex
    {
        get => _currentIndex;
        set => RaiseAndSetIfChanged(ref _currentIndex, value);
    }

    private int _currentIndex = 0;

    public const string TypographyKey = "Typography Design Guidelines";
    public const string IconsKey = "Icons Design Guidelines";
    public const string ColorsKey = "Color Design Guidelines";
}

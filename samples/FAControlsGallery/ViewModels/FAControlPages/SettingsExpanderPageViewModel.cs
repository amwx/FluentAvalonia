namespace FAControlsGallery.ViewModels;

public class SettingsExpanderPageViewModel : ViewModelBase
{
    public SettingsExpanderPageViewModel()
    {
        Items = new List<SettingsItemBase>
        {
            new RecentImagesSettingsItem() 
            { 
                Header = "Recent images",
            },
            new FooterButtonSettingsItem() 
            { 
                Header = "Choose a photo" 
            }
        };
    }

    public List<SettingsItemBase> Items { get; }
}

public class SettingsItemBase
{
    public string Header { get; set; }
}

public class RecentImagesSettingsItem : SettingsItemBase
{
    public RecentImagesSettingsItem()
    {
        Images = new List<string>
        {
            "AppWindowIcon",
            "DatePageIcon",
            "FrameIcon",
            "TextPageIcon",
        };
    }

    public List<string> Images { get; }
}

public class FooterButtonSettingsItem : SettingsItemBase
{
    public FooterButtonSettingsItem()
    {
        Footer = "Choose image...";
    }

    public object Footer { get; set; }
}

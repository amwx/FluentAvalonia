namespace FAControlsGallery.ViewModels;

public class SettingsExpanderPageViewModel : ViewModelBase
{
    public SettingsExpanderPageViewModel()
    {
        Items = new List<SettingsItemBase>
        {
            new RecentImagesSettingsItem() { Header = "Recent images" },
            new ChooseImageSettingsItem() { Header = "Choose a photo" }
        };
    }

    public List<SettingsItemBase> Items { get; }
}

public class SettingsItemBase
{
    public string Header { get; set; }

    public object Footer { get; set; }
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

public class ChooseImageSettingsItem : SettingsItemBase
{
    public ChooseImageSettingsItem()
    {
        Footer = new SettingsItemButtonFooter { Caption = "Choose image..." };
    }
}

public class SettingsItemButtonFooter
{
    public string Caption { get; set; }
}

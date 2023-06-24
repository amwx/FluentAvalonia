namespace FAControlsGallery.ViewModels;

public class FAComboBoxPageViewModel : ViewModelBase
{
    public FAComboBoxPageViewModel()
    {
        Items = new List<ComboItem>
        {
            new ComboItem("Red"),
            new ComboItem("Orange"),
            new ComboItem("Yellow"),
            new ComboItem("Green"),
            new ComboItem("Blue"),
            new ComboItem("Indigo"),
            new ComboItem("Violet"),
            new ComboItem("White"),
            new ComboItem("Black"),
        };
    }

    public List<ComboItem> Items { get; }
}

public class ComboItem
{
    public ComboItem(string name)
    {
        DisplayName = name;
    }

    public string DisplayName { get; }
}

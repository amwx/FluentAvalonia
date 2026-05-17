using Avalonia.Collections;
using FluentAvalonia.UI.Input;

namespace FAControlsGallery.ViewModels;

public class StandardUICommandPageViewModel : ViewModelBase
{
    public StandardUICommandPageViewModel()
    {
        var coms = Enum.GetValues<FAStandardUICommandKind>();
        StandardCommands = new List<StandardCommandItem>(coms.Length);
        foreach (var item in coms)
        {
            if (item == FAStandardUICommandKind.None)
                continue;

            StandardCommands.Add(new StandardCommandItem
            {
                Name = item.ToString(),
                Command = new FAStandardUICommand(item)
            });
        }

        TempItems = new AvaloniaList<string>(10);
        for (int i = 0; i < 10; i++)
        {
            TempItems.Add($"Temp item {i + 1}");
        }
    }

    public IList<StandardCommandItem> StandardCommands { get; }

    public AvaloniaList<string> TempItems { get; set; }

    public void DeleteItem(object param)
    {
        if (param != null)
        {
            TempItems.Remove(param.ToString());
        }
    }

    public void AddItem()
    {
        TempItems.Add($"New Item {++_addCounter}");
    }

    private int _addCounter = 0;
}

public class StandardCommandItem
{
    public string Name { get; set; }
    public FAStandardUICommand Command { get; set; }
}

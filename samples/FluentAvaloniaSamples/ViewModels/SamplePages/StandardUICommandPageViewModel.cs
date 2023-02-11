using System;
using System.Collections.Generic;
using Avalonia.Collections;
using FluentAvalonia.UI.Input;

namespace FluentAvaloniaSamples.ViewModels;

public class StandardUICommandPageViewModel : ViewModelBase
{
    public StandardUICommandPageViewModel()
    {
        var coms = Enum.GetValues<StandardUICommandKind>();
        StandardCommands = new List<StandardCommandItem>(coms.Length);
        foreach (var item in coms)
        {
            if (item == StandardUICommandKind.None)
                continue;

            StandardCommands.Add(new StandardCommandItem
            {
                Name = item.ToString(),
                Command = new StandardUICommand(item)
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
    public StandardUICommand Command { get; set; }
}

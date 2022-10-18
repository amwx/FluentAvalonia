using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

public class SettingsExpanderItemContainerGenerator : ItemContainerGenerator<SettingsExpanderItem>
{
    public SettingsExpanderItemContainerGenerator(IControl owner, AvaloniaProperty contentProperty, AvaloniaProperty contentTemplateProperty) 
        : base(owner, contentProperty, contentTemplateProperty)
    {
    }

    protected override IControl CreateContainer(object item)
    {
        if (item is SettingsExpanderItem expItem)
            return expItem;

        var tItem = Owner.FindDataTemplate(item, ItemTemplate)?.Build(item);

        // If DataTemplate built us a SEI, return that
        if (tItem is SettingsExpanderItem)
        {
            tItem.DataContext = item;
            return tItem;
        }

        expItem = new SettingsExpanderItem();
        expItem.DataContext = item;
        expItem.Content = item;
        expItem.ContentTemplate = ItemTemplate;

        return expItem;
    }
}

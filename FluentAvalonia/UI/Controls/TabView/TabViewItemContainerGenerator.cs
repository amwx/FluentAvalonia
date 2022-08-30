using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

internal class TabViewItemContainerGenerator : ItemContainerGenerator<TabViewItem>
{
    public TabViewItemContainerGenerator(IControl owner, AvaloniaProperty contentProperty, AvaloniaProperty contentTemplateProperty)
        : base(owner, contentProperty, contentTemplateProperty)
    {
    }

    protected override IControl CreateContainer(object item)
    {
        if (item is TabViewItem tvi)
            return tvi;

        var template = Owner.FindDataTemplate(item, ItemTemplate);
        if (template != null)
        {
            var built = template.Build(item);

            if (built is TabViewItem builtTVI)
            {
                builtTVI.DataContext = item;
                return builtTVI;
            }
        }

        throw new NotSupportedException("Unable to build TabViewItem with given template. Ensure DataTemplate builds a TabViewItem.");
    }
}

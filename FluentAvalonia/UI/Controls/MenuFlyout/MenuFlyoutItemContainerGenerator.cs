using Avalonia.Controls.Generators;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System.ComponentModel;
using Avalonia.Data;

namespace FluentAvalonia.UI.Controls;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class MenuFlyoutPresenterItemContainerGenerator : ItemContainerGenerator
{
    public MenuFlyoutPresenterItemContainerGenerator(IControl owner)
        : base(owner) { }

    protected override IControl CreateContainer(object item)
    {
        if (item == null)
            return null;

        if (item is MenuFlyoutItemBase mfib)
            return mfib;

        if (Owner is FAMenuFlyoutPresenter presenter)
        {
            var template = Owner.FindDataTemplate(item, ItemTemplate);

            if (template is MenuFlyoutSubItemTemplate mfsit)
            {
                var mfsi = new MenuFlyoutSubItem();

                var itemsSelector = mfsit.ItemsSelector(item);
                if (itemsSelector != null)
                {
                    BindingOperations.Apply(mfsi, MenuFlyoutSubItem.ItemsProperty, itemsSelector, null);
                }

                var textSelector = mfsit.TextSelector(item);
                if (textSelector != null)
                {
                    BindingOperations.Apply(mfsi, MenuFlyoutSubItem.TextProperty, textSelector, null);
                }

                var iconSelector = mfsit.IconSelector(item);
                if (iconSelector != null)
                {
                    BindingOperations.Apply(mfsi, MenuFlyoutSubItem.IconProperty, iconSelector, null);
                }

                return mfsi;
            }
            else
            {
                // Other templates will do whatever
                var builtContent = template.Build(item);

                if (builtContent is MenuFlyoutItemBase mfibFromTemplate)
                {
                    mfibFromTemplate.DataContext = item;
                    return mfibFromTemplate;
                }
            }
        }

        // If we reach here, just create a normal MenuFlyoutItem b/c we don't have enough
        // information on how to construct it.
        return new MenuFlyoutItem
        {
            DataContext = item,
            Text = item.ToString()
        };
    }
}

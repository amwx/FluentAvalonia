using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

internal class BreadcrumbElementFactory : FAElementFactory
{
    public void UserElementFactory(object newValue)
    {
        if (newValue is IDataTemplate template)
        {
            _itemTemplateWrapper = new FAItemTemplateWrapper(template);
        }
        else if (newValue is FADataTemplateSelector dts)
        {
            _itemTemplateWrapper = new FAItemTemplateWrapper(dts);
        }
        else if (newValue is IFAElementFactory ef)
        {
            _itemTemplateWrapper = ef;
        }
    }

    protected override Control GetElementCore(FAElementFactoryGetArgs args)
    {
        var newContent = GetNewContent(_itemTemplateWrapper, args);

        // Element is already a BreadcrumbBarItem, so we just return it.
        if (newContent is FABreadcrumbBarItem bcbItem)
        {
            // When the list has not changed the returned item is still a BreadcrumbBarItem but the
            // item is not reset, so we set the content here
            bcbItem.Content = args.Data;
            return bcbItem;
        }

        var newItem = new FABreadcrumbBarItem
        {
            Content = newContent
        };

        // If a user provided item template exists, we pass the template down
        // to the ContentPresenter of the BreadcrumbBarItem.
        if (_itemTemplateWrapper is FAItemTemplateWrapper wrapper)
        {
            newItem.ContentTemplate = wrapper.Template;
        }

        return newItem;

        static object GetNewContent(IFAElementFactory factory, FAElementFactoryGetArgs args0)
        {
            if (args0.Data is FABreadcrumbBarItem item)
            {
                return item;
            }

            if (factory != null)
            {
                return factory.GetElement(args0);
            }

            return args0.Data;
        }
    }

    protected override void RecycleElementCore(FAElementFactoryRecycleArgs args)
    {
        if (args.Element is Control c)
        {
            bool isEllipsisDropDownItem = false; // Use of isEllipsisDropDownItem is workaround for
            // crashing bug when attempting to show ellipsis dropdown after clicking one of its items.

            if (c is FABreadcrumbBarItem bcbItem)
            {
                bcbItem.ResetVisualProperties();
                isEllipsisDropDownItem = bcbItem.IsEllipsisDropDownItem();
            }

            if (_itemTemplateWrapper != null && isEllipsisDropDownItem)
            {
                _itemTemplateWrapper.RecycleElement(args);
            }
        }
    }

    private IFAElementFactory _itemTemplateWrapper;
}

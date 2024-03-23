using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;

namespace FluentAvalonia.UI.Controls;

internal class ItemTemplateWrapper : IElementFactory
{
    public ItemTemplateWrapper(IDataTemplate template)
    {
        _dataTemplate = template;
    }

    public ItemTemplateWrapper(DataTemplateSelector selector)
    {
        _dataTemplateSelector = selector;
    }

    public IDataTemplate Template
    {
        get => _dataTemplate;
        set => _dataTemplate = value;
    }

    public DataTemplateSelector TemplateSelector
    {
        get => _dataTemplateSelector;
        set => _dataTemplateSelector = value;
    }

    public Control GetElement(ElementFactoryGetArgs args)
    {
        var selectedTemplate = _dataTemplate ?? _dataTemplateSelector?.SelectTemplate(args.Data);

        if (selectedTemplate == null)
        {
            // Null template, use other SelectTemplate method
            try
            {
                selectedTemplate = _dataTemplateSelector.SelectTemplate(args.Data, null);
            }
            catch
            {
                // The default implementation of SelectTemplate(IInspectable item, ILayout container) throws invalid arg for null container
                // To not force everbody to provide an implementation of that, catch that here
                // Me: IGNORE
            }

            if (selectedTemplate == null)
            {
                // Still nullptr, fail with a reasonable message now.
                throw new InvalidOperationException("Null encountered as data template. That is not a valid value for a data template, and can not be used.");
            }
        }

        var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
        Control element = null;

        if (recyclePool != null)
        {
            // try to get an element from the recycle pool.
            element = recyclePool.TryGetElement(string.Empty, args.Parent);
        }

        if (element == null)
        {
            // no element was found in recycle pool, create a new element
            element = selectedTemplate.Build(args.Data);

            // Template returned null, so insert empty element to render nothing
            if (element == null)
            {
                var rectangle = new Rectangle();
                element = rectangle;
            }

            // Associate template with element
            element.SetValue(RecyclePool.OriginTemplateProperty, selectedTemplate);
        }

        return element;
    }

    public void RecycleElement(ElementFactoryRecycleArgs args)
    {
        var element = args.Element;
        var selectedTemplate = _dataTemplate ??
            element.GetValue(RecyclePool.OriginTemplateProperty);
        var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
        if (recyclePool == null)
        {
            // No Recycle pool in the template, create one.
            recyclePool = new RecyclePool();
            RecyclePool.SetPoolInstance(selectedTemplate, recyclePool);
        }

        recyclePool.PutElement(args.Element, string.Empty, args.Parent);
    }

    bool IDataTemplate.Match(object data)
    {
        throw new NotImplementedException();
    }

    Control ITemplate<object, Control>.Build(object param)
    {
        throw new NotImplementedException();
    }

    private IDataTemplate _dataTemplate;
    private DataTemplateSelector _dataTemplateSelector;
}

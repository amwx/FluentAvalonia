#pragma warning disable
// Note this class has no documentation yet from Microsoft - disabling the warnings around
// public APIs with no documentation
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public class FARecyclingElementFactory : FAElementFactory
{
    public FARecyclingElementFactory()
    {
        Templates = new Dictionary<string, IDataTemplate>();
    }

    public FARecyclePool RecyclePool { get; set; }

    public IDictionary<string, IDataTemplate> Templates { get; set; }

    public event TypedEventHandler<FARecyclingElementFactory, FASelectTemplateEventArgs> SelectTemplateKey;

    protected virtual string OnSelectTemplateKeyCore(object dataContext, Control owner)
    {
        _args ??= new FASelectTemplateEventArgs();

        _args.TemplateKey = null;
        _args.DataContext = dataContext;
        _args.Owner = owner;

        SelectTemplateKey?.Invoke(this, _args);

        var templateKey = _args.TemplateKey;
        if (string.IsNullOrEmpty(templateKey))
            throw new InvalidOperationException("Please provide a valid template identifier in the handler for the SelectTemplateKey event.");

        return templateKey;
    }

    protected override Control GetElementCore(FAElementFactoryGetArgs args)
    {
        if (Templates == null || Templates.Count == 0)
            throw new InvalidOperationException("Templates property cannot be null or empty.");

        var winrtOwner = args.Parent;
        var templateKey = Templates.Count == 1 ?
            Templates.First().Key :
            OnSelectTemplateKeyCore(args.Data, winrtOwner);

        if (string.IsNullOrEmpty(templateKey))
        {
            // Note: We could allow null/whitespace, which would work as long as
            // the recycle pool is not shared. in order to make this work in all cases
            // currently we validate that a valid template key is provided.
            throw new InvalidOperationException("Template key cannot be empty or null.");
        }

        var element = RecyclePool.TryGetElement(templateKey, winrtOwner);

        if (element == null)
        {
            // No need to call HasKey if there is only one template.
            if (Templates.Count > 1 && !Templates.ContainsKey(templateKey))
                throw new InvalidOperationException(
                    $"No templates of key {templateKey} were found in the templates collection");

            var dataTemplate = Templates[templateKey];
            element = dataTemplate.Build(args.Data);

            FARecyclePool.SetReuseKey(element, templateKey);
        }

        return element;
    }

    protected override void RecycleElementCore(FAElementFactoryRecycleArgs args)
    {
        var element = args.Element;
        var key = FARecyclePool.GetReuseKey(element);
        RecyclePool.PutElement(element, key, args.Parent);
    }

    private FASelectTemplateEventArgs _args;
}

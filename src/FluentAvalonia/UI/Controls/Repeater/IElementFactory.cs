using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

public interface IElementFactory : IDataTemplate
{
    Control GetElement(ElementFactoryGetArgs args);

    void RecycleElement(ElementFactoryRecycleArgs args);
}

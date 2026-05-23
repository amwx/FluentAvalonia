using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines the automation peer for a <see cref="FABreadcrumbBarItem"/>
/// </summary>
public class FABreadcrumbBarItemAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    public FABreadcrumbBarItemAutomationPeer(Control owner) 
        : base(owner)
    {
    }

    protected override string GetLocalizedControlTypeCore()
    {
        return FALocalizationHelper.Instance
            .GetLocalizedStringResource("BreadcrumbBarItemLocalizedControlType");
    }

    protected override string GetClassNameCore()
    {
        return nameof(FABreadcrumbBarItem);
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => 
        AutomationControlType.Button;

    private FABreadcrumbBarItem GetImpl() => Owner as FABreadcrumbBarItem;

    void IInvokeProvider.Invoke()
    {
        if (GetImpl() is FABreadcrumbBarItem item)
            item.OnClickEvent(null, null);
    }
}

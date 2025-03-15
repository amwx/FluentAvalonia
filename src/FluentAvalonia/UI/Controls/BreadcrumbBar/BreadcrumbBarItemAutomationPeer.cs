using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class BreadcrumbBarItemAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    public BreadcrumbBarItemAutomationPeer(Control owner) 
        : base(owner)
    {
    }

    protected override string GetLocalizedControlTypeCore()
    {
        // TODO:  return ResourceAccessor::GetLocalizedStringResource(SR_BreadcrumbBarItemLocalizedControlType);
        return base.GetLocalizedControlTypeCore();
    }

    protected override string GetClassNameCore()
    {
        return nameof(BreadcrumbBarItem);
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => 
        AutomationControlType.Button;

    private BreadcrumbBarItem GetImpl() => Owner as BreadcrumbBarItem;

    void IInvokeProvider.Invoke()
    {
        if (GetImpl() is BreadcrumbBarItem item)
            item.OnClickEvent(null, null);
    }
}

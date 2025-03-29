using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines the automation peer for a <see cref="BreadcrumbBarItem"/>
/// </summary>
public class BreadcrumbBarItemAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    public BreadcrumbBarItemAutomationPeer(Control owner) 
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

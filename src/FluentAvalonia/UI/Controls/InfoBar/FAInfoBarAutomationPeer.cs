using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the AutomationPeer for a <see cref="FAInfoBar"/>
/// </summary>
public sealed class FAInfoBarAutomationPeer : ControlAutomationPeer
{
    public FAInfoBarAutomationPeer(Control owner) 
        : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() =>
        AutomationControlType.StatusBar;

    protected override string GetClassNameCore() => nameof(FAInfoBar);

    internal void RaiseOpenedEvent(FAInfoBarSeverity severity, string displayString)
    {
        // Not sure how WinUI translates here
    }

    internal void RaiseClosedEvent(FAInfoBarSeverity severity, string displayString)
    {
        // Not sure how WinUI translates here
    }
}

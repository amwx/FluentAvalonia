using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represets the <see cref="AutomationPeer"/> for a <see cref="FATabView"/>
/// </summary>
public sealed class FATabViewAutomationPeer : ControlAutomationPeer, ISelectionProvider
{
    public FATabViewAutomationPeer(Control owner)
        : base(owner)
    {
    }

    public bool CanSelectMultiple => false;

    public bool IsSelectionRequired => true;
        
    protected override string GetClassNameCore() => nameof(FATabView);

    protected override AutomationControlType GetAutomationControlTypeCore() =>
        AutomationControlType.Tab;

    public IReadOnlyList<AutomationPeer> GetSelection()
    {
        if (Owner is FATabView tv)
        {
            if (tv.ContainerFromIndex(tv.SelectedIndex) is FATabViewItem tvi)
            {
                return new AutomationPeer[] { ControlAutomationPeer.CreatePeerForElement(tvi) };
            }
        }

        return Array.Empty<AutomationPeer>();
    }
}

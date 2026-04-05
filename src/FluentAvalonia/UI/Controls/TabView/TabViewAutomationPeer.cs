using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represets the <see cref="AutomationPeer"/> for a <see cref="TabView"/>
/// </summary>
public sealed class TabViewAutomationPeer : ControlAutomationPeer, ISelectionProvider
{
    public TabViewAutomationPeer(Control owner)
        : base(owner)
    {
    }

    public bool CanSelectMultiple => false;

    public bool IsSelectionRequired => true;
        
    protected override string GetClassNameCore() => nameof(TabView);

    protected override AutomationControlType GetAutomationControlTypeCore() =>
        AutomationControlType.Tab;

    public IReadOnlyList<AutomationPeer> GetSelection()
    {
        if (Owner is TabView tv)
        {
            if (tv.ContainerFromIndex(tv.SelectedIndex) is TabViewItem tvi)
            {
                return new AutomationPeer[] { ControlAutomationPeer.CreatePeerForElement(tvi) };
            }
        }

        return Array.Empty<AutomationPeer>();
    }
}

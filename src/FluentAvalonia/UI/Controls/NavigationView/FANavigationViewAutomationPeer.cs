using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public sealed class FANavigationViewAutomationPeer : ControlAutomationPeer, ISelectionProvider
{
    public FANavigationViewAutomationPeer(Control owner) 
        : base(owner)
    {
    }

    public bool CanSelectMultiple => false;
    public bool IsSelectionRequired => false;

    public IReadOnlyList<AutomationPeer> GetSelection()
    {
        if (Owner is FANavigationView nv)
        {
            var nvi = nv.GetSelectedContainer();
            var peer = ControlAutomationPeer.CreatePeerForElement(nvi);
            return new[] { peer };
        }

        return null;
    }

    internal void RaiseSelectionChangedEvent(object oldSelection, object newSelection)
    {
        if (Owner is FANavigationView nv && nv.GetSelectedContainer() is FANavigationViewItem nvi)
        {
            var peer = CreatePeerForElement(nvi);
            peer.RaisePropertyChangedEvent(SelectionPatternIdentifiers.SelectionProperty, oldSelection, newSelection);
        }
    }
}

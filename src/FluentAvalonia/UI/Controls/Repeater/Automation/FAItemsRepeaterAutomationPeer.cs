using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

public class FAItemsRepeaterAutomationPeer : ControlAutomationPeer
{
    public FAItemsRepeaterAutomationPeer(Control owner) 
        : base(owner)
    {
    }

    public new FAItemsRepeater Owner => (FAItemsRepeater)base.Owner;

    protected override IReadOnlyList<AutomationPeer> GetChildrenCore()
    {
        var repeater = Owner;
        var childrenPeers = base.GetChildrenCore();
        var peerCount = childrenPeers.Count;

        List<(int, AutomationPeer)> realizedPeers = new List<(int, AutomationPeer)>(peerCount);

        for (int i = 0; i < peerCount; i++)
        {
            var childPeer = childrenPeers[i];
            if (GetElement((ControlAutomationPeer)childPeer, repeater) is Control c)
            {
                if (FAItemsRepeater.GetVirtualizationInfo(c) is VirtualizationInfo vi && vi.IsRealized)
                {
                    realizedPeers.Add((vi.Index, childPeer));
                }
            }
        }

        realizedPeers.Sort((lhs, rhs) => lhs.Item1 < rhs.Item1 ? 1 : -1);

        return realizedPeers.Select(x => x.Item2).ToArray();
    }

    protected override AutomationControlType GetAutomationControlTypeCore() =>
        AutomationControlType.Group;

    private static Control GetElement(ControlAutomationPeer childPeer, FAItemsRepeater repeater)
    {
        var childElement = childPeer.Owner;
        var parent = childElement.GetVisualParent();
        while (parent != null && (parent as FAItemsRepeater) != repeater)
        {
            childElement = (Control)parent;
            parent = childElement.GetVisualParent();
        }

        return childElement;
    }
}

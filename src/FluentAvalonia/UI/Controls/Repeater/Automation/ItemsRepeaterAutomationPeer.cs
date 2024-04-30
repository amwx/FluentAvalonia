using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

public class ItemsRepeaterAutomationPeer : ControlAutomationPeer
{
    public ItemsRepeaterAutomationPeer(Control owner) 
        : base(owner)
    {
    }

    public new ItemsRepeater Owner => (ItemsRepeater)base.Owner;

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
                if (ItemsRepeater.GetVirtualizationInfo(c) is VirtualizationInfo vi && vi.IsRealized)
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

    private static Control GetElement(ControlAutomationPeer childPeer, ItemsRepeater repeater)
    {
        var childElement = childPeer.Owner;
        var parent = childElement.GetVisualParent();
        while (parent != null && (parent as ItemsRepeater) != repeater)
        {
            childElement = (Control)parent;
            parent = childElement.GetVisualParent();
        }

        return childElement;
    }
}

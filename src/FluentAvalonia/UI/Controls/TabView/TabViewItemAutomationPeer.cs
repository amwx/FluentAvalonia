using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the <see cref="AutomationPeer"/> for a <see cref="TabViewItem"/>
/// </summary>
public sealed class TabViewItemAutomationPeer : ListItemAutomationPeer, ISelectionItemProvider
{
    public TabViewItemAutomationPeer(ContentControl owner) 
        : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() =>
        AutomationControlType.TabItem;

    protected override string GetNameCore()
    {
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name))
        {
            if (Owner is TabViewItem tvi)
            {
                name = tvi.Header?.ToString() ?? "TabViewItem";
            }
        }

        return name;
    }

    bool ISelectionItemProvider.IsSelected => (Owner as TabViewItem)?.IsSelected ?? false;

    ISelectionProvider ISelectionItemProvider.SelectionContainer
    {
        get
        {
            if (GetParentTabView() is TabView tv)
            {
                return ControlAutomationPeer.CreatePeerForElement(tv) as ISelectionProvider;
            }

            return null;
        }
    }

    void ISelectionItemProvider.AddToSelection()
    {
        ((ISelectionItemProvider)this).Select();
    }

    void ISelectionItemProvider.RemoveFromSelection()
    {
        // Can't unselect in a TabView without knowing the next selection
    }

    void ISelectionItemProvider.Select()
    {
        if (Owner is TabViewItem tvi)
            tvi.IsSelected = true;
    }

    private TabView GetParentTabView()
    {
        if (Owner is TabViewItem tvi)
        {
            return tvi.ParentTabView ?? tvi.FindAncestorOfType<TabView>();
        }

        return null;
    }

}

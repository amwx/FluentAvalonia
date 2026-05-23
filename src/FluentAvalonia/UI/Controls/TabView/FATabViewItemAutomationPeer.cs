using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the <see cref="AutomationPeer"/> for a <see cref="FATabViewItem"/>
/// </summary>
public sealed class FATabViewItemAutomationPeer : ListItemAutomationPeer, ISelectionItemProvider
{
    public FATabViewItemAutomationPeer(ContentControl owner) 
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
            if (Owner is FATabViewItem tvi)
            {
                name = tvi.Header?.ToString() ?? "TabViewItem";
            }
        }

        return name;
    }

    bool ISelectionItemProvider.IsSelected => (Owner as FATabViewItem)?.IsSelected ?? false;

    ISelectionProvider ISelectionItemProvider.SelectionContainer
    {
        get
        {
            if (GetParentTabView() is FATabView tv)
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
        if (Owner is FATabViewItem tvi)
            tvi.IsSelected = true;
    }

    private FATabView GetParentTabView()
    {
        if (Owner is FATabViewItem tvi)
        {
            return tvi.ParentTabView ?? tvi.FindAncestorOfType<FATabView>();
        }

        return null;
    }

}

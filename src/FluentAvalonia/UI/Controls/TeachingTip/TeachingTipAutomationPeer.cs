using System.Runtime.CompilerServices;
using Avalonia.Automation.Peers;
namespace FluentAvalonia.UI.Controls;

public class TeachingTipAutomationPeer : ContentControlAutomationPeer
{
    internal TeachingTipAutomationPeer(TeachingTip owner) 
        : base(owner)
    {

    }

    private TeachingTip TeachingTip => Unsafe.As<TeachingTip>(Owner);

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        if (TeachingTip.IsLightDismissEnabled)
        {
            return AutomationControlType.Window;
        }
        else
        {
            return AutomationControlType.Pane;
        }
    }

    protected override string GetClassNameCore() => nameof(TeachingTip);

    // public WindowInteractionState InteractionState();

    public bool IsModal() => TeachingTip.IsLightDismissEnabled;

    public bool IsTopmost() => TeachingTip.IsOpen;

    public bool Maximizable => false;

    public bool Minimizable => false;

    // public WindowVisualState VisualState();

    public void Close() => TeachingTip.IsOpen = false;

    // public void SetVisualState(WindowVisualState state);

    public bool WaitForInputIdle(int milliseconds) => true;

    public void RaiseWindowClosedEvent()
    {
        // Don't have automation events
    }

    public void RaiseWindowOpenedEvent()
    {
        // Don't have automation events
    }
}

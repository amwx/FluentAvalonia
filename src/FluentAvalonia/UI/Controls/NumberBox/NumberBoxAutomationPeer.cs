using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class NumberBoxAutomationPeer : ControlAutomationPeer, IRangeValueProvider
{
    public NumberBoxAutomationPeer(Control owner) : base(owner)
    {
        
    }

    public bool IsReadOnly { get; }

    public double Minimum => GetImpl().Minimum;

    public double Maximum => GetImpl().Maximum;

    public double Value => GetImpl().Value;

    public double LargeChange => GetImpl().LargeChange;

    public double SmallChange => GetImpl().SmallChange;

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Spinner;
    }

    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        if (string.IsNullOrEmpty(name))
        {
            if (Owner is NumberBox nb)
            {
                name = nb.Header is string ? nb.Header.ToString() : null;
            }
        }

        return name;
    }

    public void SetValue(double value)
    {
        GetImpl().Value = value;
    }

    internal void RaiseValueChangedEvent(double oldValue, double newValue)
    {
        RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty,
            oldValue, newValue);
    }

    private NumberBox GetImpl() => (NumberBox)Owner;
}

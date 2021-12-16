using System;
namespace FluentAvalonia.UI.Controls
{
    public class NumberBoxValueChangedEventArgs : EventArgs
    {
        internal NumberBoxValueChangedEventArgs(double oldV, double newV)
        {
            OldValue = oldV;
            NewValue = newV;
        }

        public double OldValue { get; }
        public double NewValue { get; }
    }
}

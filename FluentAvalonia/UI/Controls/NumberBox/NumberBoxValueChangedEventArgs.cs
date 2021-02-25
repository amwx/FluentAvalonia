using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class NumberBoxValueChangedEventArgs
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

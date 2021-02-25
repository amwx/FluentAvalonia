using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class CommandBarItemBase : TemplatedControl
    {
        public static readonly DirectProperty<CommandBarItemBase, int> DynamicOverflowOrderProperty =
            AvaloniaProperty.RegisterDirect<CommandBarItemBase, int>("DynamicOverflowOrder",
                x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

        public static readonly DirectProperty<CommandBarItemBase, bool> IsInOverflowProperty =
            AvaloniaProperty.RegisterDirect<CommandBarItemBase, bool>("IsInOverflow",
                x => x.IsInOverflow);
                
        public int DynamicOverflowOrder
        {
            get => _dynamicOverflowOrder;
            set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
        }

        public bool IsInOverflow
        {
            get => _isInOverflow;
            internal set => SetAndRaise(IsInOverflowProperty, ref _isInOverflow, value);
        }


        private int _dynamicOverflowOrder = -1;
        private bool _isInOverflow;
    }
}

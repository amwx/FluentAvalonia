using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewPaneClosingEventArgs
    {
        public bool Cancel
        {
            get => _cancel;
            set
            {
                _cancel = value;
                if (SplitViewClosingArgs != null)
                {
                    SplitViewClosingArgs.Cancel = value;
                }
            }
        }
        
        public SplitViewPaneClosingEventArgs SplitViewClosingArgs { get; internal set; }

        private bool _cancel;
    }
}

using FluentAvalonia.UI.Media;
using System;

namespace FluentAvalonia.UI.Controls
{
	public sealed class ColorChangedEventArgs : EventArgs
    {
        public Color2 OldColor { get; }
        public Color2 NewColor { get; }

        internal ColorChangedEventArgs(Color2 oldC, Color2 newC)
        {
            OldColor = oldC;
            NewColor = newC;
        }
    }
}

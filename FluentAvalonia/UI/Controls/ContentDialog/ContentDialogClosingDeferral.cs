using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class ContentDialogClosingDeferral
    {
        internal ContentDialogClosingDeferral(ContentDialog owner)
        {
            _owner = owner;
        }

        public void Complete()
        {
            _owner.CompleteClosingDeferral();
        }

        private ContentDialog _owner;
    }
}

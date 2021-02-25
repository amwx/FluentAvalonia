using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class DropDownButton : Button, IStyleable
    {
        Type IStyleable.StyleKey => typeof(DropDownButton);
    }
}

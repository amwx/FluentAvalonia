using Avalonia.Styling;
using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a button that includes a chevron to indicate a menu can be opened.
    /// </summary>
    public class DropDownButton : Button, IStyleable
    {
        Type IStyleable.StyleKey => typeof(DropDownButton);
    }
}

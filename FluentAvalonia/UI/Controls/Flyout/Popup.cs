//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Have to subclass Popup to implemnet IFocusScope, otherwise
    /// weird behavior can occur with certain control, i.e. TextBox
    /// </summary>
    public class Popup : Avalonia.Controls.Primitives.Popup, IFocusScope
    {
    }
}

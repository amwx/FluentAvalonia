using System;
using System.Collections.Generic;
using FluentAvalonia.UI.Data;

public delegate void DragItemsStartingEventHandler(object sender, DragItemsStartingEventArgs args);

public class DragItemsStartingEventArgs : EventArgs
{
    public bool Cancel { get; set; }
    public DataPackage Data { get; internal init; }
    public IList<object> Items { get; internal init; }
}

namespace FluentAvalonia.UI.Windowing.Win32;

internal unsafe struct MENUITEMINFO
{
    public uint cbSize;
    public uint fMask;
    public uint fType;
    public uint fState;
    public uint wID;
    public HMENU hSubMenu;
    public HBITMAP hbmpChecked;
    public HBITMAP hbmpUnchecked;
    public nuint dwItemData;
    public ushort* dwTypeData;
    public uint cch;
    public HBITMAP hbmpItem;
}

// Adapted from TerraFX.Interop.Windows, MIT license

namespace FluentAvalonia.UI.Windowing.Win32;

internal readonly unsafe struct LRESULT : IComparable, IEquatable<LRESULT>
{
    public readonly nint Value;

    public LRESULT(nint value)
    {
        Value = value;
    }

    public static bool operator ==(LRESULT left, LRESULT right) => left.Value == right.Value;

    public static bool operator !=(LRESULT left, LRESULT right) => left.Value != right.Value;

    public static bool operator <(LRESULT left, LRESULT right) => left.Value < right.Value;

    public static bool operator <=(LRESULT left, LRESULT right) => left.Value <= right.Value;

    public static bool operator >(LRESULT left, LRESULT right) => left.Value > right.Value;

    public static bool operator >=(LRESULT left, LRESULT right) => left.Value >= right.Value;

    public static implicit operator LRESULT(byte value) => new LRESULT(value);

    public static explicit operator byte(LRESULT value) => (byte)(value.Value);

    public static implicit operator LRESULT(short value) => new LRESULT(value);

    public static explicit operator short(LRESULT value) => (short)(value.Value);

    public static implicit operator LRESULT(int value) => new LRESULT(value);

    public static explicit operator int(LRESULT value) => (int)(value.Value);

    public static explicit operator LRESULT(long value) => new LRESULT((nint)(value));

    public static implicit operator long(LRESULT value) => value.Value;

    public static implicit operator LRESULT(nint value) => new LRESULT(value);

    public static implicit operator nint(LRESULT value) => value.Value;

    public static implicit operator LRESULT(sbyte value) => new LRESULT(value);

    public static explicit operator sbyte(LRESULT value) => (sbyte)(value.Value);

    public static implicit operator LRESULT(ushort value) => new LRESULT(value);

    public static explicit operator ushort(LRESULT value) => (ushort)(value.Value);

    public static explicit operator LRESULT(uint value) => new LRESULT((nint)(value));

    public static explicit operator uint(LRESULT value) => (uint)(value.Value);

    public static explicit operator LRESULT(ulong value) => new LRESULT((nint)(value));

    public static explicit operator ulong(LRESULT value) => (ulong)(value.Value);

    public static explicit operator LRESULT(nuint value) => new LRESULT((nint)(value));

    public static explicit operator nuint(LRESULT value) => (nuint)(value.Value);

    public int CompareTo(object obj)
    {
        if (obj is LRESULT other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of LRESULT.");
    }

    //public int CompareTo(LRESULT other) => Value.CompareTo(other.Value);

    public override bool Equals(object obj) => (obj is LRESULT other) && Equals(other);

    public bool Equals(LRESULT other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    //public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    public static explicit operator LRESULT(void* value) => new LRESULT((nint)(value));

    public static implicit operator void*(LRESULT value) => (void*)(value.Value);

    public static explicit operator LRESULT(BOOL value) => new LRESULT(value.Value);

    public static explicit operator BOOL(LRESULT value) => new BOOL((int)(value.Value));

    //public static explicit operator LRESULT(HANDLE value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HANDLE(LRESULT value) => new HANDLE((void*)(value.Value));

    //public static explicit operator LRESULT(HBRUSH value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HBRUSH(LRESULT value) => new HBRUSH((void*)(value.Value));

    //public static explicit operator LRESULT(HCURSOR value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HCURSOR(LRESULT value) => new HCURSOR((void*)(value.Value));

    //public static explicit operator LRESULT(HDC value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HDC(LRESULT value) => new HDC((void*)(value.Value));

    //public static explicit operator LRESULT(HDROP value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HDROP(LRESULT value) => new HDROP((void*)(value.Value));

    //public static explicit operator LRESULT(HFONT value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HFONT(LRESULT value) => new HFONT((void*)(value.Value));

    //public static explicit operator LRESULT(HGDIOBJ value) => new LRESULT((nint)(value.Value));

   // public static explicit operator HGDIOBJ(LRESULT value) => new HGDIOBJ((void*)(value.Value));

    //public static explicit operator LRESULT(HGLOBAL value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HGLOBAL(LRESULT value) => new HGLOBAL((void*)(value.Value));

   // public static explicit operator LRESULT(HICON value) => new LRESULT((nint)(value.Value));

   // public static explicit operator HICON(LRESULT value) => new HICON((void*)(value.Value));

    //public static explicit operator LRESULT(HINSTANCE value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HINSTANCE(LRESULT value) => new HINSTANCE((void*)(value.Value));

    //public static explicit operator LRESULT(HLOCAL value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HLOCAL(LRESULT value) => new HLOCAL((void*)(value.Value));

    public static explicit operator LRESULT(HMENU value) => new LRESULT((nint)(value.Value));

    public static explicit operator HMENU(LRESULT value) => new HMENU((void*)(value.Value));

    //public static explicit operator LRESULT(HMODULE value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HMODULE(LRESULT value) => new HMODULE((void*)(value.Value));

    //public static explicit operator LRESULT(HPALETTE value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HPALETTE(LRESULT value) => new HPALETTE((void*)(value.Value));

    //public static explicit operator LRESULT(HPEN value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HPEN(LRESULT value) => new HPEN((void*)(value.Value));

    //public static explicit operator LRESULT(HRGN value) => new LRESULT((nint)(value.Value));

    //public static explicit operator HRGN(LRESULT value) => new HRGN((void*)(value.Value));

    public static explicit operator LRESULT(HWND value) => new LRESULT((nint)(value.Value));

    public static explicit operator HWND(LRESULT value) => new HWND((void*)(value.Value));

    public static explicit operator LRESULT(LPARAM value) => new LRESULT(value.Value);

    public static explicit operator LRESULT(WPARAM value) => new LRESULT((nint)(value.Value));
}

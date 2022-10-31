// Adapted from TerraFX.Interop.Windows, MIT license

namespace FluentAvalonia.UI.Windowing.Win32;

internal readonly unsafe struct WPARAM : IComparable, IEquatable<WPARAM>
{
    public readonly nuint Value;

    public WPARAM(nuint value)
    {
        Value = value;
    }

    public static bool operator ==(WPARAM left, WPARAM right) => left.Value == right.Value;

    public static bool operator !=(WPARAM left, WPARAM right) => left.Value != right.Value;

    public static bool operator <(WPARAM left, WPARAM right) => left.Value < right.Value;

    public static bool operator <=(WPARAM left, WPARAM right) => left.Value <= right.Value;

    public static bool operator >(WPARAM left, WPARAM right) => left.Value > right.Value;

    public static bool operator >=(WPARAM left, WPARAM right) => left.Value >= right.Value;

    public static implicit operator WPARAM(byte value) => new WPARAM(value);

    public static explicit operator byte(WPARAM value) => (byte)(value.Value);

    public static explicit operator WPARAM(short value) => new WPARAM((nuint)(value));

    public static explicit operator short(WPARAM value) => (short)(value.Value);

    public static explicit operator WPARAM(int value) => new WPARAM((nuint)(value));

    public static explicit operator int(WPARAM value) => (int)(value.Value);

    public static explicit operator WPARAM(long value) => new WPARAM((nuint)(value));

    public static explicit operator long(WPARAM value) => (long)(value.Value);

    public static explicit operator WPARAM(nint value) => new WPARAM((nuint)(value));

    public static explicit operator nint(WPARAM value) => (nint)(value.Value);

    public static explicit operator WPARAM(sbyte value) => new WPARAM((nuint)(value));

    public static explicit operator sbyte(WPARAM value) => (sbyte)(value.Value);

    public static implicit operator WPARAM(ushort value) => new WPARAM(value);

    public static explicit operator ushort(WPARAM value) => (ushort)(value.Value);

    public static implicit operator WPARAM(uint value) => new WPARAM(value);

    public static explicit operator uint(WPARAM value) => (uint)(value.Value);

    public static explicit operator WPARAM(ulong value) => new WPARAM((nuint)(value));

    public static implicit operator ulong(WPARAM value) => value.Value;

    public static implicit operator WPARAM(nuint value) => new WPARAM(value);

    public static implicit operator nuint(WPARAM value) => value.Value;

    public int CompareTo(object obj)
    {
        if (obj is WPARAM other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of WPARAM.");
    }

   // public int CompareTo(WPARAM other) => Value.CompareTo(other.Value);

    public override bool Equals(object obj) => (obj is WPARAM other) && Equals(other);

    public bool Equals(WPARAM other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

   // public override string ToString() => Value.ToString((sizeof(nint) == 4) ? "X8" : "X16");

   // public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    public static explicit operator WPARAM(void* value) => new WPARAM((nuint)(value));

    public static implicit operator void*(WPARAM value) => (void*)(value.Value);

    public static explicit operator WPARAM(BOOL value) => new WPARAM((nuint)(value.Value));

    public static explicit operator BOOL(WPARAM value) => new BOOL((int)(value.Value));

    //public static explicit operator WPARAM(HANDLE value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HANDLE(WPARAM value) => new HANDLE((void*)(value.Value));

    //public static explicit operator WPARAM(HBRUSH value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HBRUSH(WPARAM value) => new HBRUSH((void*)(value.Value));

    //public static explicit operator WPARAM(HCURSOR value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HCURSOR(WPARAM value) => new HCURSOR((void*)(value.Value));

    //public static explicit operator WPARAM(HDC value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HDC(WPARAM value) => new HDC((void*)(value.Value));

    //public static explicit operator WPARAM(HDROP value) => new WPARAM((nuint)(value.Value));
    
    //public static explicit operator HDROP(WPARAM value) => new HDROP((void*)(value.Value));

    //public static explicit operator WPARAM(HFONT value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HFONT(WPARAM value) => new HFONT((void*)(value.Value));

    //public static explicit operator WPARAM(HGDIOBJ value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HGDIOBJ(WPARAM value) => new HGDIOBJ((void*)(value.Value));

    //public static explicit operator WPARAM(HGLOBAL value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HGLOBAL(WPARAM value) => new HGLOBAL((void*)(value.Value));

    //public static explicit operator WPARAM(HICON value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HICON(WPARAM value) => new HICON((void*)(value.Value));

    //public static explicit operator WPARAM(HINSTANCE value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HINSTANCE(WPARAM value) => new HINSTANCE((void*)(value.Value));

    //public static explicit operator WPARAM(HLOCAL value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HLOCAL(WPARAM value) => new HLOCAL((void*)(value.Value));

    public static explicit operator WPARAM(HMENU value) => new WPARAM((nuint)(value.Value));

    public static explicit operator HMENU(WPARAM value) => new HMENU((void*)(value.Value));

    //public static explicit operator WPARAM(HMODULE value) => new WPARAM((nuint)(value.Value));
    
    //public static explicit operator HMODULE(WPARAM value) => new HMODULE((void*)(value.Value));

    //public static explicit operator WPARAM(HPALETTE value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HPALETTE(WPARAM value) => new HPALETTE((void*)(value.Value));

    //public static explicit operator WPARAM(HPEN value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HPEN(WPARAM value) => new HPEN((void*)(value.Value));

    //public static explicit operator WPARAM(HRGN value) => new WPARAM((nuint)(value.Value));

    //public static explicit operator HRGN(WPARAM value) => new HRGN((void*)(value.Value));

    public static explicit operator WPARAM(HWND value) => new WPARAM((nuint)(value.Value));

    public static explicit operator HWND(WPARAM value) => new HWND((void*)(value.Value));

    public static explicit operator WPARAM(LPARAM value) => new WPARAM((nuint)(value.Value));

    public static explicit operator WPARAM(LRESULT value) => new WPARAM((nuint)(value.Value));
}

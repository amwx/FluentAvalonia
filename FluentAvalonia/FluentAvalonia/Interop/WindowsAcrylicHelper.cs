//Playing around with getting an Acrylic Window Background
//Thanks MS for breaking SetWindowCompositionAttribute in 1903

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
namespace FluentAvalonia.Interop
{
    internal enum AccentFlagsType
    {
        Window = 0,
        Popup,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_NCRENDERING_POLICY = 2,
        WCA_ACCENT_POLICY = 19
        // ...
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public int AnimationId;
    }

    internal static class WindowsAcrylicHelper
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        internal static void EnableBlur(IntPtr hwnd, AccentFlagsType style = AccentFlagsType.Window)
        {
            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);

            var osVersionInfo = new OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)) };
            if (WindowsVersionInterop.RtlGetVersion(ref osVersionInfo) != 0)
            {
                // TODO: Error handling
            }

            //var currentVersion = osVersionInfo.BuildNumber;
            //if (currentVersion >= 18362) 
            //{ 
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            //}
            //else if (currentVersion >= 17763)
            //{
            //accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            //}
            //else if (osVersionInfo.MajorVersion == 10)
            //{
            //    accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            //}
            //else
            //{
            //accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
            //}

            if (style == AccentFlagsType.Window)
            {
                accent.AccentFlags = 2;
            }
            else
            {
                accent.AccentFlags = 0x20 | 0x40 | 0x80 | 0x100;
            }
            //accent.AccentFlags = 2;
            accent.GradientColor = 0x00FFFFFF;  

            accent.AnimationId = 0;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(hwnd, ref data);

            Marshal.FreeHGlobal(accentPtr);


           
           
        }
    }
}

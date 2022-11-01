#pragma warning disable 108
// ReSharper disable RedundantUsingDirective
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedType.Local
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantCast
// ReSharper disable IdentifierTypo
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUnsafeContext
// ReSharper disable RedundantBaseQualifier
// ReSharper disable EmptyStatement
// ReSharper disable RedundantAttributeParentheses
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MicroCom.Runtime;

namespace FluentAvalonia.UI.Windowing.Win32
{
    internal unsafe partial interface ITaskbarList : global::MicroCom.Runtime.IUnknown
    {
        void HrInit();
        void AddTab(IntPtr hwnd);
        void DeleteTab(IntPtr hwnd);
        void ActivateTab(IntPtr hwnd);
        void SetActiveAlt(IntPtr hwnd);
    }

    internal unsafe partial interface ITaskbarList2 : ITaskbarList
    {
        void MarkFullscreenWindow(IntPtr hwnd, int fFullscreen);
    }

    internal unsafe partial interface ITaskbarList3 : ITaskbarList2
    {
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        void SetProgressState(IntPtr hwnd, int tbpFlags);
        void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        void UnregisterTab(IntPtr hwndTab);
        void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, int dwReserved);
        void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, int pButton);
        void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, int pButton);
        void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
        void SetOverlayIcon(IntPtr hwnd, void* hIcon, ushort* pszDescription);
        void SetThumbnailTooltip(IntPtr hwnd, ushort* pszTip);
        void SetThumbnailClip(IntPtr hwnd, RECT* prcClip);
    }
}

namespace FluentAvalonia.UI.Windowing.Win32.Impl
{
    internal unsafe partial class __MicroComITaskbarListProxy : global::MicroCom.Runtime.MicroComProxyBase, ITaskbarList
    {
        public void HrInit()
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, int>)(*PPV)[base.VTableSize + 0])(PPV);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("HrInit failed", __result);
        }

        public void AddTab(IntPtr hwnd)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)(*PPV)[base.VTableSize + 1])(PPV, hwnd);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("AddTab failed", __result);
        }

        public void DeleteTab(IntPtr hwnd)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)(*PPV)[base.VTableSize + 2])(PPV, hwnd);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("DeleteTab failed", __result);
        }

        public void ActivateTab(IntPtr hwnd)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)(*PPV)[base.VTableSize + 3])(PPV, hwnd);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("ActivateTab failed", __result);
        }

        public void SetActiveAlt(IntPtr hwnd)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)(*PPV)[base.VTableSize + 4])(PPV, hwnd);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetActiveAlt failed", __result);
        }

        [System.Runtime.CompilerServices.ModuleInitializer()]
        internal static void __MicroComModuleInit()
        {
            global::MicroCom.Runtime.MicroComRuntime.Register(typeof(ITaskbarList), new Guid("56FDF342-FD6D-11d0-958A-006097C9A090"), (p, owns) => new __MicroComITaskbarListProxy(p, owns));
        }

        protected __MicroComITaskbarListProxy(IntPtr nativePointer, bool ownsHandle) : base(nativePointer, ownsHandle)
        {
        }

        protected override int VTableSize => base.VTableSize + 5;
    }

    unsafe class __MicroComITaskbarListVTable : global::MicroCom.Runtime.MicroComVtblBase
    {
        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int HrInitDelegate(void* @this);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int HrInit(void* @this)
        {
            ITaskbarList __target = null;
            try
            {
                {
                    __target = (ITaskbarList)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.HrInit();
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int AddTabDelegate(void* @this, IntPtr hwnd);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int AddTab(void* @this, IntPtr hwnd)
        {
            ITaskbarList __target = null;
            try
            {
                {
                    __target = (ITaskbarList)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.AddTab(hwnd);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int DeleteTabDelegate(void* @this, IntPtr hwnd);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int DeleteTab(void* @this, IntPtr hwnd)
        {
            ITaskbarList __target = null;
            try
            {
                {
                    __target = (ITaskbarList)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.DeleteTab(hwnd);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int ActivateTabDelegate(void* @this, IntPtr hwnd);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int ActivateTab(void* @this, IntPtr hwnd)
        {
            ITaskbarList __target = null;
            try
            {
                {
                    __target = (ITaskbarList)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.ActivateTab(hwnd);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetActiveAltDelegate(void* @this, IntPtr hwnd);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetActiveAlt(void* @this, IntPtr hwnd)
        {
            ITaskbarList __target = null;
            try
            {
                {
                    __target = (ITaskbarList)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetActiveAlt(hwnd);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        protected __MicroComITaskbarListVTable()
        {
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, int>)&HrInit); 
#else
            base.AddMethod((HrInitDelegate)HrInit); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)&AddTab); 
#else
            base.AddMethod((AddTabDelegate)AddTab); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)&DeleteTab); 
#else
            base.AddMethod((DeleteTabDelegate)DeleteTab); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)&ActivateTab); 
#else
            base.AddMethod((ActivateTabDelegate)ActivateTab); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)&SetActiveAlt); 
#else
            base.AddMethod((SetActiveAltDelegate)SetActiveAlt); 
#endif
        }

        [System.Runtime.CompilerServices.ModuleInitializer()]
        internal static void __MicroComModuleInit() => global::MicroCom.Runtime.MicroComRuntime.RegisterVTable(typeof(ITaskbarList), new __MicroComITaskbarListVTable().CreateVTable());
    }

    internal unsafe partial class __MicroComITaskbarList2Proxy : __MicroComITaskbarListProxy, ITaskbarList2
    {
        public void MarkFullscreenWindow(IntPtr hwnd, int fFullscreen)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, int, int>)(*PPV)[base.VTableSize + 0])(PPV, hwnd, fFullscreen);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("MarkFullscreenWindow failed", __result);
        }

        [System.Runtime.CompilerServices.ModuleInitializer()]
        internal static void __MicroComModuleInit()
        {
            global::MicroCom.Runtime.MicroComRuntime.Register(typeof(ITaskbarList2), new Guid("602D4995-B13A-429b-A66E-1935E44F4317"), (p, owns) => new __MicroComITaskbarList2Proxy(p, owns));
        }

        protected __MicroComITaskbarList2Proxy(IntPtr nativePointer, bool ownsHandle) : base(nativePointer, ownsHandle)
        {
        }

        protected override int VTableSize => base.VTableSize + 1;
    }

    unsafe class __MicroComITaskbarList2VTable : __MicroComITaskbarListVTable
    {
        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int MarkFullscreenWindowDelegate(void* @this, IntPtr hwnd, int fFullscreen);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int MarkFullscreenWindow(void* @this, IntPtr hwnd, int fFullscreen)
        {
            ITaskbarList2 __target = null;
            try
            {
                {
                    __target = (ITaskbarList2)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.MarkFullscreenWindow(hwnd, fFullscreen);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        protected __MicroComITaskbarList2VTable()
        {
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, int, int>)&MarkFullscreenWindow); 
#else
            base.AddMethod((MarkFullscreenWindowDelegate)MarkFullscreenWindow); 
#endif
        }

        [System.Runtime.CompilerServices.ModuleInitializer()]
        internal static void __MicroComModuleInit() => global::MicroCom.Runtime.MicroComRuntime.RegisterVTable(typeof(ITaskbarList2), new __MicroComITaskbarList2VTable().CreateVTable());
    }

    internal unsafe partial class __MicroComITaskbarList3Proxy : __MicroComITaskbarList2Proxy, ITaskbarList3
    {
        public void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, ulong, ulong, int>)(*PPV)[base.VTableSize + 0])(PPV, hwnd, ullCompleted, ullTotal);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetProgressValue failed", __result);
        }

        public void SetProgressState(IntPtr hwnd, int tbpFlags)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, int, int>)(*PPV)[base.VTableSize + 1])(PPV, hwnd, tbpFlags);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetProgressState failed", __result);
        }

        public void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int>)(*PPV)[base.VTableSize + 2])(PPV, hwndTab, hwndMDI);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("RegisterTab failed", __result);
        }

        public void UnregisterTab(IntPtr hwndTab)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)(*PPV)[base.VTableSize + 3])(PPV, hwndTab);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("UnregisterTab failed", __result);
        }

        public void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int>)(*PPV)[base.VTableSize + 4])(PPV, hwndTab, hwndInsertBefore);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetTabOrder failed", __result);
        }

        public void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, int dwReserved)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int, int>)(*PPV)[base.VTableSize + 5])(PPV, hwndTab, hwndMDI, dwReserved);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetTabActive failed", __result);
        }

        public void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, int pButton)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, uint, int, int>)(*PPV)[base.VTableSize + 6])(PPV, hwnd, cButtons, pButton);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("ThumbBarAddButtons failed", __result);
        }

        public void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, int pButton)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, uint, int, int>)(*PPV)[base.VTableSize + 7])(PPV, hwnd, cButtons, pButton);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("ThumbBarUpdateButtons failed", __result);
        }

        public void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int>)(*PPV)[base.VTableSize + 8])(PPV, hwnd, himl);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("ThumbBarSetImageList failed", __result);
        }

        public void SetOverlayIcon(IntPtr hwnd, void* hIcon, ushort* pszDescription)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, void*, void*, int>)(*PPV)[base.VTableSize + 9])(PPV, hwnd, hIcon, pszDescription);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetOverlayIcon failed", __result);
        }

        public void SetThumbnailTooltip(IntPtr hwnd, ushort* pszTip)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, void*, int>)(*PPV)[base.VTableSize + 10])(PPV, hwnd, pszTip);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetThumbnailTooltip failed", __result);
        }

        public void SetThumbnailClip(IntPtr hwnd, RECT* prcClip)
        {
            int __result;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, IntPtr, void*, int>)(*PPV)[base.VTableSize + 11])(PPV, hwnd, prcClip);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("SetThumbnailClip failed", __result);
        }

        [System.Runtime.CompilerServices.ModuleInitializer()]
        internal static void __MicroComModuleInit()
        {
            global::MicroCom.Runtime.MicroComRuntime.Register(typeof(ITaskbarList3), new Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf"), (p, owns) => new __MicroComITaskbarList3Proxy(p, owns));
        }

        protected __MicroComITaskbarList3Proxy(IntPtr nativePointer, bool ownsHandle) : base(nativePointer, ownsHandle)
        {
        }

        protected override int VTableSize => base.VTableSize + 12;
    }

    unsafe class __MicroComITaskbarList3VTable : __MicroComITaskbarList2VTable
    {
        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetProgressValueDelegate(void* @this, IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetProgressValue(void* @this, IntPtr hwnd, ulong ullCompleted, ulong ullTotal)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetProgressValue(hwnd, ullCompleted, ullTotal);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetProgressStateDelegate(void* @this, IntPtr hwnd, int tbpFlags);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetProgressState(void* @this, IntPtr hwnd, int tbpFlags)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetProgressState(hwnd, tbpFlags);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int RegisterTabDelegate(void* @this, IntPtr hwndTab, IntPtr hwndMDI);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int RegisterTab(void* @this, IntPtr hwndTab, IntPtr hwndMDI)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.RegisterTab(hwndTab, hwndMDI);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int UnregisterTabDelegate(void* @this, IntPtr hwndTab);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int UnregisterTab(void* @this, IntPtr hwndTab)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.UnregisterTab(hwndTab);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetTabOrderDelegate(void* @this, IntPtr hwndTab, IntPtr hwndInsertBefore);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetTabOrder(void* @this, IntPtr hwndTab, IntPtr hwndInsertBefore)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetTabOrder(hwndTab, hwndInsertBefore);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetTabActiveDelegate(void* @this, IntPtr hwndTab, IntPtr hwndMDI, int dwReserved);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetTabActive(void* @this, IntPtr hwndTab, IntPtr hwndMDI, int dwReserved)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetTabActive(hwndTab, hwndMDI, dwReserved);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int ThumbBarAddButtonsDelegate(void* @this, IntPtr hwnd, uint cButtons, int pButton);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int ThumbBarAddButtons(void* @this, IntPtr hwnd, uint cButtons, int pButton)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.ThumbBarAddButtons(hwnd, cButtons, pButton);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int ThumbBarUpdateButtonsDelegate(void* @this, IntPtr hwnd, uint cButtons, int pButton);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int ThumbBarUpdateButtons(void* @this, IntPtr hwnd, uint cButtons, int pButton)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.ThumbBarUpdateButtons(hwnd, cButtons, pButton);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int ThumbBarSetImageListDelegate(void* @this, IntPtr hwnd, IntPtr himl);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int ThumbBarSetImageList(void* @this, IntPtr hwnd, IntPtr himl)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.ThumbBarSetImageList(hwnd, himl);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetOverlayIconDelegate(void* @this, IntPtr hwnd, void* hIcon, ushort* pszDescription);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetOverlayIcon(void* @this, IntPtr hwnd, void* hIcon, ushort* pszDescription)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetOverlayIcon(hwnd, hIcon, pszDescription);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetThumbnailTooltipDelegate(void* @this, IntPtr hwnd, ushort* pszTip);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetThumbnailTooltip(void* @this, IntPtr hwnd, ushort* pszTip)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetThumbnailTooltip(hwnd, pszTip);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        delegate int SetThumbnailClipDelegate(void* @this, IntPtr hwnd, RECT* prcClip);
#if NET5_0_OR_GREATER
        [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
        static int SetThumbnailClip(void* @this, IntPtr hwnd, RECT* prcClip)
        {
            ITaskbarList3 __target = null;
            try
            {
                {
                    __target = (ITaskbarList3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                    __target.SetThumbnailClip(hwnd, prcClip);
                }
            }
            catch (System.Runtime.InteropServices.COMException __com_exception__)
            {
                return __com_exception__.ErrorCode;
            }
            catch (System.Exception __exception__)
            {
                global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
                return unchecked((int)0x80004005u);
            }

            return 0;
        }

        protected __MicroComITaskbarList3VTable()
        {
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, ulong, ulong, int>)&SetProgressValue); 
#else
            base.AddMethod((SetProgressValueDelegate)SetProgressValue); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, int, int>)&SetProgressState); 
#else
            base.AddMethod((SetProgressStateDelegate)SetProgressState); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int>)&RegisterTab); 
#else
            base.AddMethod((RegisterTabDelegate)RegisterTab); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, int>)&UnregisterTab); 
#else
            base.AddMethod((UnregisterTabDelegate)UnregisterTab); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int>)&SetTabOrder); 
#else
            base.AddMethod((SetTabOrderDelegate)SetTabOrder); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int, int>)&SetTabActive); 
#else
            base.AddMethod((SetTabActiveDelegate)SetTabActive); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, uint, int, int>)&ThumbBarAddButtons); 
#else
            base.AddMethod((ThumbBarAddButtonsDelegate)ThumbBarAddButtons); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, uint, int, int>)&ThumbBarUpdateButtons); 
#else
            base.AddMethod((ThumbBarUpdateButtonsDelegate)ThumbBarUpdateButtons); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, IntPtr, int>)&ThumbBarSetImageList); 
#else
            base.AddMethod((ThumbBarSetImageListDelegate)ThumbBarSetImageList); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, void*, ushort*, int>)&SetOverlayIcon); 
#else
            base.AddMethod((SetOverlayIconDelegate)SetOverlayIcon); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, ushort*, int>)&SetThumbnailTooltip); 
#else
            base.AddMethod((SetThumbnailTooltipDelegate)SetThumbnailTooltip); 
#endif
#if NET5_0_OR_GREATER
            base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr, RECT*, int>)&SetThumbnailClip); 
#else
            base.AddMethod((SetThumbnailClipDelegate)SetThumbnailClip); 
#endif
        }

        [System.Runtime.CompilerServices.ModuleInitializer()]
        internal static void __MicroComModuleInit() => global::MicroCom.Runtime.MicroComRuntime.RegisterVTable(typeof(ITaskbarList3), new __MicroComITaskbarList3VTable().CreateVTable());
    }
}
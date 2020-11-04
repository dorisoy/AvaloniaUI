using System;
using System.Runtime.InteropServices;

namespace Avalonia.Win32.Interop.Automation
{
    [ComVisible(true)]
    [Guid("d8e55844-7043-4edc-979d-593cc6b4775e")]
    internal enum AsyncContentLoadedState
    {
        Beginning,
        Progress,
        Completed,
    }

    [ComVisible(true)]
    [Guid("e4cfef41-071d-472c-a65c-c14f59ea81eb")]
    internal enum StructureChangeType
    {
        ChildAdded,
        ChildRemoved,
        ChildrenInvalidated,
        ChildrenBulkAdded,
        ChildrenBulkRemoved,
        ChildrenReordered,
    }

    internal static class UiaCoreProviderApi
    {
        [DllImport("UIAutomationCore.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr UiaReturnRawElementProvider(IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple el);

        [DllImport("UIAutomationCore.dll", CharSet = CharSet.Unicode)]
        public static extern int UiaHostProviderFromHwnd(IntPtr hwnd, [MarshalAs(UnmanagedType.Interface)] out IRawElementProviderSimple provider);

        [DllImport("UIAutomationCore.dll", CharSet = CharSet.Unicode)]
        public static extern int UiaRaiseStructureChangedEvent(IRawElementProviderSimple provider, StructureChangeType structureChangeType, int[] runtimeId, int runtimeIdLen);
    }
}

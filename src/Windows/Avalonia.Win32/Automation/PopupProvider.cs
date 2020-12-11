using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Primitives;
using Avalonia.Win32.Interop.Automation;

namespace Avalonia.Win32.Automation
{
    internal class PopupProvider : AutomationProvider
    {
        public PopupProvider(
            AutomationPeer peer,
            UiaControlTypeId controlType,
            bool isControlElement,
            WindowImpl visualRoot,
            IRawElementProviderFragmentRoot fragmentRoot) 
            : base(peer, controlType, isControlElement, visualRoot, fragmentRoot)
        {
        }

        public override IRawElementProviderSimple HostRawElementProvider
        {
            get
            {
                var popup = (PopupRoot)((ControlAutomationPeer)Peer).Owner;
                var hwnd = popup.PlatformImpl.Handle.Handle;
                UiaCoreProviderApi.UiaHostProviderFromHwnd(hwnd, out var result);
                return result;
            }
        }
    }
}

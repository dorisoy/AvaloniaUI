using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Threading;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal static class AutomationProviderFactory
    {
        public static AutomationProvider Create(AutomationPeer peer)
        {
            Dispatcher.UIThread.VerifyAccess();

            if (peer.PlatformImpl is object)
            {
                throw new AvaloniaInternalException($"Peer already has a platform implementation: {peer}.");
            }

            if (peer is WindowAutomationPeer windowPeer)
            {
                var windowImpl = (WindowImpl)((Window)windowPeer.Owner).PlatformImpl;
                return new WindowProvider(windowImpl, windowPeer);
            }

            var controlType = peer switch
            {
                AnonymousAutomationPeer _ => UiaControlTypeId.Group,
                ButtonAutomationPeer _ => UiaControlTypeId.Button,
                MenuAutomationPeer _ => UiaControlTypeId.Menu,
                MenuItemAutomationPeer _ => UiaControlTypeId.MenuItem,
                SliderAutomationPeer _ => UiaControlTypeId.Slider,
                TabControlAutomationPeer _ => UiaControlTypeId.Tab,
                TabItemAutomationPeer _ => UiaControlTypeId.TabItem,
                TextAutomationPeer _ => UiaControlTypeId.Text,
                _ => UiaControlTypeId.Custom,
            };

            var result = new AutomationProvider(peer, controlType);
            var _ = result.Update(notify: false);
            return result;
        }
    }
}

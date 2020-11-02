using System;
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

            var result = peer switch
            {
                AnonymousAutomationPeer _ => new AutomationProvider(peer, UiaControlTypeId.Custom, false, false),
                ButtonAutomationPeer _ => new AutomationProvider(peer, UiaControlTypeId.Button),
                MenuAutomationPeer _ => new AutomationProvider(peer, UiaControlTypeId.Menu),
                MenuItemAutomationPeer _ => new AutomationProvider(peer, UiaControlTypeId.MenuItem),
                TabControlAutomationPeer _ => new AutomationProvider(peer, UiaControlTypeId.Tab),
                TabItemAutomationPeer _ => new AutomationProvider(peer, UiaControlTypeId.TabItem),
                TextAutomationPeer _ => new AutomationProvider(peer, UiaControlTypeId.Text),
                _ => new AutomationProvider(peer, UiaControlTypeId.Custom),
            };

            var _ = result.Update();
            return result;
        }
    }
}

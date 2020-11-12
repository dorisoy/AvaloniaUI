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
        public static AutomationProvider Create(AutomationPeer peer, IRawElementProviderFragmentRoot? root)
        {
            Dispatcher.UIThread.VerifyAccess();

            if (peer.PlatformImpl is object)
            {
                throw new AvaloniaInternalException($"Peer already has a platform implementation: {peer}.");
            }

            if (peer is WindowAutomationPeer windowPeer)
            {
                if (root is object)
                {
                    throw new ArgumentNullException("Root must be null for root automation peers.");
                }

                var windowImpl = (WindowImpl)((Window)windowPeer.Owner).PlatformImpl;
                return new WindowProvider(windowImpl, windowPeer);
            }

            if (root is null)
            {
                throw new ArgumentNullException("Root may only be null for root automation peers.");
            }

            var controlType = peer switch
            {
                AnonymousAutomationPeer _ => UiaControlTypeId.Group,
                ButtonAutomationPeer _ => UiaControlTypeId.Button,
                ComboBoxAutomationPeer _ => UiaControlTypeId.ComboBox,
                MenuAutomationPeer _ => UiaControlTypeId.Menu,
                MenuItemAutomationPeer _ => UiaControlTypeId.MenuItem,
                SliderAutomationPeer _ => UiaControlTypeId.Slider,
                TabControlAutomationPeer _ => UiaControlTypeId.Tab,
                TabItemAutomationPeer _ => UiaControlTypeId.TabItem,
                TextAutomationPeer _ => UiaControlTypeId.Text,
                _ => UiaControlTypeId.Custom,
            };

            var result = new AutomationProvider(peer, controlType, root);
            var _ = result.Update(notify: false);
            return result;
        }
    }
}

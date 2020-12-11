using System;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal static class AutomationProviderFactory
    {
        public static AutomationProvider Create(
            AutomationPeer peer, 
            WindowImpl visualRoot,
            IRawElementProviderFragmentRoot? fragmentRoot)
        {
            Dispatcher.UIThread.VerifyAccess();

            if (peer.PlatformImpl is object)
            {
                throw new AvaloniaInternalException($"Peer already has a platform implementation: {peer}.");
            }

            if (peer is WindowAutomationPeer windowPeer)
            {
                if (fragmentRoot is object)
                {
                    throw new ArgumentNullException("Root must be null for root automation peers.");
                }

                var windowImpl = (WindowImpl)((Window)windowPeer.Owner).PlatformImpl;
                return new WindowProvider(windowImpl, windowPeer);
            }

            if (fragmentRoot is null)
            {
                throw new ArgumentNullException("Root may only be null for root automation peers.");
            }

            var role = peer.GetRole();
            var isControlElement = role != AutomationRole.None;
            var uiaControlType = role switch
            {
                AutomationRole.Button => UiaControlTypeId.Button,
                AutomationRole.ComboBox => UiaControlTypeId.ComboBox,
                AutomationRole.Group => UiaControlTypeId.Group,
                AutomationRole.List => UiaControlTypeId.List,
                AutomationRole.ListItem => UiaControlTypeId.ListItem,
                AutomationRole.Menu => UiaControlTypeId.Menu,
                AutomationRole.MenuItem => UiaControlTypeId.MenuItem,
                AutomationRole.Slider => UiaControlTypeId.Slider,
                AutomationRole.TabControl => UiaControlTypeId.Tab,
                AutomationRole.TabItem => UiaControlTypeId.TabItem,
                AutomationRole.Text => UiaControlTypeId.Text,
                _ => UiaControlTypeId.Custom,
            };

            var result = peer is ControlAutomationPeer c && c.Owner is PopupRoot ?
                new PopupProvider(peer, uiaControlType, isControlElement, visualRoot, fragmentRoot) :
                new AutomationProvider(peer, uiaControlType, isControlElement, visualRoot, fragmentRoot);
            var _ = result.Update(notify: false);
            return result;
        }
    }
}

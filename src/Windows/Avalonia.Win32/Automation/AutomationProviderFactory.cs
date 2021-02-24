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

            AutomationProvider result;

            if (peer is WindowAutomationPeer windowPeer)
            {
                if (fragmentRoot is object)
                {
                    throw new ArgumentNullException("Root must be null for root automation peers.");
                }

                var windowImpl = (WindowImpl)((Window)windowPeer.Owner).PlatformImpl;
                result = new WindowProvider(windowImpl, windowPeer);
            }
            else if (fragmentRoot is null)
            {
                throw new ArgumentNullException("Root may only be null for root automation peers.");
            }
            else
            {
                var role = peer.GetRole();
                var uiaControlType = role switch
                {
                    AutomationRole.Button => UiaControlTypeId.Button,
                    AutomationRole.CheckBox => UiaControlTypeId.CheckBox,
                    AutomationRole.ComboBox => UiaControlTypeId.ComboBox,
                    AutomationRole.Edit => UiaControlTypeId.Edit,
                    AutomationRole.Group => UiaControlTypeId.Group,
                    AutomationRole.List => UiaControlTypeId.List,
                    AutomationRole.ListItem => UiaControlTypeId.ListItem,
                    AutomationRole.Menu => UiaControlTypeId.Menu,
                    AutomationRole.MenuItem => UiaControlTypeId.MenuItem,
                    AutomationRole.Slider => UiaControlTypeId.Slider,
                    AutomationRole.TabControl => UiaControlTypeId.Tab,
                    AutomationRole.TabItem => UiaControlTypeId.TabItem,
                    AutomationRole.Text => UiaControlTypeId.Text,
                    AutomationRole.Toggle => UiaControlTypeId.Button,
                    _ => UiaControlTypeId.Custom,
                };

                result = peer is ControlAutomationPeer c && c.Owner is PopupRoot ?
                    new PopupProvider(peer, uiaControlType, visualRoot, fragmentRoot) :
                    new AutomationProvider(peer, uiaControlType, visualRoot, fragmentRoot);
            }

            var _ = result.Update(notify: false);
            return result;
        }
    }
}

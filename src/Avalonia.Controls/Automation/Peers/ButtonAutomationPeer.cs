#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class ButtonAutomationPeer : ContentControlAutomationPeer,
        IInvocableAutomationPeer
    {
        public ButtonAutomationPeer(Control owner, AutomationRole role = AutomationRole.Button)
            : base(owner, role) 
        {
        }
        
        public void Invoke()
        {
            EnsureEnabled();
            (Owner as Button)?.PerformClick();
        }
    }
}


#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class ButtonAutomationPeer : ContentControlAutomationPeer,
        IInvocableAutomationPeer
    {
        public ButtonAutomationPeer(Control owner)
            : base(owner, AutomationRole.Button) 
        {
        }
        
        public void Invoke()
        {
            EnsureEnabled();
            (Owner as Button)?.PerformClick();
        }
    }
}


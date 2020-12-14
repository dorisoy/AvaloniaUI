namespace Avalonia.Controls.Automation.Peers
{
    public class ImageAutomationPeer : ControlAutomationPeer
    {
        public ImageAutomationPeer(Control owner, AutomationRole role = AutomationRole.Image)
            : base(owner, role)
        {
        }
    }
}

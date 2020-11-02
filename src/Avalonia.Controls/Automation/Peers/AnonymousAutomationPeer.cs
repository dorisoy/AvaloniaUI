namespace Avalonia.Controls.Automation.Peers
{
    /// <summary>
    /// An automation peer that represents a control that does not provide any meaningful
    /// interaction with the user.
    /// </summary>
    public class AnonymousAutomationPeer : ControlAutomationPeer
    {
        public AnonymousAutomationPeer(Control owner)
            : base(owner)
        {
        }
    }
}

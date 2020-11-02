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

        protected override AutomationPeer GetPeerFromPointCore(Point point)
        {
            var result = base.GetPeerFromPointCore(point);
            return result != this ? result : null;
        }

        protected override bool IsHiddenCore() => true;
    }
}

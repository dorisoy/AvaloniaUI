namespace Avalonia.Controls.Automation.Peers
{
    public class TabControlAutomationPeer : SelectingItemsControlAutomationPeer
    {
        public TabControlAutomationPeer(Control owner) : base(owner, AutomationRole.TabControl) { }

        protected override string GetLocalizedControlTypeCore() => "tab control";
    }
}

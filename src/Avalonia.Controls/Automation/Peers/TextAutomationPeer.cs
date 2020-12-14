#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class TextAutomationPeer : ControlAutomationPeer
    {
        public TextAutomationPeer(Control owner, AutomationRole role = AutomationRole.Text)
            : base(owner, role) 
        { 
        }

        protected override string? GetNameCore() => Owner.GetValue(TextBlock.TextProperty);
    }
}

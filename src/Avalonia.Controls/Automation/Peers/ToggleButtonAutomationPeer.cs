#nullable enable

using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Automation.Peers
{
    public class ToggleButtonAutomationPeer : ContentControlAutomationPeer, IToggleableAutomationPeer
    {
        public ToggleButtonAutomationPeer(Control owner, AutomationRole role = AutomationRole.Toggle)
            : base(owner, role)
        {
        }

        bool? IToggleableAutomationPeer.GetToggleState() => Owner.GetValue(ToggleButton.IsCheckedProperty);

        void IToggleableAutomationPeer.Toggle()
        {
            EnsureEnabled();
            (Owner as ToggleButton)?.PerformClick();
        }

        protected override string GetLocalizedControlTypeCore() => "toggle button";
    }
}

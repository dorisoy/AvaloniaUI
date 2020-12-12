using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls
{
    /// <summary>
    /// A check box control.
    /// </summary>
    public class CheckBox : ToggleButton
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToggleButtonAutomationPeer(this, AutomationRole.CheckBox);
        }
    }
}

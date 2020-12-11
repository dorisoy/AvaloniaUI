#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class SliderAutomationPeer : ControlAutomationPeer
    {
        public SliderAutomationPeer(Control owner)
            : base(owner, AutomationRole.Slider) 
        {
        }
    }
}


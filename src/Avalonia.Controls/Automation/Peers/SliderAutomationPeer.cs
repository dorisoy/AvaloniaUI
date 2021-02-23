#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class SliderAutomationPeer : RangeBaseAutomationPeer
    {
        public SliderAutomationPeer(Control owner)
            : base(owner, AutomationRole.Slider) 
        {
        }
    }
}

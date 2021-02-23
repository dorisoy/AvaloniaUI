#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public interface IRangeValueAutomationPeer
    {
        public double GetMinimum();
        public double GetMaximum();
        public double GetValue();
        public void SetValue(double value);
    }
}

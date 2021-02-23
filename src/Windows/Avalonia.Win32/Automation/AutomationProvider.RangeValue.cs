using Avalonia.Controls.Automation.Peers;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal partial class AutomationProvider : IRangeValueProvider
    {
        private double _rangeValue;
        private double _rangeMinimum;
        private double _rangeMaximum;

        double IRangeValueProvider.Value => InvokeSync<IRangeValueAutomationPeer, double>(x => x.GetValue());
        bool IRangeValueProvider.IsReadOnly => false;
        double IRangeValueProvider.Maximum => InvokeSync<IRangeValueAutomationPeer, double>(x => x.GetMaximum());
        double IRangeValueProvider.Minimum => InvokeSync<IRangeValueAutomationPeer, double>(x => x.GetMinimum());
        double IRangeValueProvider.LargeChange => 1;
        double IRangeValueProvider.SmallChange => 1;

        void IRangeValueProvider.SetValue(double value) => InvokeSync<IRangeValueAutomationPeer>(x => x.SetValue(value));

        private void UpdateRangeValue(bool notify)
        {
            if (Peer is IRangeValueAutomationPeer peer)
            {
                UpdateProperty(UiaPropertyId.RangeValueValue, ref _rangeValue, peer.GetValue(), notify);
                UpdateProperty(UiaPropertyId.RangeValueMinimum, ref _rangeMinimum, peer.GetMinimum(), notify);
                UpdateProperty(UiaPropertyId.RangeValueMaximum, ref _rangeMaximum, peer.GetMaximum(), notify);
            }
        }
    }
}

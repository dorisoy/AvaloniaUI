using Avalonia.Controls.Primitives;

#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class RangeBaseAutomationPeer : ControlAutomationPeer, IRangeValueAutomationPeer
    {
        public RangeBaseAutomationPeer(Control owner, AutomationRole role)
            : base(owner, role) 
        {
            owner.PropertyChanged += OwnerPropertyChanged;
        }

        public double GetMaximum() => Owner.GetValue(RangeBase.MaximumProperty);
        public double GetMinimum() => Owner.GetValue(RangeBase.MinimumProperty);
        public double GetValue() => Owner.GetValue(RangeBase.ValueProperty);
        public void SetValue(double value) => Owner.SetValue(RangeBase.ValueProperty, value);

        protected virtual void OwnerPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == RangeBase.MinimumProperty ||
                e.Property == RangeBase.MaximumProperty ||
                e.Property == RangeBase.ValueProperty)
            {
                PlatformImpl?.PropertyChanged();
            }
        }
    }
}

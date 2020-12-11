namespace Avalonia.Controls.Automation.Peers
{
    public class ComboBoxAutomationPeer : SelectingItemsControlAutomationPeer,
        IOpenCloseAutomationPeer
    {
        public ComboBoxAutomationPeer(Control owner) : base(owner, AutomationRole.ComboBox) { }
        public void Close() => Owner.SetValue(ComboBox.IsDropDownOpenProperty, false);
        public bool GetIsOpen() => Owner.GetValue(ComboBox.IsDropDownOpenProperty);
        public void Open() => Owner.SetValue(ComboBox.IsDropDownOpenProperty, true);
    }
}

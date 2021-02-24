namespace Avalonia.Controls.Automation.Peers
{
    public class TextBoxAutomationPeer : TextAutomationPeer, IStringValueAutomationPeer
    {
        public TextBoxAutomationPeer(Control owner, AutomationRole role = AutomationRole.Edit)
            : base(owner, role)
        {
        }

        string IStringValueAutomationPeer.GetValue() => Owner.GetValue(TextBlock.TextProperty);
        void IStringValueAutomationPeer.SetValue(string value) => Owner.SetValue(TextBlock.TextProperty, value);

        protected override string GetLocalizedControlTypeCore() => "text box";
    }
}

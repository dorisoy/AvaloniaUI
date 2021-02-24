#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class MenuItemAutomationPeer : ControlAutomationPeer
    {
        public MenuItemAutomationPeer(Control owner)
            : base(owner, AutomationRole.MenuItem) 
        { 
        }

        protected override string GetLocalizedControlTypeCore() => "menu item";

        protected override string? GetNameCore()
        {
            var result = base.GetNameCore();

            if (result is null && Owner is MenuItem m && m.HeaderPresenter.Child is TextBlock text)
            {
                result = text.Text;
            }

            if (result is null)
            {
                result = Owner.GetValue(ContentControl.ContentProperty)?.ToString();
            }

            return result;
        }
    }
}

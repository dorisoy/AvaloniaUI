#nullable enable

using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using Avalonia.Platform;

namespace Avalonia.Controls.Automation.Peers
{
    public class ComboBoxAutomationPeer : SelectingItemsControlAutomationPeer,
        IOpenCloseAutomationPeer
    {
        public ComboBoxAutomationPeer(Control owner) : base(owner, AutomationRole.ComboBox) { }

        public void Close() => Owner.SetValue(ComboBox.IsDropDownOpenProperty, false);
        public bool GetIsOpen() => Owner.GetValue(ComboBox.IsDropDownOpenProperty);
        public void Open() => Owner.SetValue(ComboBox.IsDropDownOpenProperty, true);

        protected override IReadOnlyList<AutomationPeer>? GetSelectionCore()
        {
            if (GetIsOpen())
                return base.GetSelectionCore();

            // If the combo box is not open then we won't have an ItemsPresenter so the default
            // GetSelectionCore implementation won't work.
            if (Owner is ComboBox owner &&
                Owner.GetValue(ComboBox.SelectedItemProperty) is object selection)
            {
                var peer = new SurrogateItemPeer(owner, selection);
                peer.CreatePlatformImpl();
                return new[] { peer };
            }

            return null;
        }

        protected override string GetLocalizedControlTypeCore() => "combo box";

        protected override void OwnerPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            base.OwnerPropertyChanged(sender, e);

            if (e.Property == ComboBox.IsDropDownOpenProperty)
            {
                PlatformImpl?.PropertyChanged();
            }
        }

        private class SurrogateItemPeer : ListItemAutomationPeer
        {
            private readonly object _item;

            public SurrogateItemPeer(ComboBox owner, object item)
                : base(owner, AutomationRole.ListItem)
            {
                _item = item;
            }

            protected override string? GetNameCore()
            {
                if (_item is Control c)
                {
                    var result = AutomationProperties.GetName(c);

                    if (result is null && c is ContentControl cc && cc.Presenter?.Child is TextBlock text)
                    {
                        result = text.Text;
                    }

                    if (result is null)
                    {
                        result = c.GetValue(ContentControl.ContentProperty)?.ToString();
                    }

                    return result;
                }

                return _item.ToString();
            }
        }
    }
}

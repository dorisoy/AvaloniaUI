#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class ItemsControlAutomationPeer : ControlAutomationPeer, IScrollableAutomationPeer
    {
        private bool _searchedForScrollable;
        private IScrollableAutomationPeer? _scroller;

        public ItemsControlAutomationPeer(Control owner, AutomationRole role = AutomationRole.List)
            : base(owner, role)
        {
        }

        protected virtual IScrollableAutomationPeer? Scroller
        {
            get
            {
                if (!_searchedForScrollable)
                {
                    if (Owner.GetValue(ListBox.ScrollProperty) is Control scrollable)
                        _scroller = GetOrCreatePeer(scrollable) as IScrollableAutomationPeer;
                    _searchedForScrollable = true;
                }

                return _scroller;
            }
        }

        Size IScrollableAutomationPeer.GetExtent() => Scroller?.GetExtent() ?? default;
        Size IScrollableAutomationPeer.GetViewport() => Scroller?.GetViewport() ?? default;
        Vector IScrollableAutomationPeer.GetOffset() => Scroller?.GetOffset() ?? default;
        void IScrollableAutomationPeer.SetOffset(Vector value) => Scroller?.SetOffset(value);
        void IScrollableAutomationPeer.LineDown() => Scroller?.LineDown();
        void IScrollableAutomationPeer.LineLeft() => Scroller?.LineLeft();
        void IScrollableAutomationPeer.LineRight() => Scroller?.LineRight();
        void IScrollableAutomationPeer.LineUp() => Scroller?.LineUp();
        void IScrollableAutomationPeer.PageDown() => Scroller?.PageDown();
        void IScrollableAutomationPeer.PageLeft() => Scroller?.PageLeft();
        void IScrollableAutomationPeer.PageRight() => Scroller?.PageRight();
        void IScrollableAutomationPeer.PageUp() => Scroller?.PageUp();
    }
}

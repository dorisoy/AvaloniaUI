#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class ListAutomationPeer : ControlAutomationPeer, IScrollableAutomationPeer
    {
        private bool _searchedForScrollable;
        private IScrollableAutomationPeer? _scroller;

        public ListAutomationPeer(Control owner, AutomationRole role = AutomationRole.List)
            : base(owner, role)
        {
        }

        private IScrollableAutomationPeer? Scroller
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

        Size IScrollableAutomationPeer.Extent => Scroller?.Extent ?? default;
        Size IScrollableAutomationPeer.Viewport => Scroller?.Viewport ?? default;

        Vector IScrollableAutomationPeer.Offset
        {
            get => Scroller?.Offset ?? default;
            set
            {
                if (Scroller is object)
                    Scroller.Offset = value;
            }
        }

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

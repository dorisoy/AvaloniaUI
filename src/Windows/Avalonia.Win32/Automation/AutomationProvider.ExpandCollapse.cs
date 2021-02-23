using Avalonia.Controls.Automation.Peers;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal partial class AutomationProvider : IExpandCollapseProvider
    {
        private ExpandCollapseState _expandCollapseState;

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState => _expandCollapseState;
        void IExpandCollapseProvider.Expand() => InvokeSync<IOpenCloseAutomationPeer>(x => x.Open());
        void IExpandCollapseProvider.Collapse() => InvokeSync<IOpenCloseAutomationPeer>(x => x.Close());

        private void UpdateExpandCollapse(bool notify)
        {
            if (Peer is IOpenCloseAutomationPeer peer)
            {
                UpdateProperty(
                    UiaPropertyId.ExpandCollapseExpandCollapseState,
                    ref _expandCollapseState,
                    peer.GetIsOpen() ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                    notify);
            }
        }
    }
}

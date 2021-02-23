using Avalonia.Controls.Automation.Peers;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal partial class AutomationProvider : IToggleProvider
    {
        private ToggleState _toggleState;

        ToggleState IToggleProvider.ToggleState => _toggleState;
        void IToggleProvider.Toggle() => InvokeSync<IToggleableAutomationPeer>(x => x.Toggle());

        private void UpdateToggle(bool notify)
        {
            if (Peer is IToggleableAutomationPeer peer)
            {
                UpdateProperty(
                    UiaPropertyId.ToggleToggleState,
                    ref _toggleState,
                    peer.GetToggleState() switch
                    {
                        true => ToggleState.On,
                        false => ToggleState.Off,
                        null => ToggleState.Indeterminate,
                    },
                    notify);
            }
        }
    }
}

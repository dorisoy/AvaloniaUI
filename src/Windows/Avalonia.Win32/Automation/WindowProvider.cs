using System;
using System.Runtime.InteropServices;
using Avalonia.Controls.Automation.Peers;
using Avalonia.VisualTree;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal class WindowProvider : AutomationProvider, IRawElementProviderFragmentRoot
    {
        private WeakReference<AutomationPeer>? _focusPeer;
        private AutomationProvider? _focus;
        private bool _focusValid;

        public WindowProvider(WindowImpl owner, WindowAutomationPeer peer)
            : base(peer, owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public WindowImpl Owner { get; }
        
        public IRawElementProviderFragment? ElementProviderFromPoint(double x, double y)
        {
            var p = Owner.PointToClient(new PixelPoint((int)x, (int)y));
            var peer = InvokeSync(() => Peer.GetPeerFromPoint(p));
            return peer?.PlatformImpl as IRawElementProviderFragment;
        }

        public override IRawElementProviderFragmentRoot FragmentRoot => this;

        public IRawElementProviderFragment? GetFocus()
        {
            if (!_focusValid)
            {
                var peer = (WindowAutomationPeer)Peer;
                var focusPeer = InvokeSync(() => peer.GetFocus());

                _focus = focusPeer?.PlatformImpl as AutomationProvider;
                _focusValid = true;
            }

            return _focus;
        }

        public override IRawElementProviderSimple? HostRawElementProvider
        {
            get
            {
                var hr = UiaCoreProviderApi.UiaHostProviderFromHwnd(Owner.Handle.Handle, out var result);
                Marshal.ThrowExceptionForHR(hr);
                return result;
            }
        }

        protected override void UpdateCore(bool notify)
        {
            base.UpdateCore(notify);

            var peer = (WindowAutomationPeer)Peer;
            var newFocusPeer = peer.GetFocus();

            AutomationPeer? oldFocusPeer = null;
            _focusPeer?.TryGetTarget(out oldFocusPeer);

            if (newFocusPeer != oldFocusPeer)
            {
                _focusValid = false;

                if (newFocusPeer is object)
                {
                    _focusPeer = new WeakReference<AutomationPeer>(newFocusPeer);

                    var oldProvider = oldFocusPeer?.PlatformImpl as AutomationProvider;
                    var newProvider = (AutomationProvider)newFocusPeer.PlatformImpl!;
                    var _ = oldProvider?.Update(false);
                    _ = newProvider.Update(false);
                    UiaCoreProviderApi.UiaRaiseAutomationEvent(newProvider, (int)UiaEventId.AutomationFocusChanged);
                }
                else
                {
                    _focusPeer = null;
                }
            }
        }
    }
}

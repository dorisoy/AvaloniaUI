#nullable enable

using System;
using System.ComponentModel;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Automation.Peers
{
    public class WindowAutomationPeer : ControlAutomationPeer,
        IRootAutomationPeer
    {
        public WindowAutomationPeer(Control owner)
            : base(owner) 
        {
            KeyboardDevice.Instance.PropertyChanged += KeyboardDevicePropertyChanged;
        }

        public AutomationPeer? GetFocus() => GetFocusCore();

        protected AutomationPeer? GetFocusCore()
        {
            var focused = KeyboardDevice.Instance.FocusedElement as Control;

            if (focused is null)
                return null;

            return GetRoot(focused) == Owner ? GetOrCreatePeer(focused) : null;
        }

        protected override string GetNameCore() => Owner.GetValue(Window.TitleProperty);
        protected override AutomationPeer? GetParentCore() => null;

        private static TopLevel? GetRoot(IControl control)
        {
            var root = control.VisualRoot as TopLevel;

            while (root is IHostedVisualTreeRoot popup)
            {
                root = popup.Host?.GetVisualRoot() as TopLevel;
            }

            return root;
        }

        private void KeyboardDevicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PlatformImpl?.PropertyChanged();
        }
    }
}



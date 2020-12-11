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
            : base(owner, AutomationRole.Window)
        {
            KeyboardDevice.Instance.PropertyChanged += KeyboardDevicePropertyChanged;
        }

        public AutomationPeer? GetFocus() => GetFocusCore();

        protected virtual void FocusChangedCore(IInputElement? focusedElement)
        {
            // HACK: Don't send focus changed messages when application deactivates. We shouldn't
            // be clearing the focus in this case.
            if (Owner.GetValue(Window.IsActiveProperty))
            {
                PlatformImpl?.PropertyChanged();
            }
        }

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
            if (e.PropertyName == nameof(KeyboardDevice.FocusedElement))
            {
                FocusChangedCore(KeyboardDevice.Instance.FocusedElement);
            }
        }
    }
}



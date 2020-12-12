using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Automation.Peers
{
    public interface IToggleableAutomationPeer
    {
        bool? GetToggleState();
        void Toggle();
    }
}

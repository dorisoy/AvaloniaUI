using System;
using Avalonia.Controls.Automation.Peers;

namespace Avalonia.Platform
{
    public interface IAutomationPeerImpl : IDisposable
    {
        /// <summary>
        /// Called by the <see cref="AutomationPeer"/> when a property other than the parent or
        /// children changes.
        /// </summary>
        void PropertyChanged();

        /// <summary>
        /// Called by the <see cref="AutomationPeer"/> when the parent or children of the peer
        /// change.
        /// </summary>
        void StructureChanged();
    }
}

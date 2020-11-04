#nullable enable

using System;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Automation.Peers
{
    public class TabItemAutomationPeer : ContentControlAutomationPeerBase,
        ISelectableAutomationPeer
    {
        public TabItemAutomationPeer(Control owner) : base(owner) { }

        public bool GetIsSelected() => Owner.GetValue(TabItem.IsSelectedProperty);
        
        public ISelectingAutomationPeer? GetSelectionContainer()
        {
            if (Owner.Parent is Control parent)
            {
                var parentPeer = ControlAutomationPeer.GetOrCreatePeer(parent);
                return parentPeer as ISelectingAutomationPeer;
            }

            return null;
        }

        public void Select()
        {
            EnsureEnabled();

            if (Owner.Parent is SelectingItemsControl parent)
            {
                var index = parent.ItemContainerGenerator.IndexFromContainer(Owner);

                if (index != -1)
                    parent.SelectedIndex = index;
            }
        }

        void ISelectableAutomationPeer.AddToSelection() => throw new InvalidOperationException();
        
        void ISelectableAutomationPeer.RemoveFromSelection()
        {
            EnsureEnabled();
            if (GetIsSelected())
                throw new InvalidOperationException();
        }
    }
}

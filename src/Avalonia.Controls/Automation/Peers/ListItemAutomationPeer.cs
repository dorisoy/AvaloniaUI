using System;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;

#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public class ListItemAutomationPeer : ContentControlAutomationPeer,
        ISelectableAutomationPeer
    {
        public ListItemAutomationPeer(Control owner, AutomationRole role = AutomationRole.ListItem)
            : base(owner, role)
        {
        }

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

        void ISelectableAutomationPeer.AddToSelection()
        {
            EnsureEnabled();

            if (Owner.Parent is ItemsControl parent &&
                parent.GetValue(ListBox.SelectionProperty) is ISelectionModel selectionModel)
            {
                var index = parent.ItemContainerGenerator.IndexFromContainer(Owner);

                if (index != -1)
                    selectionModel.Select(index);
            }
        }

        void ISelectableAutomationPeer.RemoveFromSelection()
        {
            EnsureEnabled();

            if (Owner.Parent is ItemsControl parent &&
                parent.GetValue(ListBox.SelectionProperty) is ISelectionModel selectionModel)
            {
                var index = parent.ItemContainerGenerator.IndexFromContainer(Owner);

                if (index != -1)
                    selectionModel.Deselect(index);
            }
        }
    }
}

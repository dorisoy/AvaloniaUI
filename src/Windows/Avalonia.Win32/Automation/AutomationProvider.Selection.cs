using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal partial class AutomationProvider : ISelectionProvider, ISelectionItemProvider
    {
        private bool _canSelectMultiple;
        private bool _isSelectionRequired;
        private bool _isSelected;
        private IRawElementProviderSimple[]? _selection;

        bool ISelectionProvider.CanSelectMultiple => _canSelectMultiple;
        bool ISelectionProvider.IsSelectionRequired => _isSelectionRequired;
        bool ISelectionItemProvider.IsSelected => _isSelected;
        IRawElementProviderSimple? ISelectionItemProvider.SelectionContainer => null;

        IRawElementProviderSimple[] ISelectionProvider.GetSelection() => _selection ?? Array.Empty<IRawElementProviderSimple>();
        void ISelectionItemProvider.Select() => InvokeSync<ISelectableAutomationPeer>(x => x.Select());
        void ISelectionItemProvider.AddToSelection() => InvokeSync<ISelectableAutomationPeer>(x => x.AddToSelection());
        void ISelectionItemProvider.RemoveFromSelection() => InvokeSync<ISelectableAutomationPeer>(x => x.RemoveFromSelection());

        private void UpdateSelection(bool notify)
        {
            if (Peer is ISelectingAutomationPeer selectionPeer)
            {
                var selection = selectionPeer.GetSelection();
                var selectionMode = selectionPeer.GetSelectionMode();

                UpdateProperty(
                    UiaPropertyId.SelectionCanSelectMultiple,
                    ref _canSelectMultiple,
                    selectionMode.HasFlagCustom(SelectionMode.Multiple),
                    notify);
                UpdateProperty(
                    UiaPropertyId.SelectionIsSelectionRequired,
                    ref _isSelectionRequired,
                    selectionMode.HasFlagCustom(SelectionMode.AlwaysSelected),
                    notify);
                UpdateProperty(
                    UiaPropertyId.SelectionSelection,
                    ref _selection,
                    selection.Select(x => (IRawElementProviderSimple)x.PlatformImpl!).ToArray(),
                    notify);
            }

            if (Peer is ISelectableAutomationPeer selectablePeer)
            {
                UpdateProperty(
                    UiaPropertyId.SelectionItemIsSelected,
                    ref _isSelected,
                    selectablePeer.GetIsSelected(),
                    notify);
            }
        }
    }
}

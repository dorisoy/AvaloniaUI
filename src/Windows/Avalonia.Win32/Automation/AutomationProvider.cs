using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    [ComVisible(true)]
    internal class AutomationProvider : MarshalByRefObject,
        IAutomationPeerImpl,
        IRawElementProviderSimple,
        IRawElementProviderFragment,
        IInvokeProvider,
        ISelectionProvider
    {
        private readonly UiaControlTypeId _controlType;
        private readonly WeakReference<AutomationPeer> _peer;
        private readonly bool _isHidden;
        private AutomationProvider? _parent;
        private IRawElementProviderFragmentRoot? _fragmentRoot;
        private Rect _boundingRect;
        private List<AutomationProvider>? _children;
        private bool _childrenValid;
        private string? _className;
        private bool _isKeyboardFocusable;
        private string? _name;
        private bool _canSelectMultiple;
        private bool _isSelectionRequired;
        private IRawElementProviderSimple[]? _selection;
        private bool _isDisposed;

        public AutomationProvider(
            AutomationPeer peer,
            UiaControlTypeId controlType)
        {
            Dispatcher.UIThread.VerifyAccess();

            _peer = new WeakReference<AutomationPeer>(peer ?? throw new ArgumentNullException(nameof(peer)));
            _controlType = controlType;
            _isHidden = peer.IsHidden();
        }

        protected AutomationProvider(AutomationPeer peer)
        {
            Dispatcher.UIThread.VerifyAccess();

            _peer = new WeakReference<AutomationPeer>(peer ?? throw new ArgumentNullException(nameof(peer)));
            _controlType = UiaControlTypeId.Window;
        }

        public AutomationPeer Peer
        {
            get
            {
                _peer.TryGetTarget(out var value);
                return value;
            }
        }

        public Rect BoundingRectangle 
        { 
            get
            {
                if (Window is null)
                {
                    throw new NotSupportedException("Non-Window roots not yet supported.");
                }

                return new PixelRect(
                    Window.PointToScreen(_boundingRect.TopLeft),
                    Window.PointToScreen(_boundingRect.BottomRight))
                    .ToRect(1);
            }
        }

        public virtual IRawElementProviderFragmentRoot FragmentRoot
        {
            get
            {
                return _fragmentRoot ??= GetParent()?.FragmentRoot ??
                    throw new AvaloniaInternalException("Could not get FragmentRoot from parent.");
            }
        }
        
        public ProviderOptions ProviderOptions => ProviderOptions.ServerSideProvider;
        public WindowImpl? Window => (FragmentRoot as WindowProvider)?.Owner;
        public virtual IRawElementProviderSimple? HostRawElementProvider => null;
        bool ISelectionProvider.CanSelectMultiple => _canSelectMultiple;
        bool ISelectionProvider.IsSelectionRequired => _isSelectionRequired;

        public void Dispose() => _isDisposed = true;
        
        public void PropertyChanged() 
        {
            Dispatcher.UIThread.VerifyAccess();
            UpdateCore(true);
        }
        
        public void StructureChanged() 
        {
            _childrenValid = false;
            UiaCoreProviderApi.UiaRaiseStructureChangedEvent(this, StructureChangeType.ChildrenInvalidated, null, 0);
        }

        [return: MarshalAs(UnmanagedType.IUnknown)]
        public virtual object? GetPatternProvider(int patternId)
        {
            return (UiaPatternId)patternId switch
            {
                UiaPatternId.Invoke => Peer is IInvocableAutomationPeer ? this : null,
                UiaPatternId.Selection => Peer is ISelectingAutomationPeer ? this : null,
                _ => null,
            };
        }

        public virtual object? GetPropertyValue(int propertyId)
        {
            return (UiaPropertyId)propertyId switch
            {
                UiaPropertyId.ClassName => _className,
                UiaPropertyId.ControlType => _controlType,
                UiaPropertyId.IsContentElement => !_isHidden,
                UiaPropertyId.IsControlElement => !_isHidden,
                UiaPropertyId.IsKeyboardFocusable => _isKeyboardFocusable,
                UiaPropertyId.LocalizedControlType => _controlType.ToString().ToLowerInvariant(),
                UiaPropertyId.Name => _name,
                _ => null,
            };
        }

        public int[]? GetRuntimeId() => new int[] { 3, Peer.GetHashCode() };

        public virtual IRawElementProviderFragment? Navigate(NavigateDirection direction)
        {
            if (direction == NavigateDirection.Parent)
            {
                return GetParent();
            }

            EnsureChildren();

            return direction switch
            {
                NavigateDirection.NextSibling => GetParent()?.GetSibling(this, 1),
                NavigateDirection.PreviousSibling => GetParent()?.GetSibling(this, -1),
                NavigateDirection.FirstChild => _children?.FirstOrDefault(),
                NavigateDirection.LastChild => _children?.LastOrDefault(),
                _ => null,
            };
        }

        public void SetFocus()
        {
            InvokeSync(() => Peer.SetFocus());
        }

        public async Task Update(bool notify)
        {
            if (Dispatcher.UIThread.CheckAccess())
                UpdateCore(notify);
            else
                await Dispatcher.UIThread.InvokeAsync(() => Update(notify));
        }

        public override string ToString() => _className!;

        IRawElementProviderSimple[]? IRawElementProviderFragment.GetEmbeddedFragmentRoots() => null;

        void IInvokeProvider.Invoke()
        {
            if (Peer is IInvocableAutomationPeer i)
                InvokeSync(() => i.Invoke());
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection() => _selection ?? Array.Empty<IRawElementProviderSimple>();

        protected void InvokeSync(Action action)
        {
            if (_isDisposed)
                return;

            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(action).Wait();
            }
        }

        [return: MaybeNull]
        protected T InvokeSync<T>(Func<T> func)
        {
            if (_isDisposed)
                return default;

            if (Dispatcher.UIThread.CheckAccess())
            {
                return func();
            }
            else
            {
                return Dispatcher.UIThread.InvokeAsync(func).Result;
            }
        }

        protected virtual void UpdateCore(bool notify)
        {
            notify &= UiaCoreProviderApi.UiaClientsAreListening();

            _className = Peer.GetClassName();

            UpdateProperty(UiaPropertyId.BoundingRectangle, ref _boundingRect, Peer.GetBoundingRectangle(), notify);
            UpdateProperty(UiaPropertyId.IsKeyboardFocusable, ref _isKeyboardFocusable, Peer.IsKeyboardFocusable(), notify);
            UpdateProperty(UiaPropertyId.Name, ref _name, Peer.GetName(), notify);

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
        }

        private void UpdateProperty<T>(UiaPropertyId id, ref T _field, T value, bool notify)
        {
            if (!EqualityComparer<T>.Default.Equals(_field, value))
            {
                _field = value;
                if (notify)
                    UiaCoreProviderApi.UiaRaiseAutomationPropertyChangedEvent(this, (int)id, null, null);
            }
        }

        private AutomationProvider? GetParent()
        {
            if (_parent is null && !(this is IRawElementProviderFragmentRoot))
            {
                _parent = InvokeSync(() => Peer.GetParent())?.PlatformImpl as AutomationProvider ??
                    throw new AvaloniaInternalException($"Could not find parent AutomationProvider for {Peer}.");
            }

            return _parent;
        }

        private void EnsureChildren()
        {
            if (!_childrenValid)
            {
                InvokeSync(() => LoadChildren());
                _childrenValid = true;
            }
        }

        private void LoadChildren()
        {
            var childPeers = InvokeSync(() => Peer.GetChildren());

            _children?.Clear();

            if (childPeers is null)
                return;

            foreach (var childPeer in childPeers)
            {
                _children ??= new List<AutomationProvider>();

                if (childPeer.PlatformImpl is AutomationProvider child)
                {
                    _children.Add(child);
                }
                else
                {
                    throw new AvaloniaInternalException(
                        "AutomationPeer platform implementation not recognised.");
                }
            }
        }

        private IRawElementProviderFragment? GetSibling(AutomationProvider child, int direction)
        {
            EnsureChildren();

            var index = _children?.IndexOf(child) ?? -1;

            if (index >= 0)
            {
                index += direction;

                if (index >= 0 && index < _children!.Count)
                {
                    return _children[index];
                }
            }

            return null;
        }
    }
}

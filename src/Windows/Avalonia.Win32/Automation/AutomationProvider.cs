using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Automation;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Utilities;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    [ComVisible(true)]
    internal class AutomationProvider : MarshalByRefObject,
        IAutomationPeerImpl,
        IRawElementProviderSimple,
        IRawElementProviderSimple2,
        IRawElementProviderFragment,
        IExpandCollapseProvider,
        IInvokeProvider,
        IScrollProvider,
        IScrollItemProvider,
        ISelectionProvider,
        ISelectionItemProvider,
        IToggleProvider,
        IValueProvider
    {
        private readonly UiaControlTypeId _controlType;
        private readonly bool _isControlElement;
        private readonly WeakReference<AutomationPeer> _peer;
        private readonly WindowImpl _visualRoot;
        private readonly IRawElementProviderFragmentRoot _fragmentRoot;
        private AutomationProvider? _parent;
        private Rect _boundingRect;
        private List<AutomationProvider>? _children;
        private bool _childrenValid;
        private string? _className;
        private bool _hasKeyboardFocus;
        private bool _isKeyboardFocusable;
        private bool _isEnabled;
        private string? _name;
        private ExpandCollapseState _expandCollapseState;
        private bool _canSelectMultiple;
        private bool _isSelectionRequired;
        private bool _isSelected;
        private IRawElementProviderSimple[]? _selection;
        private bool _isDisposed;

        public AutomationProvider(
            AutomationPeer peer,
            UiaControlTypeId controlType,
            bool isControlElement,
            WindowImpl visualRoot,
            IRawElementProviderFragmentRoot fragmentRoot)
        {
            Dispatcher.UIThread.VerifyAccess();

            _peer = new WeakReference<AutomationPeer>(peer ?? throw new ArgumentNullException(nameof(peer)));
            _controlType = controlType;
            _isControlElement = isControlElement;
            _visualRoot = visualRoot ?? throw new ArgumentNullException(nameof(visualRoot));
            _fragmentRoot = fragmentRoot ?? throw new ArgumentNullException(nameof(fragmentRoot));
        }

        protected AutomationProvider(AutomationPeer peer, WindowImpl visualRoot)
        {
            Dispatcher.UIThread.VerifyAccess();

            _peer = new WeakReference<AutomationPeer>(peer ?? throw new ArgumentNullException(nameof(peer)));
            _controlType = UiaControlTypeId.Window;
            _visualRoot = visualRoot;
            _fragmentRoot = (IRawElementProviderFragmentRoot)this;
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
                return new PixelRect(
                    _visualRoot.PointToScreen(_boundingRect.TopLeft),
                    _visualRoot.PointToScreen(_boundingRect.BottomRight))
                    .ToRect(1);
            }
        }

        public virtual IRawElementProviderFragmentRoot FragmentRoot => _fragmentRoot;
        
        public ProviderOptions ProviderOptions => ProviderOptions.ServerSideProvider;
        public virtual IRawElementProviderSimple? HostRawElementProvider => null;
        bool ISelectionProvider.CanSelectMultiple => _canSelectMultiple;
        bool ISelectionProvider.IsSelectionRequired => _isSelectionRequired;
        bool ISelectionItemProvider.IsSelected => _isSelected;
        IRawElementProviderSimple? ISelectionItemProvider.SelectionContainer => null;
        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState => _expandCollapseState;

        double IScrollProvider.HorizontalScrollPercent
        {
            get => InvokeSync<IScrollableAutomationPeer, double>(
                x => x.Offset.X * 100 / (x.Extent.Width - x.Viewport.Width));
        }

        double IScrollProvider.VerticalScrollPercent
        {
            get => InvokeSync<IScrollableAutomationPeer, double>(
                x => x.Offset.Y * 100 / (x.Extent.Height - x.Viewport.Height));
        }

        double IScrollProvider.HorizontalViewSize
        {
            get
            {
                return InvokeSync<IScrollableAutomationPeer, double>(x =>
                {
                    if (MathUtilities.IsZero(x.Extent.Width))
                        return 100;
                    return Math.Min(100, x.Viewport.Width / x.Extent.Width);
                });
            }
        }

        double IScrollProvider.VerticalViewSize
        {
            get
            {
                return InvokeSync<IScrollableAutomationPeer, double>(x =>
                {
                    if (MathUtilities.IsZero(x.Extent.Height))
                        return 100;
                    return Math.Min(100, x.Viewport.Height / x.Extent.Height);
                });
            }
        }
        bool IScrollProvider.HorizontallyScrollable
        {
            get => InvokeSync<IScrollableAutomationPeer, bool>(x => x.Extent.Width > x.Viewport.Width);
        }

        bool IScrollProvider.VerticallyScrollable
        {
            get => InvokeSync<IScrollableAutomationPeer, bool>(x => x.Extent.Height > x.Viewport.Height);
        }

        ToggleState IToggleProvider.ToggleState
        {
            get
            {
                return InvokeSync<IToggleableAutomationPeer, bool?>(x => x.GetToggleState()) switch
                {
                    true => ToggleState.On,
                    false => ToggleState.Off,
                    null => ToggleState.Indeterminate,
                };
            }
        }

        string? IValueProvider.Value => InvokeSync<IStringValueAutomationPeer, string>(x => x.GetValue());
        bool IValueProvider.IsReadOnly => false;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            UiaCoreProviderApi.UiaDisconnectProvider(this);
        }
        
        public void PropertyChanged() 
        {
            if (_isDisposed)
                return;

            Dispatcher.UIThread.VerifyAccess();
            UpdateCore(true);
        }
        
        public void StructureChanged() 
        {
            if (_isDisposed)
                return;

            _childrenValid = false;
            UiaCoreProviderApi.UiaRaiseStructureChangedEvent(this, StructureChangeType.ChildrenInvalidated, null, 0);
        }

        [return: MarshalAs(UnmanagedType.IUnknown)]
        public virtual object? GetPatternProvider(int patternId)
        {
            if (_isDisposed)
                return null;

            return (UiaPatternId)patternId switch
            {
                UiaPatternId.ExpandCollapse => Peer is IOpenCloseAutomationPeer ? this : null,
                UiaPatternId.Invoke => Peer is IInvocableAutomationPeer ? this : null,
                UiaPatternId.Scroll => Peer is IScrollableAutomationPeer ? this : null,
                UiaPatternId.ScrollItem => this,
                UiaPatternId.Selection => Peer is ISelectingAutomationPeer ? this : null,
                UiaPatternId.SelectionItem => Peer is ISelectableAutomationPeer ? this : null,
                UiaPatternId.Toggle => Peer is IToggleableAutomationPeer ? this : null,
                UiaPatternId.Value => Peer is IStringValueAutomationPeer ? this : null,
                _ => null,
            };
        }

        public virtual object? GetPropertyValue(int propertyId)
        {
            if (_isDisposed)
                return null;

            object? result = (UiaPropertyId)propertyId switch
            {
                UiaPropertyId.ClassName => _className,
                UiaPropertyId.ClickablePoint => new[] { BoundingRectangle.Center.X, BoundingRectangle.Center.Y },
                UiaPropertyId.ControlType => _controlType,
                UiaPropertyId.Culture => CultureInfo.CurrentCulture.LCID,
                UiaPropertyId.HasKeyboardFocus => _hasKeyboardFocus,
                UiaPropertyId.IsContentElement => _isControlElement,
                UiaPropertyId.IsControlElement => _isControlElement,
                UiaPropertyId.IsEnabled => _isEnabled,
                UiaPropertyId.IsKeyboardFocusable => _isKeyboardFocusable,
                UiaPropertyId.LocalizedControlType => _controlType.ToString().ToLowerInvariant(),
                UiaPropertyId.Name => _name,
                UiaPropertyId.ProcessId => Process.GetCurrentProcess().Id,
                UiaPropertyId.RuntimeId => GetRuntimeId(),
                _ => null,
            };

            return result;
        }

        public int[]? GetRuntimeId() => new int[] { 3, Peer.GetHashCode() };

        public virtual IRawElementProviderFragment? Navigate(NavigateDirection direction)
        {
            if (_isDisposed)
                return null;

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
            if (_isDisposed)
                return;

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
        void IRawElementProviderSimple2.ShowContextMenu() => InvokeSync(() => Peer.ShowContextMenu());

        void IExpandCollapseProvider.Expand() => InvokeSync<IOpenCloseAutomationPeer>(x => x.Open());
        void IExpandCollapseProvider.Collapse() => InvokeSync<IOpenCloseAutomationPeer>(x => x.Close());
        void IInvokeProvider.Invoke() => InvokeSync<IInvocableAutomationPeer>(x => x.Invoke());
        IRawElementProviderSimple[] ISelectionProvider.GetSelection() => _selection ?? Array.Empty<IRawElementProviderSimple>();
        void ISelectionItemProvider.Select() => InvokeSync<ISelectableAutomationPeer>(x => x.Select());
        void ISelectionItemProvider.AddToSelection() => InvokeSync<ISelectableAutomationPeer>(x => x.AddToSelection());
        void ISelectionItemProvider.RemoveFromSelection() => InvokeSync<ISelectableAutomationPeer>(x => x.RemoveFromSelection());


        void IScrollProvider.Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
        {
            switch (verticalAmount)
            {
                case ScrollAmount.LargeDecrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.PageUp());
                    break;
                case ScrollAmount.SmallDecrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.LineUp());
                    break;
                case ScrollAmount.SmallIncrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.LineDown());
                    break;
                case ScrollAmount.LargeIncrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.PageDown());
                    break;
            }

            switch (horizontalAmount)
            {
                case ScrollAmount.LargeDecrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.PageLeft());
                    break;
                case ScrollAmount.SmallDecrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.LineLeft());
                    break;
                case ScrollAmount.SmallIncrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.LineRight());
                    break;
                case ScrollAmount.LargeIncrement:
                    InvokeSync<IScrollableAutomationPeer>(x => x.PageRight());
                    break;
            }
        }

        void IScrollProvider.SetScrollPercent(double horizontalPercent, double verticalPercent)
        {
            InvokeSync<IScrollableAutomationPeer>(x =>
            {
                var sx = horizontalPercent >= 0 && horizontalPercent <= 100 ?
                    (x.Extent.Width - x.Viewport.Width) * horizontalPercent :
                    x.Offset.X;
                var sy = verticalPercent >= 0 && verticalPercent <= 100 ?
                    (x.Extent.Height - x.Viewport.Height) * verticalPercent :
                    x.Offset.Y;
                x.Offset = new Vector(sx, sy);
            });
        }

        void IScrollItemProvider.ScrollIntoView()
        {
            InvokeSync(() => Peer.BringIntoView());
        }

        void IToggleProvider.Toggle() => InvokeSync<IToggleableAutomationPeer>(x => x.Toggle());

        void IValueProvider.SetValue(string value) => InvokeSync<IStringValueAutomationPeer>(x => x.SetValue(value));

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

        protected void InvokeSync<TInterface>(Action<TInterface> action)
        {
            if (Peer is TInterface i)
            {
                try
                {
                    InvokeSync(() => action(i));
                }
                catch (AggregateException e) when (e.InnerException is ElementNotEnabledException)
                {
                    throw new COMException(e.Message, UiaCoreProviderApi.UIA_E_ELEMENTNOTENABLED);
                }
            }
        }

        [return: MaybeNull]
        protected TResult InvokeSync<TInterface, TResult>(Func<TInterface, TResult> func)
        {
            if (Peer is TInterface i)
            {
                try
                {
                    return InvokeSync(() => func(i));
                }
                catch (AggregateException e) when (e.InnerException is ElementNotEnabledException)
                {
                    throw new COMException(e.Message, UiaCoreProviderApi.UIA_E_ELEMENTNOTENABLED);
                }
            }

            return default;
        }

        protected virtual void UpdateCore(bool notify)
        {
            _className = Peer.GetClassName();

            UpdateProperty(UiaPropertyId.BoundingRectangle, ref _boundingRect, Peer.GetBoundingRectangle(), notify);
            UpdateProperty(UiaPropertyId.HasKeyboardFocus, ref _hasKeyboardFocus, Peer.HasKeyboardFocus(), notify);
            UpdateProperty(UiaPropertyId.IsKeyboardFocusable, ref _isKeyboardFocusable, Peer.IsKeyboardFocusable(), notify);
            UpdateProperty(UiaPropertyId.IsEnabled, ref _isEnabled, Peer.IsEnabled(), notify);
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

            if (Peer is ISelectableAutomationPeer selectablePeer)
            {
                UpdateProperty(
                    UiaPropertyId.SelectionItemIsSelected,
                    ref _isSelected,
                    selectablePeer.GetIsSelected(),
                    notify);
            }

            if (Peer is IOpenCloseAutomationPeer openClosePeer)
            {
                UpdateProperty(
                    UiaPropertyId.ExpandCollapseExpandCollapseState,
                    ref _expandCollapseState,
                    openClosePeer.GetIsOpen() ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
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

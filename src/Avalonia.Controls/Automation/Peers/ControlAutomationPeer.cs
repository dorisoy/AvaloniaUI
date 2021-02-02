using System;
using System.Collections.Generic;
using Avalonia.Controls.Platform;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.VisualTree;

#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    /// <summary>
    /// An automation peer which represents a <see cref="Control"/> element.
    /// </summary>
    public class ControlAutomationPeer : AutomationPeer
    {
        private readonly AutomationRole _role;
        private readonly EventHandler<VisualTreeAttachmentEventArgs> _invalidateChildren;
        private List<AutomationPeer>? _children;
        private List<WeakReference<Control>>? _subscribedChildren;
        private bool _childrenValid;

        public ControlAutomationPeer(Control owner, AutomationRole role)
        {
            Owner = owner ?? throw new ArgumentNullException("owner");

            _role = role;
            _invalidateChildren = InvalidateStructure;

            owner.PropertyChanged += OwnerPropertyChanged;
            owner.AttachedToVisualTree += OwnerVisualTreeAttachedDetached;
            owner.DetachedFromVisualTree += OwnerVisualTreeAttachedDetached;
            
            var logicalChildren = ((ILogical)owner).LogicalChildren;
            logicalChildren.CollectionChanged += InvalidateStructure;
        }

        public Control Owner { get; }

        public static AutomationPeer GetOrCreatePeer(Control element)
        {
            element = element ?? throw new ArgumentNullException("element");
            return element.GetOrCreateAutomationPeer();
        }

        protected override void BringIntoViewCore() => Owner.BringIntoView();

        protected override IAutomationPeerImpl CreatePlatformImplCore()
        {
            return GetPlatformImplFactory()?.CreateAutomationPeerImpl(this) ??
                DetachedPlatformImpl.Instance;
        }

        protected override Rect GetBoundingRectangleCore()
        {
            var root = Owner.GetVisualRoot();

            if (root is null)
                return Rect.Empty;

            var t = Owner.TransformToVisual(root);

            if (!t.HasValue)
                return Rect.Empty;

            return new Rect(Owner.Bounds.Size).TransformToAABB(t.Value);
        }

        protected override int GetChildCountCore()
        {
            var logicalChildren = ((ILogical)Owner).LogicalChildren;
            var result = 0;

            foreach (var child in logicalChildren)
            {
                if (child is Control c && ((IVisual)c).IsAttachedToVisualTree)
                    ++result;
            }

            return result;
        }

        protected override IReadOnlyList<AutomationPeer>? GetChildrenCore()
        {
            var logicalChildren = ((ILogical)Owner).LogicalChildren;

            if (!_childrenValid)
            {
                if (_children is null && logicalChildren.Count > 0)
                    _children = new List<AutomationPeer>();

                if (_subscribedChildren is object)
                {
                    foreach (var c in _subscribedChildren)
                    {
                        if (c.TryGetTarget(out var target))
                        {
                            target.AttachedToVisualTree -= _invalidateChildren;
                            target.DetachedFromVisualTree -= _invalidateChildren;
                        }
                    }

                    _subscribedChildren.Clear();
                }

                var i = -1;

                foreach (var child in logicalChildren)
                {
                    if (child is Control c)
                    {
                        if (((IVisual)c).IsAttachedToVisualTree)
                        {
                            var peer = GetOrCreatePeer(c);

                            if (_children!.Count <= ++i)
                                _children.Add(peer);
                            else
                                _children[i] = peer;
                        }

                        _subscribedChildren ??= new List<WeakReference<Control>>();
                        _subscribedChildren.Add(new WeakReference<Control>(c));
                        c.AttachedToVisualTree += _invalidateChildren;
                        c.DetachedFromVisualTree += _invalidateChildren;
                    }
                }

                if (_children?.Count > ++i)
                {
                    _children.RemoveRange(i, _children.Count - i);
                }

                _childrenValid = true;
            }

            return _children;
        }

        protected override string GetClassNameCore() => Owner.GetType().Name;
        protected override string? GetNameCore() => AutomationProperties.GetName(Owner);

        protected override AutomationPeer? GetParentCore()
        {
            return Owner.Parent switch
            {
                Control c => GetOrCreatePeer(c),
                null => null,
                _ => throw new NotSupportedException("Don't know how to create a peer for a non-Control parent."),
            };
        }

        protected override AutomationRole GetRoleCore() => _role;
        protected override bool HasKeyboardFocusCore() => Owner.IsFocused;
        protected override bool IsEnabledCore() => Owner.IsEnabled;
        protected override bool IsKeyboardFocusableCore() => Owner.Focusable;
        protected override void SetFocusCore() => Owner.Focus();
        
        protected override bool ShowContextMenuCore()
        {
            var c = Owner;

            while (c is object)
            {
                if (c.ContextMenu is object)
                {
                    c.ContextMenu.Open(c);
                    return true;
                }

                c = c.Parent as Control;
            }

            return false;
        }

        private IPlatformAutomationInterface? GetPlatformImplFactory()
        {
            var root = Owner.GetVisualRoot();

            // We only create a real (i.e. not detached) platform impl if the control is attached
            // to the visual tree.
            if (root is null || !root.IsVisible)
                return null;

            return (root as TopLevel)?.PlatformImpl as IPlatformAutomationInterface;
        }

        private void InvalidateProperties()
        {
            _childrenValid = false;
            PlatformImpl!.PropertyChanged();
        }

        private void InvalidateStructure()
        {
            _childrenValid = false;
            PlatformImpl!.StructureChanged();
        }

        private void InvalidateStructure(object sender, EventArgs e) => InvalidateStructure();

        private void OwnerVisualTreeAttachedDetached(object sender, VisualTreeAttachmentEventArgs e)
        {
            InvalidatePlatformImpl();
        }

        private void OwnerPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case nameof(Visual.TransformedBounds):
                    InvalidateProperties();
                    break;
            }
        }

        // When a control is detched from the visual tree, we use a stub platform impl.
        internal class DetachedPlatformImpl : IAutomationPeerImpl
        {
            public static readonly DetachedPlatformImpl Instance = new DetachedPlatformImpl();
            private DetachedPlatformImpl() { }
            public void Dispose() { }
            public void PropertyChanged() { }
            public void StructureChanged() { }
        }
    }
}


using System;
using System.Collections.Generic;
using Avalonia.Controls.Platform;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.VisualTree;

#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public abstract class ControlAutomationPeer : AutomationPeer
    {
        private readonly EventHandler<VisualTreeAttachmentEventArgs> _invalidateChildren;
        private List<AutomationPeer>? _children;
        private List<WeakReference<Control>>? _subscribedChildren;
        private bool _childrenValid;

        public ControlAutomationPeer(Control owner)
        {
            Owner = owner ?? throw new ArgumentNullException("owner");
            _invalidateChildren = InvalidateChildren;

            var logicalChildren = ((ILogical)owner).LogicalChildren;
            logicalChildren.CollectionChanged += InvalidateChildren;
        }

        public Control Owner { get; }

        public static AutomationPeer GetOrCreatePeer(Control element)
        {
            element = element ?? throw new ArgumentNullException("element");
            return element.GetOrCreateAutomationPeer();
        }

        protected override IAutomationPeerImpl CreatePlatformImplCore()
        {
            var root = Owner.GetVisualRoot() as TopLevel ??
                throw new InvalidOperationException("Cannot create automation peer for non-rooted control.");
            var factory = root.PlatformImpl as IPlatformAutomationInterface ??
                throw new InvalidOperationException("UI Automation is not enabled for this platform.");
            return factory.CreateAutomationPeerImpl(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!IsDisposed)
            {
                var logicalChildren = ((ILogical)Owner).LogicalChildren;
                logicalChildren.CollectionChanged -= InvalidateChildren;
                _children = null;
            }
        }

        protected override Rect GetBoundingRectangleCore()
        {
            var root = Owner.GetVisualRoot();

            if (root is null)
                return Rect.Empty;

            var t = Owner.TransformToVisual(root);

            if (!t.HasValue)
                return Rect.Empty;

            return Owner.Bounds.TransformToAABB(t.Value);
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

        protected override bool IsHiddenCore() => false;
        protected override bool IsKeyboardFocusableCore() => Owner.Focusable;
        protected override void SetFocusCore() => Owner.Focus();

        private void InvalidateChildren()
        {
            if (!IsDisposed)
            {
                _childrenValid = false;
                PlatformImpl!.StructureChanged();
            }
        }

        private void InvalidateChildren(object sender, EventArgs e) => InvalidateChildren();
    }
}


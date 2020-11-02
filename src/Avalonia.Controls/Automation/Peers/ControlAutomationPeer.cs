using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls.Platform;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.VisualTree;

#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public abstract class ControlAutomationPeer : AutomationPeer
    {
        private List<AutomationPeer>? _children;
        private bool _childrenValid;

        public ControlAutomationPeer(Control owner)
        {
            Owner = owner ?? throw new ArgumentNullException("owner");

            var logicalChildren = ((ILogical)owner).LogicalChildren;
            logicalChildren.CollectionChanged += LogicalChildrenChanged;
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
                logicalChildren.CollectionChanged -= LogicalChildrenChanged;
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

        protected override int GetChildCountCore() => ((IVisual)Owner).VisualChildren.Count;

        protected override IReadOnlyList<AutomationPeer>? GetChildrenCore()
        {
            var logicalChildren = ((ILogical)Owner).LogicalChildren;

            if (!_childrenValid)
            {
                if (_children is null && logicalChildren.Count > 0)
                {
                    _children = new List<AutomationPeer>();
                }

                for (var i = 0; i < logicalChildren.Count; ++i)
                {
                    if (logicalChildren[i] is Control c && ((IVisual)c).IsAttachedToVisualTree)
                    {
                        var peer = GetOrCreatePeer(c);

                        if (_children!.Count <= i)
                        {
                            _children.Add(peer);
                        }
                        else
                        {
                            _children[i] = peer;
                        }
                    }
                }

                if (_children?.Count > logicalChildren.Count)
                {
                    _children.RemoveRange(logicalChildren.Count, _children.Count - logicalChildren.Count);
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

        private void LogicalChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsDisposed)
            {
                _childrenValid = false;
                PlatformImpl!.StructureChanged();
            }
        }
    }
}


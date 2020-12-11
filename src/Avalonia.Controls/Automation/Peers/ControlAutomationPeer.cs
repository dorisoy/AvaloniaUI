using System;
using System.Collections.Generic;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
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
            _role = role;
            _invalidateChildren = InvalidateStructure;

            Owner = owner ?? throw new ArgumentNullException("owner");
            owner.PropertyChanged += OwnerPropertyChanged;
            
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
            return GetPlatformImplFactory().CreateAutomationPeerImpl(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!IsDisposed)
            {
                Owner.PropertyChanged -= OwnerPropertyChanged;

                var logicalChildren = ((ILogical)Owner).LogicalChildren;
                logicalChildren.CollectionChanged -= InvalidateStructure;
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

        private IPlatformAutomationInterface GetPlatformImplFactory()
        {
            var root = Owner.GetVisualRoot() as TopLevel;

            while (root is object)
            {
                if (root?.PlatformImpl is IPlatformAutomationInterface i)
                    return i;
                root = (root as IHostedVisualTreeRoot)?.Host?.GetVisualRoot() as TopLevel;
            }

            throw new InvalidOperationException("Cannot create automation peer for non-rooted control.");
        }

        private void InvalidateProperties()
        {
            if (!IsDisposed)
            {
                _childrenValid = false;
                PlatformImpl!.PropertyChanged();
            }
        }

        private void InvalidateStructure()
        {
            if (!IsDisposed)
            {
                _childrenValid = false;
                PlatformImpl!.StructureChanged();
            }
        }

        private void InvalidateStructure(object sender, EventArgs e) => InvalidateStructure();

        private void OwnerPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case nameof(Visual.TransformedBounds):
                    InvalidateProperties();
                    break;
            }
        }
    }
}


using System;
using System.Collections.Generic;
using Avalonia.Platform;

#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    /// <summary>
    /// Provides a base class that exposes an element to UI Automation.
    /// </summary>
    public abstract class AutomationPeer : IDisposable
    {
        ~AutomationPeer() => Dispose(false);

        /// <summary>
        /// Gets the platform implementation of the automation peer.
        /// </summary>
        public IAutomationPeerImpl? PlatformImpl { get; private set; }

        /// <summary>
        /// Gets a value describing whether the automation peer has been disposed.
        /// </summary>
        public bool IsDisposed => PlatformImpl is null;

        /// <summary>
        /// Attempts to bring the element associated with the automation peer into view.
        /// </summary>
        public void BringIntoView() => BringIntoViewCore();

        /// <summary>
        /// Releases all resources used by the automation peer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the bounding rectangle of the element that is associated with the automation peer
        /// in top-level coordinates.
        /// </summary>
        public Rect GetBoundingRectangle() => GetBoundingRectangleCore();

        /// <summary>
        /// Gets the number of child automation peers.
        /// </summary>
        public int GetChildCount() => GetChildCountCore();

        /// <summary>
        /// Gets the child automation peers.
        /// </summary>
        public IReadOnlyList<AutomationPeer> GetChildren() => GetChildrenCore() ?? Array.Empty<AutomationPeer>();

        /// <summary>
        /// Gets a string that describes the class of the element.
        /// </summary>
        public string GetClassName() => GetClassNameCore() ?? string.Empty;

        /// <summary>
        /// Gets text that describes the element that is associated with this automation peer.
        /// </summary>
        public string GetName() => GetNameCore() ?? string.Empty;

        /// <summary>
        /// Gets the <see cref="AutomationPeer"/> that is the parent of this <see cref="AutomationPeer"/>.
        /// </summary>
        /// <returns></returns>
        public AutomationPeer? GetParent() => GetParentCore();

        /// <summary>
        /// Gets an <see cref="AutomationPeer"/> from the specified point.
        /// </summary>
        /// <param name="point">The point, in window coordinates.</param>
        public AutomationPeer? GetPeerFromPoint(Point point) => GetPeerFromPointCore(point);

        /// <summary>
        /// Gets the role of the element that is associated with this automation peer.
        /// </summary>
        public AutomationRole GetRole() => GetRoleCore();

        /// <summary>
        /// Gets a value that indicates whether the element that is associated with this automation
        /// peer currently has keyboard focus.
        /// </summary>
        public bool HasKeyboardFocus() => HasKeyboardFocusCore();

        /// <summary>
        /// Gets a value indicating whether the control is enabled for user interaction.
        /// </summary>
        public bool IsEnabled() => IsEnabledCore();

        /// <summary>
        /// Gets a value that indicates whether the element can accept keyboard focus.
        /// </summary>
        /// <returns></returns>
        public bool IsKeyboardFocusable() => IsKeyboardFocusableCore();

        /// <summary>
        /// Sets the keyboard focus on the element that is associated with this automation peer.
        /// </summary>
        public void SetFocus() => SetFocusCore();

        protected abstract void BringIntoViewCore();
        protected abstract IAutomationPeerImpl CreatePlatformImplCore();
        protected abstract Rect GetBoundingRectangleCore();
        protected abstract int GetChildCountCore();
        protected abstract IReadOnlyList<AutomationPeer>? GetChildrenCore();
        protected abstract string GetClassNameCore();
        protected abstract string? GetNameCore();
        protected abstract AutomationPeer? GetParentCore();
        protected abstract AutomationRole GetRoleCore();
        protected abstract bool HasKeyboardFocusCore();
        protected abstract bool IsEnabledCore();
        protected abstract bool IsKeyboardFocusableCore();
        protected abstract void SetFocusCore();

        protected virtual void Dispose(bool disposing)
        {
            PlatformImpl?.Dispose();
            PlatformImpl = null;
        }

        protected virtual AutomationPeer? GetPeerFromPointCore(Point point)
        {
            foreach (var child in GetChildren())
            {
                var found = child.GetPeerFromPoint(point);
                if (found is object)
                    return found;
            }

            return GetBoundingRectangle().Contains(point) ? this : null;
        }

        protected void EnsureEnabled()
        {
            if (!IsEnabled())
                throw new ElementNotEnabledException();
        }

        internal void CreatePlatformImpl()
        {
            PlatformImpl = CreatePlatformImplCore() ??
                throw new InvalidOperationException("CreatePlatformImplCore returned null.");
        }
    }
}

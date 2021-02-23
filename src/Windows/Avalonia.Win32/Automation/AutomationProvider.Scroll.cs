using System;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Utilities;
using Avalonia.Win32.Interop.Automation;

#nullable enable

namespace Avalonia.Win32.Automation
{
    internal partial class AutomationProvider : IScrollProvider, IScrollItemProvider
    {
        private double _horizontalScrollPercent;
        private double _verticalScrollPercent;
        private double _horizontalViewSize;
        private double _verticalViewSize;
        private bool _horizontallyScrollable;
        private bool _verticallyScrollable;

        double IScrollProvider.HorizontalScrollPercent => _horizontalScrollPercent;
        double IScrollProvider.VerticalScrollPercent => _verticalScrollPercent;
        double IScrollProvider.HorizontalViewSize => _horizontalViewSize;
        double IScrollProvider.VerticalViewSize => _verticalViewSize;
        bool IScrollProvider.HorizontallyScrollable => _horizontallyScrollable;
        bool IScrollProvider.VerticallyScrollable => _verticallyScrollable;

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
                var extent = x.GetExtent();
                var offset = x.GetOffset();
                var viewport = x.GetViewport();
                var sx = horizontalPercent >= 0 && horizontalPercent <= 100 ?
                    (extent.Width - viewport.Width) * horizontalPercent :
                    offset.X;
                var sy = verticalPercent >= 0 && verticalPercent <= 100 ?
                    (extent.Height - viewport.Height) * verticalPercent :
                    offset.Y;
                x.SetOffset(new Vector(sx, sy));
            });
        }

        void IScrollItemProvider.ScrollIntoView()
        {
            InvokeSync(() => Peer.BringIntoView());
        }

        private void UpdateScroll(bool notify)
        {
            if (Peer is IScrollableAutomationPeer peer)
            {
                UpdateProperty(
                    UiaPropertyId.ScrollHorizontalScrollPercent,
                    ref _horizontalScrollPercent,
                    peer.GetOffset().X * 100 / (peer.GetExtent().Width - peer.GetViewport().Width),
                    notify);
                UpdateProperty(
                    UiaPropertyId.ScrollVerticalScrollPercent,
                    ref _verticalScrollPercent,
                    peer.GetOffset().Y * 100 / (peer.GetExtent().Height - peer.GetViewport().Height),
                    notify);
                UpdateProperty(
                    UiaPropertyId.ScrollHorizontalViewSize,
                    ref _horizontalViewSize,
                    MathUtilities.IsZero(peer.GetExtent().Width) ?
                        100 :
                        Math.Min(100, peer.GetViewport().Width / peer.GetExtent().Width),
                    notify);
                UpdateProperty(
                    UiaPropertyId.ScrollVerticalViewSize,
                    ref _verticalViewSize,
                    MathUtilities.IsZero(peer.GetExtent().Height) ?
                        100 :
                        Math.Min(100, peer.GetViewport().Height / peer.GetExtent().Height),
                    notify);
                UpdateProperty(
                    UiaPropertyId.ScrollHorizontallyScrollable,
                    ref _horizontallyScrollable,
                    peer.GetExtent().Width > peer.GetViewport().Width,
                    notify);
                UpdateProperty(
                    UiaPropertyId.ScrollVerticallyScrollable,
                    ref _verticallyScrollable,
                    peer.GetExtent().Height > peer.GetViewport().Height,
                    notify);
            }
        }
    }
}

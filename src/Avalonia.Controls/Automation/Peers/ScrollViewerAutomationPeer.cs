namespace Avalonia.Controls.Automation.Peers
{
    public class ScrollViewerAutomationPeer : ControlAutomationPeer, IScrollableAutomationPeer
    {
        public ScrollViewerAutomationPeer(Control owner, AutomationRole role = AutomationRole.ScrollArea)
            : base(owner, role)
        {
        }

        public Size Extent => Owner.GetValue(ScrollViewer.ExtentProperty);
        public Size Viewport => Owner.GetValue(ScrollViewer.ViewportProperty);

        public Vector Offset 
        {
            get => Owner.GetValue(ScrollViewer.OffsetProperty);
            set => Owner.SetValue(ScrollViewer.OffsetProperty, value);
        }

        public void LineDown() => (Owner as ScrollViewer)?.LineDown();
        public void LineLeft() => (Owner as ScrollViewer)?.LineLeft();
        public void LineRight() => (Owner as ScrollViewer)?.LineRight();
        public void LineUp() => (Owner as ScrollViewer)?.LineUp();
        public void PageDown() => (Owner as ScrollViewer)?.PageDown();
        public void PageLeft() => (Owner as ScrollViewer)?.PageLeft();
        public void PageRight() => (Owner as ScrollViewer)?.PageRight();
        public void PageUp() => (Owner as ScrollViewer)?.PageUp();
    }
}

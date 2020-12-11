namespace Avalonia.Controls.Automation.Peers
{
    public interface IScrollableAutomationPeer
    {
        Size Extent { get; }
        Vector Offset { get; set; }
        Size Viewport { get; }
        void PageUp();
        void PageDown();
        void PageLeft();
        void PageRight();
        void LineUp();
        void LineDown();
        void LineLeft();
        void LineRight();
    }
}

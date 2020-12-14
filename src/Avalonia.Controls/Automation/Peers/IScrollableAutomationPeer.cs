namespace Avalonia.Controls.Automation.Peers
{
    public interface IScrollableAutomationPeer
    {
        Size GetExtent();
        Vector GetOffset();
        Size GetViewport();
        void SetOffset(Vector value);
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

namespace Avalonia.Controls.Automation.Peers
{
    public interface IOpenCloseAutomationPeer
    {
        bool GetIsOpen();
        void Open();
        void Close();
    }
}

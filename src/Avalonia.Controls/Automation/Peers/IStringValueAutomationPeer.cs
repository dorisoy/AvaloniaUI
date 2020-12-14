#nullable enable

namespace Avalonia.Controls.Automation.Peers
{
    public interface IStringValueAutomationPeer
    {
        public string? GetValue();
        public void SetValue(string? value);
    }
}

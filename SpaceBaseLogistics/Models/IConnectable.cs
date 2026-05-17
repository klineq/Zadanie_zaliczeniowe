namespace SpaceBaseLogistics.Models;

// [WYMÓG 7: Interfejsy / Abstrakcja]
public interface IConnectable
{
    string ConnectionId { get; }
    bool IsConnected { get; }
    void ConnectTo(string targetId);
    void Disconnect();
}

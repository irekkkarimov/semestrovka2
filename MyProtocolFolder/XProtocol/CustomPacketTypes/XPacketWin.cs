namespace XProtocol.CustomPacketTypes;

public class XPacketWin
{
    [XField(1)] public string WinnerUsername;
    [XField(2)] public string Goal;
    [XField(3)] public string Message;
}
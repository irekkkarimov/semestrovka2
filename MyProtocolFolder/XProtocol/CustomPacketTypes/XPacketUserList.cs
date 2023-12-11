namespace XProtocol.CustomPacketTypes;

public class XPacketUserList
{
    [XField(1)] public List<string> UserList;
    [XField(2)] public string TurnUser;
}
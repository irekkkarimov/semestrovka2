namespace XProtocol;

public enum XPacketType
{
    Unknown,
    Handshake,
    String,
    Username,
    UserList,
    UserDisconnection,
    TurnRequest,
    TurnResponse,
    CardJson
}
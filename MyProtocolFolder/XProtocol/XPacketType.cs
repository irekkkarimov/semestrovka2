namespace XProtocol;

public enum XPacketType
{
    Unknown,
    Handshake,
    String,
    Username,
    UserList,
    UserDisconnection,
    CardJson,
    CardsArray,
    GameStart,
    TurnRequest,
    TurnResponse,
}
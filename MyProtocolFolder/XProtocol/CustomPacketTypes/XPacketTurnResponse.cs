namespace XProtocol.CustomPacketTypes;

public class XPacketTurnResponse
{
    [XField(1)] public int Counter;
    [XField(2)] public string NextTurnUser;
}
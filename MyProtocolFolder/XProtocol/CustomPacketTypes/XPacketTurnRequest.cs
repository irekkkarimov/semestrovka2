namespace XProtocol.CustomPacketTypes;

public class XPacketTurnRequest
{
    [XField(1)] public List<int> PlayedCards;
}
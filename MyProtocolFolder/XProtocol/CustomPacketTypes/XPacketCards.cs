namespace XProtocol.CustomPacketTypes;

public class XPacketCards
{
    [XField(1)] public List<string[]> CurrentPlayerInHand;
    [XField(2)] public List<string[]> AllPlayersInPlay;
    [XField(3)] public int CountOfPlayers;
    [XField(4)] public string TurnUsername;
    [XField(5)] public bool IsDrawing;
    [XField(6)] public bool IsPlaying;
}
namespace XProtocol.CustomPacketTypes;

public class XPacketCards
{
    [XField(1)] public List<int> CurrentPlayerInHand;
    [XField(2)] public List<string[]> AllPlayersInPlay;
    [XField(3)] public int CountOfCardsToDraw;
    [XField(4)] public int CountOfCardsToPlay;
    [XField(5)] public string Goal;
    [XField(6)] public int CountOfPlayers;
    [XField(7)] public string TurnUsername;
    [XField(8)] public bool IsDrawing;
    [XField(9)] public bool IsPlaying;
    [XField(10)] public List<int> CardsAllowedToPlayInCurrentPlay;
}
using FluxxGame.Cards.Abstractions;

namespace FluxxGame.PlayerHandler;

public class Player
{
    public string Username { get; set; }
    public List<ICard> InHand { get; set; } = new();
    public List<ICard> CurrentMoveHand { get; set; } = new();
    public List<KeeperCard> Keepers { get; set; } = new();

    public static Player Create(string username)
    {
        return new Player
        {
            Username = username
        };
    }
}
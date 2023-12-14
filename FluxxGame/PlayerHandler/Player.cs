using FluxxGame.Cards.Abstractions;

namespace FluxxGame.PlayerHandler;

public class Player
{
    public string Username { get; set; }
    public List<ICard> InHand { get; set; }
    public List<KeeperCard> Keepers { get; set; }

    public static Player Create()
    {
        return new Player();
    }
}
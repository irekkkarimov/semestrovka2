using FluxxGame.Cards.Abstractions;

namespace Client.Cards;

public class KeeperCardGui
{
    public CardType Type { get; set; } = CardType.Keeper;

    public KeeperName Name { get; set; }

    public KeeperCardGui(KeeperName name)
    {
        Name = name;
    }
}
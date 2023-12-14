namespace FluxxGame.Cards.Abstractions;

public class KeeperCard : ICard
{
    public CardType Type { get; } = CardType.Keeper;

    public KeeperName Name { get; set; }

    public KeeperCard(KeeperName name)
    {
        Name = name;
    }
}

public enum KeeperName
{
    Chocolate,
    Cookies,
    Love
}
namespace XProtocol.Game.Cards.Abstractions;

public class ActionCard : ICard
{
    public CardType Type { get; } = CardType.Action;

    public ActionName Name { get; set; }

    public ActionCard(ActionName name)
    {
        Name = name;
    }
}

public enum ActionName
{
    Draw2AndUseThem,
    Draw3AndPlay2,
    LoseATurn
}
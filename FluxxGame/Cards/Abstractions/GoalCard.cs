namespace FluxxGame.Cards.Abstractions;

public class GoalCard : ICard
{
    public CardType Type { get; } = CardType.Goal;

    public GoalName Name { get; set; }

    public GoalCard(GoalName name)
    {
        Name = name;
    }
}

public enum GoalName
{
    _10CardsInHand,
    _5Keepers,
    ChocolateCookies
}
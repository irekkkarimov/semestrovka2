namespace XProtocol.Game.Cards.Abstractions;

public interface ICard
{
    public CardType Type { get; }
}

public enum CardType
{
    Action,
    Goal,
    Keeper,
    Rule
}
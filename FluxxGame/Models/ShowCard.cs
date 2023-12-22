using FluxxGame.Cards.Abstractions;

namespace FluxxGame.Models;

public class ShowCard
{
    public CardType Type { get; set; }
    public string Name { get; set; }

    public ShowCard() { }

    public ShowCard(ICard card, string name) : this(card.Type, name) { }

    public ShowCard(CardType type, string name)
    {
        Type = type;
        Name = name;
    }
}
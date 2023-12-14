using FluxxGame.Cards.Abstractions;

namespace Client.Models;

public class ShowCard
{
    public CardType Type { get; set; }
    public string Name { get; set; }
    public ICard Card { get; set; }

    public ShowCard() { }

    public ShowCard(ICard card, string name) : this(card.Type, name, card) { }

    public ShowCard(CardType type, string name, ICard card)
    {
        Type = type;
        Name = name;
        Card = card;
    }
}
namespace FluxxGame.Cards.Abstractions;

public class RuleCard : ICard
{
    public CardType Type { get; } = CardType.Rule;

    public RuleName Name { get; set; }

    public RuleCard(RuleName name)
    {
        Name = name;
    }
    
    public static int DetermineNumberOfCardsToDraw(RuleCard ruleCard)
    {
        return DetermineNumberOfCardsToDraw(ruleCard.Name);
    }
    
    public static int DetermineNumberOfCardsToDraw(RuleName ruleName)
    {
        return ruleName switch
        {
            RuleName.Draw1 => 1,
            RuleName.Draw2 => 2,
            RuleName.Draw3 => 3,
            _ => 1
        };
    }

    public static int DetermineNumberOfCardsToPlay(RuleCard ruleCard)
    {
        return DetermineNumberOfCardsToPlay(ruleCard.Name);
    }

    public static int DetermineNumberOfCardsToPlay(RuleName ruleName)
    {
        return ruleName switch
        {
            RuleName.Play1 => 1,
            RuleName.Play2 => 2,
            _ => 1
        };
    }
}

public enum RuleName
{
    Draw1,
    Draw2,
    Draw3,
    Play1,
    Play2
}
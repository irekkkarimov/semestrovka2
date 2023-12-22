using FluxxGame.Cards.Abstractions;

namespace FluxxGame.Handlers;

public static class Parser
{
    public static string[] ParseInHandCardToStringArray(ICard cardInput)
    {
        switch (cardInput.Type)
        {
            case CardType.Action:
            {
                var card = (ActionCard)cardInput;
                return new[]
                {
                    card.Type.ToString(),
                    card.Name.ToString()
                };
            }
            case CardType.Goal:
            {
                var card = (GoalCard)cardInput;
                return new[]
                {
                    card.Type.ToString(),
                    card.Name.ToString()
                };
            }
            case CardType.Keeper:
            {
                var card = (KeeperCard)cardInput;
                return new[]
                {
                    card.Type.ToString(),
                    card.Name.ToString()
                };
            }
            case CardType.Rule:
            {
                var card = (RuleCard)cardInput;
                return new[]
                {
                    card.Type.ToString(),
                    card.Name.ToString()
                };
            }
            default:
                return new[]
                {
                    "null",
                    "null"
                };
        }
    }

    public static string[] ParseKeeperInPlayCardToStringArray(int id, string username)
    {
        return new[]
        {
            username,
            id.ToString()
        };
    }

    public static ICard? ParseStringArrayWithoutUsernameToICard(string[] cardInput)
    {
        switch (cardInput[0])
        {
            case "Action":
            {
                var actionName = StringToCardNameMapper.MapActionName(cardInput[1]);
                return new ActionCard(actionName);
            }
            case "Goal":
            {
                var goalName = StringToCardNameMapper.MapGoalName(cardInput[1]);
                return new GoalCard(goalName);
            }
            case "Keeper":
            {
                var keeperName = StringToCardNameMapper.MapKeeperName(cardInput[1]);
                return new KeeperCard(keeperName);
            }
            case "Rule":
            {
                var ruleName = StringToCardNameMapper.MapRuleName(cardInput[1]);
                return new RuleCard(ruleName);
            }
            default:
                return null;
        }
    }
}
namespace FluxxGame.Cards.Abstractions;

public static class StringToCardNameMapper
{
    private static Dictionary<string, ActionName> _actionNames;
    private static Dictionary<string, GoalName> _goalNames;
    private static Dictionary<string, KeeperName> _keeperNames;
    private static Dictionary<string, RuleName> _ruleNames;

    static StringToCardNameMapper()
    {
        _actionNames = new Dictionary<string, ActionName>()
        {
            { ActionName.Draw3AndPlay2.ToString(), ActionName.Draw3AndPlay2 },
            { ActionName.Draw2AndUseThem.ToString(), ActionName.Draw2AndUseThem },
            { ActionName.LoseATurn.ToString(), ActionName.LoseATurn }
        };

        _goalNames = new Dictionary<string, GoalName>()
        {
            { GoalName._10CardsInHand.ToString(), GoalName._10CardsInHand },
            { GoalName.ChocolateCookies.ToString(), GoalName.ChocolateCookies },
            { GoalName._5Keepers.ToString(), GoalName._5Keepers }
        };

        _keeperNames = new Dictionary<string, KeeperName>()
        {
            { KeeperName.Chocolate.ToString(), KeeperName.Chocolate },
            { KeeperName.Cookies.ToString(), KeeperName.Cookies },
            { KeeperName.Love.ToString(), KeeperName.Love }
        };

        _ruleNames = new Dictionary<string, RuleName>()
        {
            { RuleName.Play1.ToString(), RuleName.Play1 },
            { RuleName.Play2.ToString(), RuleName.Play2 },
            { RuleName.Draw3.ToString(), RuleName.Draw3 },
            { RuleName.Draw2.ToString(), RuleName.Draw2 },
            { RuleName.Draw1.ToString(), RuleName.Draw1 }
        };
    }

    public static ActionName MapActionName(string actionName)
    {
        return _actionNames[actionName];
    }

    public static GoalName MapGoalName(string goalName)
    {
        return _goalNames[goalName];
    }

    public static KeeperName MapKeeperName(string keeperName)
    {
        return _keeperNames[keeperName];
    }

    public static RuleName MapRuleName(string ruleName)
    {
        return _ruleNames[ruleName];
    }
}
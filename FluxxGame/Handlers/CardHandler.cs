using FluxxGame.Cards.Abstractions;
using FluxxGame.Models;

namespace FluxxGame.Handlers;

public static class CardHandler
{
    public static Dictionary<int, ShowCard> ShowCardsDict { get; set; }
    public static Dictionary<string, int> ShowCardsDictReversed { get; private set; }
    public static List<ActionCard> ActionCards { get; private set; }
    public static List<GoalCard> GoalCards { get; private set; }
    public static List<KeeperCard> KeeperCards { get; private set; }
    public static List<RuleCard> RuleCards { get; private set; }
    public static List<ShowCard> ShowCards { get; private set; }

    static CardHandler()
    {
        ActionCards = new List<ActionCard>()
        {
            new ActionCard(ActionName.Draw3AndPlay2),
            new ActionCard(ActionName.LoseATurn),
            new ActionCard(ActionName.Draw2AndUseThem)
        };

        GoalCards = new List<GoalCard>()
        {
            new GoalCard(GoalName._5Keepers),
            new GoalCard(GoalName.ChocolateCookies),
            new GoalCard(GoalName._10CardsInHand)
        };

        KeeperCards = new List<KeeperCard>()
        {
            new KeeperCard(KeeperName.Chocolate),
            new KeeperCard(KeeperName.Cookies),
            new KeeperCard(KeeperName.Love)
        };

        RuleCards = new List<RuleCard>()
        {
            new RuleCard(RuleName.Draw1),
            new RuleCard(RuleName.Draw2),
            new RuleCard(RuleName.Draw3),
            new RuleCard(RuleName.Play1),
            new RuleCard(RuleName.Play2)
        };

        ShowCards = new List<ShowCard>();

        foreach (var newActionShowCard in ActionCards.Select(actionCard =>
                     new ShowCard(actionCard, actionCard.Name.ToString())))
        {
            ShowCards.Add(newActionShowCard);
        }

        foreach (var newGoalShowCard in GoalCards.Select(goalCard =>
                     new ShowCard(goalCard, goalCard.Name.ToString())))
        {
            ShowCards.Add(newGoalShowCard);
        }

        foreach (var newKeeperShowCard in KeeperCards.Select(keeperCard =>
                     new ShowCard(keeperCard, keeperCard.Name.ToString())))
        {
            ShowCards.Add(newKeeperShowCard);
        }

        foreach (var newRuleShowCard in RuleCards.Select(ruleCard =>
                     new ShowCard(ruleCard, ruleCard.Name.ToString())))
        {
            ShowCards.Add(newRuleShowCard);
        }


        ShowCardsDict = new Dictionary<int, ShowCard>();
        ShowCardsDictReversed = new Dictionary<string, int>();

        for (var i = 0; i < ShowCards.Count; i++)
        {
            ShowCardsDict[i + 1] = ShowCards[i];
            ShowCardsDictReversed[ShowCards[i].Name] = i + 1;
        }
    }

    public static ShowCard GetShowCardById(int id)
    {
        if (!ShowCardsDict.ContainsKey(id))
            throw new ArgumentException($"There is no show card for id {id}");

        return ShowCardsDict[id];
    }

    public static int GetIdByShowCard(ICard cardInput)
    {
        switch (cardInput.Type)
        {
            case CardType.Action:
            {
                var card = (ActionCard)cardInput;
                return GetIdByCardName(card.Name.ToString());
            }
            case CardType.Goal:
            {
                var card = (GoalCard)cardInput;
                return GetIdByCardName(card.Name.ToString());
            }
            case CardType.Keeper:
            {
                var card = (KeeperCard)cardInput;
                return GetIdByCardName(card.Name.ToString());
            }
            case CardType.Rule:
            {
                var card = (RuleCard)cardInput;
                return GetIdByCardName(card.Name.ToString());
            }
        }

        return 0;
    }

    public static int GetIdByCardName(string cardName)
    {
        if (!ShowCardsDictReversed.ContainsKey(cardName))
            throw new ArgumentException($"There is no show card with name {cardName}");

        return ShowCardsDictReversed[cardName];
    }

    public static ICard GetICardById(int id)
    {
        if (!ShowCardsDict.ContainsKey(id))
            throw new AggregateException();

        var showCard = GetShowCardById(id);

        return showCard.Type switch
        {
            CardType.Action => new ActionCard(StringToCardNameMapper.MapActionName(showCard.Name)),
            CardType.Goal => new GoalCard(StringToCardNameMapper.MapGoalName(showCard.Name)),
            CardType.Keeper => new KeeperCard(StringToCardNameMapper.MapKeeperName(showCard.Name)),
            CardType.Rule => new RuleCard(StringToCardNameMapper.MapRuleName(showCard.Name)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
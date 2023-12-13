using XProtocol.Game.Cards.Abstractions;
using XProtocol.Game.PlayerHandler;

namespace XProtocol.Game;

public class GameLogic
{
    private Stack<ICard> Deck { get; set; }
    private List<Player> Players { get; set; }
    private Player Turn { get; set; }
    private List<ICard> CurrentTurnCards { get; set; }
    private int NumberOfCardsMustBePlayed { get; set; }
    public bool CurrentTurnDrawn { get; set; }
    private List<GoalCard> CurrentGoalCards { get; set; }
    private List<RuleCard> CurrentRules { get; set; }

    private GameLogic()
    {
        Deck = Shuffle();
    }

    public GameLogic Start()
    {
        return new GameLogic();
    }

    public void ProcessTurn()
    {
        if (CurrentTurnDrawn)
        {
            return;
        }
        
        Draw();
    }

    public void ProcessTurn(List<ICard> cardsPlayed)
    {
        var playerCards = Turn.InHand;
        var countOfCardsInHand = playerCards.Count;

        if (!CurrentTurnDrawn)
        {
            ProcessTurn();
            CurrentTurnDrawn = false;
        }
        

        if (!cardsPlayed.Any())
            throw new ArgumentException();

        if (cardsPlayed.Count == 1)
        {
            var card = cardsPlayed[0];
            PlayCard(card);
        }
        else
        {
            foreach (var card in cardsPlayed)
                PlayCard(card);
        }
        
    }

    private void Draw()
    {
        var countOfCardsToDraw = 1;
        var cards = new List<ICard>();
        
        // Creating list with all rule names that affect card draw
        var allDrawRuleNames = new List<RuleName>()
        {
            RuleName.Draw2,
            RuleName.Draw3
        };

        // Checking if current rule cards contain draw rule cards
        foreach (var rule in allDrawRuleNames)
        {
            // If yes, then setting countOfCardsToDraw to correspondent number
            if (CurrentRules.Select(i => i.Name).Contains(rule))
                countOfCardsToDraw = RuleCard.DetermineNumberOfCardsToDraw(rule);
            break;
        }

        // Drawing needed number of cards from the deck
        for (var i = 0; i < countOfCardsToDraw; i++)
            cards.Add(Deck.Pop());
    }

    private void PlayCard(ICard card)
    {
        switch (card.Type)
        {
            case CardType.Action:
                PerformAction((ActionCard)card);
                break;
            case CardType.Goal:
                UpdateGoal((GoalCard)card);
                break;
            case CardType.Keeper:
                UseKeeper((KeeperCard)card);
                break;
            case CardType.Rule:
                UpdateRules((RuleCard)card);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void PerformAction(ActionCard card)
    {
        
    }

    private void UpdateRules(RuleCard card)
    {
        var rule = card.Name;
        var ruleNamesAffectingDraw = new List<RuleName>
        {
            RuleName.Draw2,
            RuleName.Draw3
        };
        var ruleNamesAffectingPlay = new List<RuleName>
        {
            RuleName.Play2
        };

        if (ruleNamesAffectingDraw.Contains(rule))
        {
            foreach (var currentRule in CurrentRules)
                if (ruleNamesAffectingDraw.Contains(currentRule.Name))
                {
                    CurrentRules.Remove(currentRule);
                    CurrentRules.Add(card);
                    break;
                }
        }
        else if (ruleNamesAffectingPlay.Contains(rule))
        {
            foreach (var currentRule in CurrentRules)
            {
                if (ruleNamesAffectingPlay.Contains(currentRule.Name))
                {
                    CurrentRules.Remove(currentRule);
                    CurrentRules.Add(card);
                    break;
                }
            }
        }
    }

    private void UpdateGoal(GoalCard card)
    {
        var goal = card.Name;

        if (CurrentRules.Any())
        {
            CurrentGoalCards.Clear();
            CurrentGoalCards.Add(card);
        }
    }

    private void UseKeeper(KeeperCard card)
    {
        var keeper = card.Name;
        var playerKeepers = Turn.Keepers;
        
        if (!playerKeepers.Select(i => i.Name).Contains(keeper))
            playerKeepers.Add(card);
    }

    private static Stack<ICard> Shuffle()
    {
        var cards = new Stack<ICard>();
        
        cards.Push(new ActionCard(ActionName.Draw3AndPlay2));
        cards.Push(new ActionCard(ActionName.Draw2AndUseThem));
        cards.Push(new ActionCard(ActionName.LoseATurn));
        cards.Push(new GoalCard(GoalName._5Keepers));
        cards.Push(new GoalCard(GoalName._10CardsInHand));
        cards.Push(new GoalCard(GoalName.ChocolateCookies));
        cards.Push(new KeeperCard(KeeperName.Chocolate));
        cards.Push(new KeeperCard(KeeperName.Cookies));
        cards.Push(new KeeperCard(KeeperName.Love));
        cards.Push(new RuleCard(RuleName.Draw2));
        cards.Push(new RuleCard(RuleName.Draw3));
        cards.Push(new RuleCard(RuleName.Play2));
        
        return cards;
    }
}
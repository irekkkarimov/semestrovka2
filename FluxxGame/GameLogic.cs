using System.Collections;
using System.Runtime.InteropServices.JavaScript;
using FluxxGame.Cards.Abstractions;
using FluxxGame.PlayerHandler;

namespace FluxxGame;

public class GameLogic
{
    private Stack<ICard> Deck { get; set; }
    public List<Player> Players { get; set; } = new();
    public Player Turn { get; set; }
    private bool _skipTurnSwitch = false;
    public List<ICard> CurrentTurnCards { get; private set; } = new();
    public int CountOfCardsMustBePlayed { get; private set; } = 1;
    public int CountOfCardsMustBePlayedReserve { get; private set; } = 0;
    public int CountOfCardsMustBeDrawn { get; private set; } = 1;
    public int CountOfCardsMustBeDrawnReserve { get; private set; } = 0;
    public string Goal { get; private set; }
    public bool CurrentTurnDrawn { get; set; } = false;
    private List<GoalCard> CurrentGoalCards { get; set; } = new();
    private List<RuleCard> CurrentRules { get; set; } = new();
    public Player? SkippingTurn { get; private set; }

    private GameLogic()
    {
        Deck = Shuffle();
    }

    public static GameLogic Start()
    {
        return new GameLogic();
    }

    public void AddPlayer(Player player)
    {
        if (Turn is null)
        {
            Turn = player;
        }

        Players.Add(player);
    }

    public void ProcessTurn(List<ICard> cards = null)
    {
        if (CurrentTurnDrawn)
        {
            ProcessTurnOnlyPlay(cards);
        }
        else
        {
            Draw();
        }
    }

    private void ProcessTurnOnlyPlay(List<ICard> cardsPlayed)
    {
        if (CountOfCardsMustBePlayedReserve > 0)
        {
            CountOfCardsMustBePlayed = CountOfCardsMustBePlayedReserve;
            CountOfCardsMustBePlayedReserve = 0;
        }
        
        var playerCards = Turn.InHand;
        var countOfCardsInHand = playerCards.Count;

        Console.WriteLine(CurrentTurnDrawn);
        if (!CurrentTurnDrawn)
        {
            ProcessTurn();
            CurrentTurnDrawn = false;
        }


        if (!cardsPlayed.Any())
            throw new ArgumentException();

        foreach (var card in cardsPlayed)
            PlayCard(card);

        SwitchTurn();
        CurrentTurnDrawn = false;
    }

    public void Draw()
    {
        CurrentTurnCards.Clear();
        if (Deck.Count < CountOfCardsMustBeDrawn)
            Deck = Shuffle();

        var cards = new List<ICard>();

        // Creating list with all rule names that affect card draw
        var allDrawRuleNames = new List<RuleName>
        {
            RuleName.Draw2,
            RuleName.Draw3
        };

        // Checking if current rule cards contain draw rule cards
        // foreach (var rule in allDrawRuleNames)
        // {
        //     // If yes, then setting countOfCardsToDraw to correspondent number
        //     if (CurrentRules.Select(i => i.Name).Contains(rule))
        //         CountOfCardsMustBeDrawn = RuleCard.DetermineNumberOfCardsToDraw(rule);
        //     break;
        // }

        // Drawing needed number of cards from the deck
        for (var i = 0; i < CountOfCardsMustBeDrawn; i++)
            if (Turn.InHand.Count + cards.Count < 10)
                cards.Add(Deck.Pop());

        if (CountOfCardsMustBeDrawnReserve > 0)
        {
            CurrentTurnCards.AddRange(cards);
            CountOfCardsMustBeDrawn = CountOfCardsMustBeDrawnReserve;
            CountOfCardsMustBeDrawnReserve = 0;
        }
        
        // Turn.CurrentMoveHand.AddRange(cards);
        Turn.InHand.AddRange(cards);
        CurrentTurnDrawn = true;
    }

    public void DrawGameStart(Player player)
    {
        if (!Players.Contains(player))
            return;

        var cards = new List<ICard>();

        // Drawing needed number of cards from the deck
        for (var i = 0; i < 3; i++)
            cards.Add(Deck.Pop());

        player.InHand.AddRange(cards);
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
        var cardToRemove = Turn.InHand.Where(i => i.Type == CardType.Action)
            .Select(i => (ActionCard)i)
            .FirstOrDefault(i => i.Name.Equals(card.Name));

        Turn.InHand.Remove(cardToRemove);

        switch (card.Name)
        {
            case ActionName.Draw3AndPlay2:
            {
                CurrentTurnDrawn = false;
                CountOfCardsMustBeDrawnReserve = CountOfCardsMustBeDrawn;
                CountOfCardsMustBeDrawn = 3;
                CountOfCardsMustBePlayedReserve = CountOfCardsMustBePlayed;
                CountOfCardsMustBePlayed = 2;
                _skipTurnSwitch = true;
                break;
            }
            case ActionName.Draw2AndUseThem:
            {
                CurrentTurnDrawn = false;
                CountOfCardsMustBeDrawnReserve = CountOfCardsMustBeDrawn;
                CountOfCardsMustBeDrawn = 2;
                CountOfCardsMustBePlayedReserve = CountOfCardsMustBePlayed;
                CountOfCardsMustBePlayed = 2;
                _skipTurnSwitch = true;
                break;
            }
            case ActionName.LoseATurn:
            {
                SkippingTurn = Turn;
                break;
            }
        }
    }

    private void UpdateRules(RuleCard card)
    {
        var cardToRemove = Turn.InHand.Where(i => i.Type == CardType.Rule)
            .Select(i => (RuleCard)i)
            .FirstOrDefault(i => i.Name.Equals(card.Name));

        Turn.InHand.Remove(cardToRemove);

        var rule = card.Name;
        var ruleNamesAffectingDraw = new List<RuleName>
        {
            RuleName.Draw1,
            RuleName.Draw2,
            RuleName.Draw3
        };
        var ruleNamesAffectingPlay = new List<RuleName>
        {
            RuleName.Play1,
            RuleName.Play2
        };

        if (ruleNamesAffectingDraw.Contains(rule))
        {
            foreach (var currentRule in CurrentRules)
                if (ruleNamesAffectingDraw.Contains(currentRule.Name))
                {
                    CurrentRules.Remove(currentRule);
                    break;
                }

            CurrentRules.Add(card);
        }
        else if (ruleNamesAffectingPlay.Contains(rule))
        {
            foreach (var currentRule in CurrentRules)
                if (ruleNamesAffectingPlay.Contains(currentRule.Name))
                {
                    CurrentRules.Remove(currentRule);
                    break;
                }

            CurrentRules.Add(card);
        }

        foreach (var currentRule in CurrentRules)
        {
            switch (currentRule.Name)
            {
                case RuleName.Draw1:
                case RuleName.Draw2:
                case RuleName.Draw3:
                    CountOfCardsMustBeDrawn = RuleCard.DetermineNumberOfCardsToDraw(currentRule);
                    break;
                case RuleName.Play1:    
                case RuleName.Play2:
                    CountOfCardsMustBePlayed = RuleCard.DetermineNumberOfCardsToPlay(currentRule);
                    break;
            }
        }
    }

    private void UpdateGoal(GoalCard card)
    {
        var cardToRemove = Turn.InHand.Where(i => i.Type == CardType.Goal)
            .Select(i => (GoalCard)i)
            .FirstOrDefault(i => i.Name.Equals(card.Name));

        Turn.InHand.Remove(cardToRemove);

        var goal = card.Name;

        if (CurrentGoalCards.Any())
        {
            CurrentGoalCards.Clear();
        }
        
        CurrentGoalCards.Add(card);
        
        Goal = goal.ToString();
    }

    private void UseKeeper(KeeperCard card)
    {
        var cardToRemove = Turn.InHand.Where(i => i.Type == CardType.Keeper)
            .Select(i => (KeeperCard)i)
            .FirstOrDefault(i => i.Name.Equals(card.Name));

        Turn.InHand.Remove(cardToRemove);

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
        cards.Push(new RuleCard(RuleName.Draw1));
        cards.Push(new RuleCard(RuleName.Draw2));
        cards.Push(new RuleCard(RuleName.Draw3));
        cards.Push(new RuleCard(RuleName.Play2));
        cards.Push(new RuleCard(RuleName.Play1));

        var random = new Random();

        var cardsShuffled = cards.OrderBy(i => random.Next()).ToList();

        var shuffledStack = new Stack<ICard>(cardsShuffled);
        
        return shuffledStack;
    }

    private void SwitchTurn()
    {
        if (_skipTurnSwitch)
        {
            _skipTurnSwitch = false;
            return;
        }

        while (true)
        {
            var index = Players.IndexOf(Turn);
            if (index == Players.Count - 1)
                Turn = Players[0];
            else
                Turn = Players[index + 1];

            if (SkippingTurn is not null)
            {
                if (Turn == SkippingTurn)
                {
                    SkippingTurn = null;
                    continue;
                }
            }

            break;
        }
    }

    public (string, string, string) CheckWinner()
    {
        var goalCardNamesForKeepers = new List<GoalName>
        {
            GoalName._5Keepers,
            GoalName.ChocolateCookies
        };

        var otherGoalCardNames = new List<GoalName>
        {
            GoalName._10CardsInHand
        };

        while (true)
        {
            foreach (var player in Players)
            {
                foreach (var goalCard in CurrentGoalCards)
                {
                    if (goalCardNamesForKeepers.Contains(goalCard.Name))
                    {
                        var playerKeepersNames = player.Keepers.Select(i => i.Name);

                        switch (goalCard.Name)
                        {
                            case GoalName._5Keepers:
                            {
                                if (player.Keepers.Count >= 5)
                                    return (player.Username, goalCard.Name.ToString(),
                                        $"Player {player.Username} had {player.Keepers.Count} Keeper cards and won");
                                break;
                            }
                            case GoalName.ChocolateCookies:
                            {
                                if (playerKeepersNames.Contains(KeeperName.Chocolate) &&
                                    playerKeepersNames.Contains(KeeperName.Cookies))
                                    return (player.Username, goalCard.Name.ToString(),
                                        $"Player {player.Username} had Cookies and Chocolate cards and won");
                                break;
                            }
                        }

                        // Current game version is only for a single goal card!!
                        break;
                    }

                    if (otherGoalCardNames.Contains(goalCard.Name))
                    {
                        switch (goalCard.Name)
                        {
                            case GoalName._10CardsInHand:
                            {
                                if (player.InHand.Count >= 10)
                                    return (player.Username, goalCard.Name.ToString(),
                                        $"Player {player.Username} had {player.InHand.Count} cards in Hand and won");
                                break;
                            }
                        }

                        // Current game version is only for a single goal card!!
                        break;
                    }
                }
            }
            
            Thread.Sleep(2000);
        }
    }
}
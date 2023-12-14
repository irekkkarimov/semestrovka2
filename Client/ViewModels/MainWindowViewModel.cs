using System.Collections.ObjectModel;
using Client.Cards;
using Client.Models;
using FluxxGame.Cards.Abstractions;

namespace Client.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<ShowCard> Cards { get; set; }

    public ObservableCollection<ShowCard> CardsInPlay { get; set; } = new();

    public MainWindowViewModel()
    {
        var card1 = new ActionCard(ActionName.Draw3AndPlay2);
        var card2 = new GoalCard(GoalName.ChocolateCookies);
        var card3 = new KeeperCard(KeeperName.Chocolate);
        var card4 = new RuleCard(RuleName.Draw2);

        Cards = new ObservableCollection<ShowCard>
        {
            new(card1, card1.Name.ToString()),
            new(card2, card2.Name.ToString()),
            new(card3, card3.Name.ToString()),
            new(card4, card4.Name.ToString())
        };
    }
}
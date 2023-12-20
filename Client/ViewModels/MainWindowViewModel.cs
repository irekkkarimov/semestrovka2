using System.Collections.ObjectModel;
using System.Linq;
using Client.Cards;
using Client.Models;
using FluxxGame.Cards.Abstractions;

namespace Client.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<ShowCard> Cards { get; set; }
    public ObservableCollection<Player> Players { get; set; }

    public ObservableCollection<ShowCard> CardsInPlay { get; set; } = new();

    public MainWindowViewModel()
    {
        Players = new ObservableCollection<Player>();

        for (var i = 0; i < 3; i++)
        {
            Players.Add(new Player());
        }

        Cards = new ObservableCollection<ShowCard>
        {
            // new(card1, card1.Name.ToString()),
            // new(card2, card2.Name.ToString()),
            // new(card3, card3.Name.ToString()),
            // new(card4, card4.Name.ToString())
        };
    }
}
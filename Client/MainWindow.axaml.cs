using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Client.Cards;
using Client.Models;
using Client.ViewModels;
using FluxxGame.Cards.Abstractions;
using XProtocol;
using XProtocol.CustomPacketTypes;

namespace Client;

public partial class MainWindow : Window
{
    public static int HandshakeMagic { get; private set; }
    private bool _isGameStarted = false;
    private XClient _client = null!;
    private string _username;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SendUserName(object? sender, RoutedEventArgs e)
    {
        if (_client == null!)
        {
            _client = new XClient();
            Task.Run(Start);
            Thread.Sleep(2000);
        }

        SendUserName(new XPacketUsername
        {
            Name = EnterName.Text
        });
    }


    private Task Start()
    {
        _client = new XClient();
        _client.OnPacketReceive += OnPacketReceive;
        _client.Connect("127.0.0.1", 4910);

        var rand = new Random();
        HandshakeMagic = rand.Next();

        Thread.Sleep(1000);

        Console.WriteLine("Sending handshake packet..");

        _client.QueuePacketSend(
            XPacketConverter.Serialize(
                    XPacketType.Handshake,
                    new XPacketHandshake
                    {
                        MagicHandshakeNumber = HandshakeMagic
                    })
                .ToPacket());
        while (true)
        {
        }
    }

    private void SendStringPacket(XPacketString packet)
    {
        _client.QueuePacketSend(
            XPacketConverter.Serialize(
                    XPacketType.String,
                    packet)
                .ToPacket());
    }

    private void SendUserName(XPacketUsername packet)
    {
        _client.QueuePacketSend(
            XPacketConverter.Serialize(
                    XPacketType.Username,
                    packet)
                .ToPacket());
    }
    
    private void StartGame(object? sender, RoutedEventArgs e)
    {
        _client.QueuePacketSend(XPacketConverter
            .Serialize(
                XPacketType.GameStart,
                new XPacketGameStart()
                {
                    UsernameRequested = _username
                })
            .ToPacket());
    }
    
    private void DisconnectButton(object? sender, RoutedEventArgs e)
    {
        Disconnect();
        Close();
    }

    private void Disconnect()
    {
        _client.QueuePacketSend(
            XPacketConverter.Serialize(
                    XPacketType.UserDisconnection,
                    new XPacketUserDisconnection())
                .ToPacket());
    }

    private void OnPacketReceive(byte[] packet)
    {
        var parsed = XPacket.Parse(packet);
        if (parsed != null)
            ProcessIncomingPacket(parsed);
    }

    private void ProcessIncomingPacket(XPacket packet)
    {
        var type = XPacketTypeManager.GetTypeFromPacket(packet);

        switch (type)
        {
            case XPacketType.Handshake:
                ProcessHandshake(packet);
                break;
            case XPacketType.String:
                break;
            case XPacketType.Username:
                ProcessUsernameAnswer(packet);
                break;
            case XPacketType.UserList:
                ProcessUserList(packet);
                break;
            case XPacketType.TurnResponse:
                ProcessTurnResponse(packet);
                break;
            case XPacketType.CardsArray:
                ProcessCardsReceive(packet);
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ProcessHandshake(XPacket packet)
    {
        var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

        if (HandshakeMagic - handshake.MagicHandshakeNumber == 15)
        {
            Console.WriteLine("Handshake successful!");
        }
    }

    private void ProcessUsernameAnswer(XPacket packet)
    {
        var userNamePacket = XPacketConverter.Deserialize<XPacketUsername>(packet);
        if (!userNamePacket.IsProcessed)
            return;

        _username = userNamePacket.Name;
        Dispatcher.UIThread.Invoke(() => UserEnterContainer.IsVisible = false);
        Dispatcher.UIThread.Invoke(() => GameContainer.IsVisible = true);
        Dispatcher.UIThread.Invoke(() => UserNameContainer.IsVisible = true);
        Dispatcher.UIThread.Invoke(() => NameTextBlock.Text = userNamePacket.Name);
    }

    private void ProcessUserList(XPacket packet)
    {
        var userListPacket = XPacketConverter.Deserialize<XPacketUserList>(packet);
        if (!userListPacket.UserList.Any())
            return;

        Dispatcher.UIThread.Invoke(() =>
        {
            UserList.Items.Clear();

            foreach (var username in userListPacket.UserList)
            {
                var listboxItem = new ListBoxItem
                {
                    Content = username,
                    IsHitTestVisible = false
                };

                if (userListPacket.TurnUser.Equals(username))
                    listboxItem.Background = Brushes.SeaGreen;

                UserList.Items.Add(listboxItem);
            }
        });
        Dispatcher.UIThread.Invoke(() => UserListGrid.IsVisible = true);
        Dispatcher.UIThread.Invoke(() => UserList.IsVisible = true);
    }

    private void ProcessTurnResponse(XPacket packet)
    {
        var turnResponse = XPacketConverter.Deserialize<XPacketTurnResponse>(packet);
        // Dispatcher.UIThread.Invoke(() => Counter.Text = turnResponse.Counter.ToString());

        foreach (ListBoxItem? user in UserList.Items)
        {
            if (user == null)
                continue;

            object? usernameObject = null;
            Dispatcher.UIThread.Invoke(() => usernameObject = user.Content);

            if (usernameObject != null && turnResponse.NextTurnUser.Equals((string)usernameObject))
                Dispatcher.UIThread.Invoke(() => user.Background = Brushes.SeaGreen);
            else
                Dispatcher.UIThread.Invoke(() => user.Background = Brushes.Transparent);
        }
    }

    private void ProcessCardReceive(XPacket packet)
    {
        var cardPacket = XPacketConverter.Deserialize<XPacketCard>(packet);
        var card = JsonSerializer.Deserialize<ICard>(cardPacket.CardJson);

        var cards = new ObservableCollection<KeeperCardGui>()
        {
            new KeeperCardGui(KeeperName.Chocolate)
        };
    }

    private void ProcessCardsReceive(XPacket packet)
    {
        var dataContext = Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);
        var cards = XPacketConverter.Deserialize<XPacketCards>(packet);

        
        var cardsCurrentPlayerInHand = cards.CurrentPlayerInHand;
        var cardsInPlay = cards.AllPlayersInPlay;

        Console.WriteLine(cardsCurrentPlayerInHand.Count);
        
        var cardsInHandKeeperActionCards = cardsCurrentPlayerInHand
            .Where(i => i[0].Equals(CardType.Action.ToString()))
            .Select(i => new ActionCard(StringToCardNameMapper.MapActionName(i[1])))
            .Select(i => new ShowCard(i, i.Name.ToString()))
            .ToArray();

        var cardsInHandKeeperGoalCards = cardsCurrentPlayerInHand
            .Where(i => i[0].Equals(CardType.Goal.ToString()))
            .Select(i => new GoalCard(StringToCardNameMapper.MapGoalName(i[1])))
            .Select(i => new ShowCard(i, i.Name.ToString()))
            .ToArray();

        var cardsInHandKeeperKeeperCards = cardsCurrentPlayerInHand
            .Where(i => i[0].Equals(CardType.Keeper.ToString()))
            .Select(i => new KeeperCard(StringToCardNameMapper.MapKeeperName(i[1])))
            .Select(i => new ShowCard(i, i.Name.ToString()))
            .ToArray();

        var cardsInHandKeeperRuleCards = cardsCurrentPlayerInHand
            .Where(i => i[0].Equals(CardType.Rule.ToString()))
            .Select(i => new RuleCard(StringToCardNameMapper.MapRuleName(i[1])))
            .Select(i => new ShowCard(i, i.Name.ToString()))
            .ToArray();

        var allCardsInHand = new List<ShowCard>();

        allCardsInHand.AddRange(cardsInHandKeeperActionCards);
        allCardsInHand.AddRange(cardsInHandKeeperGoalCards);
        allCardsInHand.AddRange(cardsInHandKeeperKeeperCards);
        allCardsInHand.AddRange(cardsInHandKeeperRuleCards);

        var allPlayersGrouped = cardsInPlay.GroupBy(i => i[0]).ToArray();
        var players = new List<Player>();

        foreach (var group in allPlayersGrouped)
        {
            var playerName = group.Key;
            var playerCardsRaw = group.Select(i => i).ToList();

            var keeperCards = playerCardsRaw
                .Select(i =>
                {
                    var keeperName = StringToCardNameMapper.MapKeeperName(i[2]);
                    var keeperCard = new KeeperCard(keeperName);
                    var showCard = new ShowCard(keeperCard, i[2]);
                    return showCard;
                })
                .ToList();

            var player = new Player
            {
                Username = playerName,
                CardsInPlay = keeperCards
            };
            
            players.Add(player);
        }

        var currentPlayer = players.Find(i => i.Username.Equals(_username));
        players.Remove(currentPlayer);
        
        Dispatcher.UIThread.Invoke(() =>
        {
            foreach (var showCard in allCardsInHand)
            {
                dataContext.Cards.Add(showCard);
            }

            for (var i = 0; i < players.Count; i++)
            {
                dataContext.Players[i] = players[i];
            }

            switch (players.Count)
            {
                case 2:
                    Slot1.IsVisible = true;
                    break;
                case 3:
                    Slot1.IsVisible = true;
                    Slot2.IsVisible = true;
                    break;
                case 4:
                    Slot1.IsVisible = true;
                    Slot2.IsVisible = true;
                    Slot3.IsVisible = true;
                    break;
            }

            Rules.IsVisible = true;
            
            if (currentPlayer is not null)
                foreach (var card in currentPlayer.CardsInPlay)
                {
                    dataContext.CardsInPlay.Add(card);
                }
                
        });
    }

    private void AddCardToHand(object? sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as MainWindowViewModel;
        var newCard = new KeeperCard(KeeperName.Chocolate);

        if (dataContext.Cards.Count < 8)
            dataContext.Cards.Add(new ShowCard(newCard, newCard.Name.ToString()));
    }

    private void MoveFromHandToPlay(object? sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as MainWindowViewModel;

        if (!dataContext.Cards.Any())
            return;

        var card = dataContext.Cards.Last();
        dataContext.Cards.Remove(card);
        dataContext.CardsInPlay.Add(card);
    }
    
    private void CounterIncrementButton(object? sender, RoutedEventArgs e)
    {
        var turnRequest = new XPacketTurnRequest();

        _client.QueuePacketSend(
            XPacketConverter.Serialize(
                    XPacketType.TurnRequest,
                    turnRequest)
                .ToPacket());
    }

    private void MakeMove(object? sender, PointerPressedEventArgs e)
    {
        if (sender is StackPanel clickedItem)
        {
            Console.WriteLine("stack panel");
            var dataContext = clickedItem.DataContext;

            if (dataContext is ShowCard card)
                Console.WriteLine(card.Name);
        }
    }
}
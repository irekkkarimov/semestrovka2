using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Client.Cards;
using Client.Models;
using Client.ViewModels;
using FluxxGame.Cards.Abstractions;
using FluxxGame.Handlers;
using FluxxGame.Models;
using XProtocol;
using XProtocol.CustomPacketTypes;

namespace Client;

public partial class MainWindow : Window
{
    public static int HandshakeMagic { get; private set; }
    private bool _isGameStarted = false;
    private bool _isDrawn = false;
    private List<(int, string)> _chosenCards = new();
    private List<string> _currentMoveAllowedCards = new();
    private string _turnUsername;
    private int _numberOfCardsToPlay = 1;
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
            case XPacketType.Win:
                ProcessWinReceive(packet);
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
        {
            Console.WriteLine("Server didn't accept you");
            return;
        }

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

        _isGameStarted = true;

        var allCardsInHand = cardsCurrentPlayerInHand
            .Select(CardHandler.GetShowCardById)
            .ToList();

        if (cards.CardsAllowedToPlayInCurrentPlay.Any())
        {
            _currentMoveAllowedCards = cards.CardsAllowedToPlayInCurrentPlay
                .Select(i => CardHandler.GetShowCardById(i).Name)
                .ToList();

            foreach (var i in _currentMoveAllowedCards)
            {
                Console.WriteLine(i);
            }
        }
        else
        {
            _currentMoveAllowedCards.Clear();
        }
        
        var allPlayersGrouped = cardsInPlay.GroupBy(i => i[0]).ToArray();
        var players = new List<Player>();

        _turnUsername = cards.TurnUsername;

        foreach (var group in allPlayersGrouped)
        {
            var playerName = group.Key;
            var playerCardsRaw = group.Select(i => i).ToList();

            var keeperCards = playerCardsRaw
                .Select(i =>
                    CardHandler.GetShowCardById(int.Parse(i[1])))
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
            Rule1.Text = cards.CountOfCardsToDraw.ToString();
            Rule2.Text = cards.CountOfCardsToPlay.ToString();

            if (!string.IsNullOrEmpty(cards.Goal))
                Goal1.Text = cards.Goal;

            if (_isGameStarted)
                GameStartButton.IsVisible = false;

            if (_turnUsername.Equals(_username))
            {
                TurnStackPanel.IsVisible = true;
                _numberOfCardsToPlay = cards.CountOfCardsToPlay;
                NumberOfCardsNeededToPlay.Text = _numberOfCardsToPlay.ToString();
                NumberOfCardsChosenToPlay.Text = "0";

                var isDrawing = cards.IsDrawing;
                _isDrawn = !cards.IsDrawing;

                if (isDrawing)
                {
                    DrawButton.IsVisible = true;
                    PlayButton.IsVisible = false;
                }
                else
                {
                    DrawButton.IsVisible = false;
                    PlayButton.IsVisible = true;
                }
            }
            else
            {
                TurnStackPanel.IsVisible = false;
                DrawButton.IsVisible = false;
                PlayButton.IsVisible = false;
            }

            foreach (var user in UserList.Items)
            {
                if (user is ListBoxItem userListBoxItem)
                {
                    if (userListBoxItem.Content is string castedUserName)
                    {
                        if (castedUserName.Equals(_turnUsername))
                        {
                            userListBoxItem.Background = Brushes.SeaGreen;
                        }
                        else
                        {
                            userListBoxItem.Background = Brushes.Transparent;
                        }
                    }
                }
            }

            dataContext.Cards.Clear();
            dataContext.CardsInPlay.Clear();

            foreach (var showCard in allCardsInHand)
            {
                dataContext.Cards.Add(showCard);

                // foreach (ListBoxItem cardInHand in CardsInHand.Items)
                // {
                //     cardInHand.Background = Brushes.Transparent;
                // }
            }

            for (var i = 0; i < players.Count; i++)
            {
                dataContext.Players[i] = players[i];
            }

            switch (cards.CountOfPlayers)
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
            Goals.IsVisible = true;
            MessageBox.IsVisible = true;

            if (currentPlayer is not null)
                foreach (var card in currentPlayer.CardsInPlay)
                {
                    dataContext.CardsInPlay.Add(card);
                }
        });
    }

    private void ProcessWinReceive(XPacket packet)
    {
        var winPacket = XPacketConverter.Deserialize<XPacketWin>(packet);

        Dispatcher.UIThread.Invoke(() =>
        {
            GameContainer.IsVisible = false;
            WinGrid.IsVisible = true;
            WinnerUsername.Text = winPacket.WinnerUsername;

            if (winPacket.WinnerUsername.Equals(_username))
            {
                WinMessage.Text = "You won!!!";
            }
            else
            {
                WinMessage.Text = winPacket.Message;
            }
        });
    }

    private void MakeMove(object? sender, RoutedEventArgs e)
    {
        if (!_isDrawn)
        {
            var drawRequest = new XPacketDrawRequest();

            _client.QueuePacketSend(
                XPacketConverter.Serialize(
                        XPacketType.DrawRequest,
                        drawRequest)
                    .ToPacket());

            return;
        }

        if (!_chosenCards.Any())
        {
            Console.WriteLine("Choose cards to play!");
            PrintToMessageBox("Choose cards to play!");
            return;
        }

        var countOfCardsInHand =
            Dispatcher.UIThread.Invoke(() => (DataContext as MainWindowViewModel).Cards);

        if ((countOfCardsInHand.Count - _chosenCards.Count > 0) && _chosenCards.Count < _numberOfCardsToPlay)
        {
            var leftToChoose = _numberOfCardsToPlay - _chosenCards.Count;
            Console.WriteLine($"Choose {leftToChoose} more cards to play");
            PrintToMessageBox($"Choose {leftToChoose} more cards to play");
            return;
        }

        var chosenCardsInStringArrayList = _chosenCards
            .Select(i => i.Item1)
            .ToList();

        var turnRequest = new XPacketTurnRequest
        {
            PlayedCards = chosenCardsInStringArrayList
        };

        _client.QueuePacketSend(
            XPacketConverter.Serialize(
                    XPacketType.TurnRequest,
                    turnRequest)
                .ToPacket());

        _chosenCards.Clear();
    }

    private void ChooseCard(object? sender, PointerPressedEventArgs e)
    {
        if (_username != _turnUsername
            || !_isDrawn
            || _chosenCards.Count >= _numberOfCardsToPlay)
            return;

        if (sender is StackPanel clickedItem)
        {
            var dataContext = clickedItem.DataContext;

            if (dataContext is ShowCard card)
            {
                if (_currentMoveAllowedCards.Any())
                {
                    if (!_currentMoveAllowedCards.Contains(card.Name))
                    {
                        Console.WriteLine("This card is not allowed");
                        PrintToMessageBox("This card is not allowed");
                        return;
                    }
                }
                
                var mainWindowViewModel = DataContext as MainWindowViewModel;

                var chosenCardsNames = _chosenCards
                    .Select(i => i.Item2)
                    .ToList();

                if (chosenCardsNames.Contains(card.Name))
                {
                    Console.WriteLine("This card was already been chosen");
                    PrintToMessageBox("This card was already been chosen");
                    return;
                }

                _chosenCards.Add((CardHandler.GetIdByCardName(card.Name), card.Name));
                Console.WriteLine($"Card of type {card.Type} and name of {card.Name} was choosen for play");
                PrintToMessageBox($"Card of type {card.Type} and name of {card.Name} was choosen for play");

                var leftToChoose = _numberOfCardsToPlay - _chosenCards.Count;
                Dispatcher.UIThread.Invoke(() =>
                {
                    NumberOfCardsNeededToPlay.Text = leftToChoose.ToString();
                    NumberOfCardsChosenToPlay.Text = _chosenCards.Count.ToString();
                });
            }
        }
    }

    private void PrintToMessageBox(string message)
    {
        Dispatcher.UIThread.Invoke(() =>
            MessageStackPanelTextBlock.Text = message);
    }
}
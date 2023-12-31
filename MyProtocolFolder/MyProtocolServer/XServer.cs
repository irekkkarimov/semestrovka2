using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Net.Sockets;
using FluxxGame;
using FluxxGame.Cards.Abstractions;
using FluxxGame.Handlers;
using FluxxGame.PlayerHandler;
using XProtocol;
using XProtocol.CustomPacketTypes;

namespace MyProtocolServer;

public class XServer
{
    private readonly Socket _socket;
    public List<ConnectedClient> Clients { get; }
    public ConnectedClient? TurnClient { get; set; }
    public bool IsGameStarted { get; private set; }
    private int _counter = 0;
    private readonly object _locker = new();
    private GameLogic _gameLogic;

    private bool _listening;
    private bool _stopListening;

    public XServer()
    {
        var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = ipHostInfo.AddressList[0];
        Console.WriteLine(ipAddress.AddressFamily);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Clients = new List<ConnectedClient>();
    }

    public void Start()
    {
        if (_listening)
        {
            throw new Exception("Server is already listening incoming requests.");
        }

        _socket.Bind(new IPEndPoint(IPAddress.Any, 4910));
        _socket.Listen(10);

        _listening = true;
    }

    public void Stop()
    {
        if (!_listening)
        {
            throw new Exception("Server is already not listening incoming requests.");
        }

        _stopListening = true;
        _socket.Shutdown(SocketShutdown.Both);
        _listening = false;
    }

    public void AcceptClients()
    {
        while (true)
        {
            if (_stopListening)
            {
                return;
            }

            Socket client;

            try
            {
                client = _socket.Accept();
            }
            catch
            {
                return;
            }

            Console.WriteLine($"[!] Accepted client from {(IPEndPoint)client.RemoteEndPoint}");

            var c = new ConnectedClient(client, this);

            if (IsGameStarted)
                continue;

            Clients.Add(c);
        }
    }

    public void SendUserListToAllUsers()
    {
        if (Clients.Count == 1 || TurnClient == null)
            TurnClient = Clients.First();

        var userList = new XPacketUserList
        {
            UserList = Clients.Select(i => i.Username).ToList(),
            TurnUser = TurnClient.Username
        };

        foreach (var client in Clients)
        {
            client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.UserList, userList).ToPacket());
        }
    }

    public void StartGame()
    {
        if (IsGameStarted)
            return;

        Console.WriteLine("Game is starting...");
        _gameLogic = GameLogic.Start();
        IsGameStarted = true;
        var players = new List<Player>();

        if (Clients.Count <= 4)
        {
            players = Clients.Select(i => Player.Create(i.Username)).ToList();
        }

        foreach (var player in players)
        {
            _gameLogic.AddPlayer(player);
            _gameLogic.DrawGameStart(player);
        }

        SendCards();
        Task.Run(CheckGameLogicWin);
    }

    public void DrawCards(ConnectedClient client)
    {
        if (_gameLogic.CurrentTurnDrawn)
            return;

        Console.WriteLine(_gameLogic.CurrentTurnDrawn);
        _gameLogic.ProcessTurn();

        SendCards();
    }

    public void SendCards()
    {
        List<string[]> allPlayersInPlayInStringArrayList = new();
        var turn = _gameLogic.Turn.Username;
        var isTurnDrawn = _gameLogic.CurrentTurnDrawn;

        foreach (var player in _gameLogic.Players)
        {
            allPlayersInPlayInStringArrayList
                .AddRange(player.Keepers
                    .Select(card =>
                    {
                        var id = CardHandler.GetIdByShowCard(card);
                        return Parser.ParseKeeperInPlayCardToStringArray(id, player.Username);
                    })
                );
        }

        foreach (var client in Clients)
        {
            var currentPlayer = _gameLogic.Players
                .FirstOrDefault(i => i.Username.Equals(client.Username));

            var currentPlayerInHand = currentPlayer.InHand;

            var currentPlayerInHandInIdArray = currentPlayerInHand
                .Select(CardHandler.GetIdByShowCard)
                .ToList();

            Console.WriteLine($"Count of clients: {Clients.Count}");

            var currentTurnCardsIds = _gameLogic.CurrentTurnCards.Any()
                ? _gameLogic.CurrentTurnCards.Select(CardHandler.GetIdByShowCard)
                    .ToList()
                : new List<int>();

            var cardsResponse = new XPacketCards
            {
                CurrentPlayerInHand = currentPlayerInHandInIdArray,
                AllPlayersInPlay = allPlayersInPlayInStringArrayList,
                CountOfCardsToDraw = _gameLogic.CountOfCardsMustBeDrawn,
                CountOfCardsToPlay = _gameLogic.CountOfCardsMustBePlayed,
                Goal = _gameLogic.Goal,
                CountOfPlayers = Clients.Count,
                TurnUsername = turn,
                IsDrawing = !isTurnDrawn,
                IsPlaying = isTurnDrawn,
                CardsAllowedToPlayInCurrentPlay = currentTurnCardsIds
            };


            client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.CardsArray, cardsResponse).ToPacket());
        }
    }

    public void PerformTurn(ConnectedClient currentClient, List<int> cardsPlayed)
    {
        if (!currentClient.Username.Equals(_gameLogic.Turn.Username))
        {
            Console.WriteLine($"{currentClient.Username} tried to make a move, but the turn of {TurnClient.Username}");
            return;
        }

        lock (_locker)
        {
            var cardsParsed = cardsPlayed
                .Select(CardHandler.GetICardById)
                .ToList();

            Console.WriteLine(_gameLogic.CurrentTurnDrawn);
            Console.WriteLine(cardsParsed.Count);

            _gameLogic.ProcessTurn(cardsParsed);
            // SwitchTurn();
            SendCards();
        }
    }

    private void CheckGameLogicWin()
    {
        var gameResult = _gameLogic.CheckWinner();
        Console.WriteLine("game stopped");

        var winPacket = new XPacketWin
        {
            WinnerUsername = gameResult.Item1,
            Goal = gameResult.Item2,
            Message = gameResult.Item3
        };

        foreach (var client in Clients)
        {
            client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.Win, winPacket).ToPacket());
        }
    }

    // private void SwitchTurn()
    // {
    //     var index = Clients.IndexOf(TurnClient);
    //     if (index == Clients.Count - 1)
    //         TurnClient = Clients[0];
    //     else
    //         TurnClient = Clients[index + 1];
    // }
}
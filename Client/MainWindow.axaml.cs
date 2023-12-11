using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using XProtocol;
using XProtocol.CustomPacketTypes;

namespace Client;

public partial class MainWindow : Window
{
    public static int HandshakeMagic { get; private set; }
    private XClient _client = null!;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SendUserName(object? sender, RoutedEventArgs e)
    {
        if (_client != null!)
            return;

        _client = new XClient();
        Task.Run(Start);
        
        Thread.Sleep(2000);
        
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
        while(true) {}
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
        Console.WriteLine(parsed.PacketType);
        if (parsed != null)
            ProcessIncomingPacket(parsed);
    }
    
    private  void ProcessIncomingPacket(XPacket packet)
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
        Console.WriteLine(turnResponse.Counter);
        Dispatcher.UIThread.Invoke(() => Counter.Text = turnResponse.Counter.ToString());

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
    
    private void DisconnectButton(object? sender, RoutedEventArgs e)
    {
        Disconnect();
        Close();
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
}
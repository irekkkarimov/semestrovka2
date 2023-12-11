using System.Net;
using System.Net.Sockets;
using XProtocol;
using XProtocol.CustomPacketTypes;

namespace MyProtocolServer;

public class XServer
{
    private readonly Socket _socket;
    public List<ConnectedClient> Clients { get; }
    public ConnectedClient TurnClient { get; set; }
    private int _counter = 0;
    private readonly object _locker = new();

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
            } catch { return; }

            Console.WriteLine($"[!] Accepted client from {(IPEndPoint) client.RemoteEndPoint}");

            var c = new ConnectedClient(client, this);
            Clients.Add(c);
        }
    }

    public void SendUserListToAllUsers()
    {
        var userList = new XPacketUserList
        {
            UserList = Clients.Select(i => i.Username).ToList()
        };
        
        foreach (var client in Clients)
        {
            client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.UserList, userList).ToPacket());
        }
    }

    public void PerformTurn(ConnectedClient currentClient)
    {
        if (currentClient != TurnClient)
            return;

        lock (_locker)
        {
            _counter++;
            var turnResponse = new XPacketTurnResponse { Counter = _counter };
            foreach (var client in Clients)
                client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.TurnResponse, turnResponse).ToPacket());
        }
    }
}
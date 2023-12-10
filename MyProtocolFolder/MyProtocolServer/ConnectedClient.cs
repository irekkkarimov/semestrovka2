using System.Net.Sockets;
using XProtocol;
using XProtocol.CustomPacketTypes;

namespace MyProtocolServer;

public class ConnectedClient
{
    public Socket Client { get; }
    private readonly XServer _server;
    public string Username { get; set; }

    private readonly Queue<byte[]> _packetSendingQueue = new Queue<byte[]>();

    public ConnectedClient(Socket client, XServer server)
    {
        Client = client;
        _server = server;

        Task.Run(ProcessIncomingPackets);
        Task.Run(SendPackets);
        Task.Run(CheckClientConnection);
    }

    private void ProcessIncomingPackets()
    {
        while (true) // Слушаем пакеты, пока клиент не отключится.
        {
            var buff = new byte[256]; // Максимальный размер пакета - 256 байт.
            Client.Receive(buff);

            buff = buff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return buff[i + 1] != 0;
            }).Concat(new byte[] { 0xFF, 0 }).ToArray();

            var parsed = XPacket.Parse(buff);

            if (parsed != null)
            {
                ProcessIncomingPacket(parsed);
            }
        }
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
                ProcessString(packet);
                break;
            case XPacketType.Username:
                ProcessUsername(packet);
                break;
            case XPacketType.UserDisconnection:
                ProcessUserDisconnection();
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ProcessHandshake(XPacket packet)
    {
        Console.WriteLine("Recieved handshake packet.");

        var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);
        handshake.MagicHandshakeNumber -= 15;

        Console.WriteLine("Answering..");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Handshake, handshake).ToPacket());
    }

    private void ProcessString(XPacket packet)
    {
        Console.WriteLine("Recieved string packet.");

        var stringPacket = XPacketConverter.Deserialize<XPacketString>(packet);

        Console.WriteLine($"Message: {stringPacket.TestString}");
        stringPacket.TestString = stringPacket.TestString + "__" + "ANSWER";
        Console.WriteLine("Answering..");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.String, stringPacket).ToPacket());
    }

    private void ProcessUsername(XPacket packet)
    {
        Console.WriteLine("Recieved username packet.");

        var usernamePacket = XPacketConverter.Deserialize<XPacketUsername>(packet);

        Console.WriteLine($"Username: {usernamePacket.Name}");
        Username = usernamePacket.Name;
        usernamePacket.IsProcessed = true;
        Console.WriteLine("Answering..");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Username, usernamePacket).ToPacket());
        _server.SendUserListToAllUsers();
    }

    private void CheckClientConnection()
    {
        while (Client.Connected)
        {
            Task.Delay(5000);
        }
        
        ProcessUserDisconnection();
    }
    
    private void ProcessUserDisconnection()
    {
        _server.Clients.Remove(this);
        _server.SendUserListToAllUsers();
        Client.Close();
        Console.WriteLine($"User '{Username}' was disconnected");
    }

    public void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 256)
        {
            throw new Exception("Max packet size is 256 bytes.");
        }

        _packetSendingQueue.Enqueue(packet);
    }

    private void SendPackets()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
            {
                Thread.Sleep(100);
                continue;
            }

            var packet = _packetSendingQueue.Dequeue();
            Client.Send(packet);

            Thread.Sleep(100);
        }
    }
}
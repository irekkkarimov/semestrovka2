using System;
using System.Threading;
using XProtocol;
using XProtocol.CustomPacketTypes;

namespace Client;

public class ClientProtocolHandler
{
    public static int HandshakeMagic { get; private set; }
    private XClient _client;
    
    public void Start()
    {
        Console.Title = "XClient";
        Console.ForegroundColor = ConsoleColor.White;
            
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

        SendStringPacket(new XPacketString
        {
            TestString = "Hello world!"
        });
        
        while(true) {}
    }

    private bool SendStringPacket(XPacketString packet)
    {
        _client.QueuePacketSend(
            XPacketConverter.Serialize(
                    XPacketType.String,
                    packet)
                .ToPacket());

        return true;
    }

    private static void OnPacketReceive(byte[] packet)
    {
        var parsed = XPacket.Parse(packet);
        
        if (parsed != null)
            ProcessIncomingPacket(parsed);
    }
    
    private static void ProcessIncomingPacket(XPacket packet)
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
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void ProcessHandshake(XPacket packet)
    {
        var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

        if (HandshakeMagic - handshake.MagicHandshakeNumber == 15)
        {
            Console.WriteLine("Handshake successful!");
        }
    }

    private static void ProcessString(XPacket packet)
    {
        var stringPacket = XPacketConverter.Deserialize<XPacketString>(packet);
        
        if (!string.IsNullOrEmpty(stringPacket.TestString))
            Console.WriteLine(stringPacket.TestString);
    }
}
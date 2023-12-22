namespace XProtocol;

public static class XPacketTypeManager
{
    private static readonly Dictionary<XPacketType, Tuple<byte, byte>> TypeDictionary = new();

    static XPacketTypeManager()
    {
        RegisterType(XPacketType.Handshake, 1, 0);
        RegisterType(XPacketType.String, 2, 0);
        RegisterType(XPacketType.Username, 3, 0);
        RegisterType(XPacketType.UserList, 4, 0);
        RegisterType(XPacketType.UserDisconnection, 5, 0);
        RegisterType(XPacketType.CardJson, 6, 0);
        RegisterType(XPacketType.CardsArray, 7, 0);
        RegisterType(XPacketType.GameStart, 8, 0);
        RegisterType(XPacketType.TurnRequest, 9, 0);
        RegisterType(XPacketType.TurnResponse, 10, 0);
        RegisterType(XPacketType.DrawRequest, 11, 0);
        RegisterType(XPacketType.Win, 12, 0);
    }
    
    public static void RegisterType(XPacketType type, byte bType, byte bSubtype)
    {
        if (TypeDictionary.ContainsKey(type))
            throw new Exception($"Packet type {type:G} is already registered");
        
        TypeDictionary.Add(type, Tuple.Create(bType, bSubtype));
    }

    public static Tuple<byte, byte> GetType(XPacketType type)
    {
        if (!TypeDictionary.ContainsKey(type))
            throw new Exception($"Packet type {type:G} is not registered");

        return TypeDictionary[type];
    }

    public static XPacketType GetTypeFromPacket(XPacket packet)
    {
        var type = packet.PacketType;
        var subtype = packet.PacketSubtype;

        foreach (var tuple in TypeDictionary)
        {
            var value = tuple.Value;

            if (value.Item1 == type && value.Item2 == subtype)
                return tuple.Key;
        }

        return XPacketType.Unknown;
    }
}
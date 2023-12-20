using System.Text;
using System.Text.Json;

namespace XProtocol;

public class XPacket
{
    public byte PacketType { get; private init; }
    public byte PacketSubtype { get; private init; }
    private List<XPacketField> Fields { get; } = new();

    // public bool Protected { get; set; }
    private bool ChangeHeaders => false;

    private XPacket()
    {
    }

    private XPacketField GetField(byte id) => Fields.FirstOrDefault(field => field.FieldId == id)!;

    public bool HasField(byte id) => GetField(id) != null!;
    
    
    public T GetValue<T>(byte id)
    {
        var field = GetField(id);

        if (field == null)
            throw new Exception($"Field with ID {id} wasn't found.");

        var jsonString = Encoding.UTF8.GetString(field.Contents);
        return JsonSerializer.Deserialize<T>(jsonString);
        JsonSerializer.Deserialize<T>(jsonString);
    }

    public void SetValue(byte id, object? obj)
    {
        var field = GetField(id);

        if (field == null!)
        {
            field = new XPacketField
            {
                FieldId = id
            };

            Fields.Add(field);
        }

        var jsonString = JsonSerializer.Serialize(obj);
        var bytes = Encoding.UTF8.GetBytes(jsonString);


        if (bytes.Length > byte.MaxValue)
            throw new Exception("Object is too big. Max length is 255 bytes.");

        field.FieldSize = (byte)bytes.Length;
        field.Contents = bytes;
    }

    public static XPacket Create(byte type, byte subtype)
    {
        return new XPacket
        {
            PacketType = type,
            PacketSubtype = subtype
        };
    }

    public byte[] ToPacket()
    {
        var packet = new MemoryStream();

        packet.Write(
            ChangeHeaders
                ? new byte[] { 0x95, 0xAA, 0xFF, PacketType, PacketSubtype }
                : new byte[] { 0xAF, 0xAA, 0xAF, PacketType, PacketSubtype }, 0, 5);

        // Сортируем поля по ID
        var fields = Fields.OrderBy(field => field.FieldId);

        // Записываем поля
        foreach (var field in fields)
        {
            packet.Write(new[] { field.FieldId, field.FieldSize }, 0, 2);
            packet.Write(field.Contents, 0, field.Contents.Length);
        }

        // Записываем конец пакета
        packet.Write(new byte[] { 0xFF, 0x00 }, 0, 2);

        return packet.ToArray();
    }

    public static XPacket Parse(byte[] packet) //bool markAsEncrypted = false
    {
        /*
         * Минимальный размер пакета - 7 байт
         * HEADER (3) + TYPE (1) + SUBTYPE (1) + PACKET ENDING (2)
         */
        if (packet.Length < 7)
            return null!;

        // var encrypted = false;

        // Проверяем заголовок
        if (packet[0] != 0xAF ||
            packet[1] != 0xAA ||
            packet[2] != 0xAF)
        {
            if (packet[0] != 0x95 &&
                packet[1] != 0xAA &&
                packet[2] != 0xFF)
                return null!;
            // else
            // {
            //     encrypted = true;
            // }
        }

        var mIndex = packet.Length - 1;

        // Проверяем, что бы пакет заканчивался нужными байтами
        if (packet[mIndex - 1] != 0xFF ||
            packet[mIndex] != 0x00)
        {
            return null!;
        }

        var type = packet[3];
        var subtype = packet[4];

        // var xpacket = new XPacket {PacketType = type, PacketSubtype = subtype, Protected = markAsEncrypted};

        var xPacket = XPacket.Create(type, subtype);

        var fields = packet.Skip(5).ToArray();

        while (true)
        {
            if (fields.Length == 2)
                return xPacket;

            var id = fields[0];
            var size = fields[1];

            var contents = size != 0 ? fields.Skip(2).Take(size).ToArray() : null;

            xPacket.Fields.Add(new XPacketField
            {
                FieldId = id,
                FieldSize = size,
                Contents = contents!
            });

            fields = fields.Skip(2 + size).ToArray();
        }
    }

    // private byte[] GetValueRaw(byte id)
    // {
    //     var field = GetField(id);
    //
    //     if (field == null)
    //         throw new Exception($"Field with ID {id} wasn't found.");
    //
    //     return field.Contents;
    // }
    //
    // private void SetValueRaw(byte id, byte[] rawData)
    // {
    //     var field = GetField(id);
    //
    //     if (field == null!)
    //     {
    //         field = new XPacketField
    //         {
    //             FieldId = id
    //         };
    //
    //         Fields.Add(field);
    //     }
    //
    //     if (rawData.Length > byte.MaxValue)
    //     {
    //         throw new Exception("Object is too big. Max length is 255 bytes.");
    //     }
    //
    //     field.FieldSize = (byte) rawData.Length;
    //     field.Contents = rawData;
    // }

    // private static XPacket EncryptPacket(XPacket? packet) //Не нужен, оставлю, вдруг пригодится потом
    // {
    //     if (packet == null)
    //         return null!; // Нам попросту нечего шифровать
    //     
    //     var rawBytes = packet.ToPacket(); // получаем пакет в байтах
    //     var encrypted = XProtocolEncryptor.Encrypt(rawBytes); // шифруем его
    //     
    //     var p = Create(0, 0); // создаем пакет
    //     p.SetValueRaw(0, encrypted); // записываем данные
    //     p.ChangeHeaders = true; // помечаем, что нам нужен другой заголовок
    //     
    //     return p;
    // }
    //
    // private static XPacket DecryptPacket(XPacket packet) //Не нужен, оставлю, вдруг пригодится потом
    // {
    //     if (!packet.HasField(0))
    //         return null!; // Зашифрованные данные должны быть в 0 поле
    //
    //     var rawData = packet.GetValueRaw(0); // получаем зашифрованный пакет
    //     var decrypted = XProtocolEncryptor.Decrypt(rawData);
    //
    //     return Parse(decrypted, true);
    // }
}
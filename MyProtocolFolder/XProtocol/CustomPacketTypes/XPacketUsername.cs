namespace XProtocol.CustomPacketTypes;

public class XPacketUsername
{
    [XField(1)] public string Name = "";
    [XField(2)] public bool IsProcessed = false;
}
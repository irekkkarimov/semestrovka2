namespace XProtocol;

public static class XProtocolEncryptor
{
    private static string Key => "2e985f930";

    public static byte[] Encrypt(byte[] data)
    {
        return RijndaelHandler.Encrypt(data, Key);
    }

    public static byte[] Decrypt(byte[] data)
    {
        return RijndaelHandler.Decrypt(data, Key);
    }
}
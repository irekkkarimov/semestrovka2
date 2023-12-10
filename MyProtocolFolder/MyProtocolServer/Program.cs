

namespace MyProtocolServer;

public static class Program
{
    public static void Main(string[] args)
    {
        var server = new XServer();
        server.Start();
        server.AcceptClients();
    }
}
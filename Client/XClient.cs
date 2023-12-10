using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Client;

public class XClient
{
    public Action<byte[]> OnPacketReceive { get; set; }
    private readonly Queue<byte[]> _packetSendingQueue = new();
    private Socket _socket;
    private IPEndPoint _serverEndPoint;

    public void Connect(string ip, int port)
    {
        Connect(new IPEndPoint(IPAddress.Parse(ip), port));
    }

    private void Connect(IPEndPoint server)
    {
        _serverEndPoint = server;

        var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = ipHostInfo.AddressList[0];

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(_serverEndPoint);

        Task.Run(ReceivePackets);
        Task.Run(SendPackets);
    }

    public void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 256)
            throw new Exception("Max packet size is 256 bytes");
        
        _packetSendingQueue.Enqueue(packet);
    }

    private void ReceivePackets()
    {
        while (true)
        {
            var buff = new byte[256];
            _socket.Receive(buff);

            buff = buff.TakeWhile((b, i) =>
            {
                if (b != 0xFF)
                    return true;
                return buff[i + 1] != 0;
            }).Concat(new byte[] { 0xFF, 0 }).ToArray();

            OnPacketReceive?.Invoke(buff);
        }
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
            _socket.Send(packet);
            Thread.Sleep(100);
        }
    }
}
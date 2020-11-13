using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HolePuncher
{
    class Client
    {
        UdpClient udpClient;
        IPAddress localIpAddress;
        IPAddress remoteIpAddress;
        IPEndPoint localEndpoint;
        IPEndPoint remoteEndpoint;
        State state = new State();
        private const int bufSize = 8 * 1024;
        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public Client(IPAddress localIpAddress, int localOutboundPort, IPAddress remoteIpAddress, int remotePort)
        {
            udpClient = new UdpClient();
            this.localIpAddress = localIpAddress;
            this.remoteIpAddress = remoteIpAddress;
            localEndpoint = new IPEndPoint(localIpAddress, localOutboundPort);
            remoteEndpoint = new IPEndPoint(remoteIpAddress, remotePort);
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndpoint);
            udpClient.Connect(remoteEndpoint);
            //udpClient.AllowNatTraversal(true);
        }

        public void SendMessage(string text)
        {
            string outboundMessage = $"Content: {text} \n Sent at: {DateTime.Now}\n Outgoing address: {GetLocalAddress()} \n Target address: {remoteEndpoint}";
            byte[] message = Encoding.ASCII.GetBytes(outboundMessage);
            udpClient.BeginSend(message, message.Length, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = udpClient.EndSend(ar);
                Console.WriteLine("-----------------------");
                Console.WriteLine($"SENT: \n {outboundMessage} \n Sent bytes: {bytes} \n");
            }, state);
        }

        public string GetLocalAddress()
        {
            return udpClient.Client.LocalEndPoint.ToString();
        }
    }
}

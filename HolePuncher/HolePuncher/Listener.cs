using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HolePuncher
{
    class Listener
    {
        UdpClient listenerClient;
        IPAddress localIpAddress;
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.153"), 6000);
        State state = new State();
        private IPEndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private const int bufSize = 8 * 1024;
        private AsyncCallback recv = null;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public Listener(IPAddress localIpAddress, int localInboundPort)
        {
            listenerClient = new UdpClient();
            this.localIpAddress = localIpAddress;
            localEndPoint = new IPEndPoint(localIpAddress, localInboundPort);
            listenerClient.ExclusiveAddressUse = false;
            listenerClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listenerClient.Client.Bind(localEndPoint);
            //listenerClient.AllowNatTraversal(true);
        }
        
        public void Listen()
        {
            listenerClient.BeginReceive(recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                byte[] bytes = listenerClient.EndReceive(ar, ref epFrom);
                listenerClient.BeginReceive(recv, so);
                Console.WriteLine("-----------------------");
                Console.WriteLine($"RECV: \n Received from: {epFrom.ToString()}  \n Received on: {GetLocalAddress()} \n Received bytes: {bytes.Length} \n ---- \n Received message: \n {Encoding.ASCII.GetString(bytes, 0, bytes.Length)} \n");
            }, state);
        }
        
        public string GetLocalAddress()
        {
            return listenerClient.Client.LocalEndPoint.ToString();
        }
    }
}

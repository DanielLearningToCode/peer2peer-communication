
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPsocketTest
{
    static class Program
    {
        public static Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public const int bufSize = 8 * 1024;
        public static State state = new State();
        public static EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        public static AsyncCallback recv = null;
        public static IPAddress localIp;
        public static int localPort;
        public static IPEndPoint localEndpoint;
        public static IPAddress remoteIp;
        public static int remotePort;
        public static IPEndPoint remoteEndPoint;
        
        static void Main(string[] args)
        {
            IPHostEntry iPHost = Dns.GetHostEntry(Dns.GetHostName());
            localIp = iPHost.AddressList[1];
            if (!args.Any() || args.Length < 3)
            {
                Console.WriteLine(Help());
                Console.ReadLine();
                return;
            }
            else
            {
                try
                {
                    localPort = Convert.ToInt32(args[0]);
                    remoteIp = IPAddress.Parse(args[1]);
                    remotePort = Convert.ToInt32(args[2]);
                }
                catch (Exception e) when (e is ArgumentException || e is FormatException)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(Help());
                    Console.ReadLine();
                    return;
                }
            }
           
            localEndpoint = new IPEndPoint(localIp, localPort);
            remoteEndPoint = new IPEndPoint(remoteIp, remotePort);
            socket1.ExclusiveAddressUse = false;
            socket1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket1.Bind(localEndpoint);
            socket1.Connect(remoteEndPoint);
            Receive();
            
            do
            {
                Console.WriteLine("Enter the text to send: ");
                Send(Console.ReadLine());
                Console.WriteLine("Press q and hit enter to quit or any key to continue");
            }
            while (Console.ReadKey().KeyChar != 'q');
            Console.ReadKey();
        }

        private static void Receive()
        {
            socket1.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = socket1.EndReceiveFrom(ar, ref epFrom);
                socket1.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                Console.WriteLine("-----------------------");
                Console.WriteLine($"RECV: \n Received from: {epFrom} \n Received on: {socket1.LocalEndPoint} \n Received bytes: {bytes} \n ---- \n Received message: \n {Encoding.ASCII.GetString(so.buffer, 0, bytes)} \n");
            }, state);
        }

        public static void Send(string text)
        {
            string outboundMessage = $"Content: {text} \n Sent at: {DateTime.Now}\n Outgoing address: {socket1.LocalEndPoint}";
            byte[] data = Encoding.ASCII.GetBytes(outboundMessage);
            socket1.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = socket1.EndSend(ar);
                Console.WriteLine("-----------------------");
                Console.WriteLine($"SENT: \n {outboundMessage} \n Sent bytes: {bytes} \n");
            }, state);
        }

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public static string Help()
        {
            return "Usage: NameOfTheApp.exe <local port> <destination ip> <destination port>";
        }

    }
}


using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HolePuncher
{
    class Program
    {
        private static IPAddress localIpAddress;
        private static int localInboundPort;
        private static int localOutboundPort;
        private static IPAddress remoteIpAddress;
        private static int remotePort;
        static void Main(string[] args)
        {

            if (!args.Any() || args.Length < 5)
            {
                Console.WriteLine(Help());
                Console.ReadLine();
                return;
            }

            try
            {
                localIpAddress = IPAddress.Parse(args[0]);
                localInboundPort = Convert.ToInt32(args[1]);
                localOutboundPort = Convert.ToInt32(args[2]);
                remoteIpAddress = IPAddress.Parse(args[3]);
                remotePort = Convert.ToInt32(args[4]);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(Help());
                Console.ReadLine();
                return;
            }

            Listener listener = new Listener(localIpAddress, localInboundPort);
            Client client = new Client(localIpAddress, localOutboundPort, remoteIpAddress, remotePort);
            listener.Listen();

            do
            {
                Console.WriteLine("Enter the text to send: ");
                client.SendMessage(Console.ReadLine());
                Console.WriteLine("Press q and hit enter to quit or any key to continue");
            }
            while (Console.ReadKey().KeyChar != 'q');
            Console.ReadKey();
        }

        public static string Help()
        {
            return "Usage: NameOfTheApp.exe <local ip> <local inbound port> <local outbound port> <destination ip> <destination port>";
        }
    }
}

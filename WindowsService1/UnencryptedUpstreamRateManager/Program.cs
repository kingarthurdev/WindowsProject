using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;

//When recieve packets on port 12001, forward to _________________:12000
//when recieve packets on port 1544 (should be response?), forward to __________________:1543

namespace UnencryptedUpstreamRateManager
{
    internal class Program
    {
        public static int listeningPort1 = 1544;
        public static int listeningPort2 = 12001;

        public static int destinationPort = 1543;
        public static int destinationPort2 = 12000;

        public static IPEndPoint endpoint1;
        public static IPEndPoint endpoint2;

        public static ConcurrentQueue<byte[]> bytesQueue1 = new ConcurrentQueue<byte[]>();
        public static ConcurrentQueue<byte[]> bytesQueue2 = new ConcurrentQueue<byte[]>();

        public static Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        async static Task Main(string[] args)
        {
            Console.Title = "Upstream Filtering Proxy";

            Console.WriteLine("Enter the IP Address listening on port 1543:");
            //recipient address and port
            endpoint1 = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort);
            Console.WriteLine("Listening on port " + listeningPort1 + " and forwarding valid traffic to " + endpoint1.Address.ToString() + ":1543 \n\n");

            Console.WriteLine("Enter the IP Address listening on port 12000:");
            //recipient address and port
            endpoint2 = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort2);
            Console.WriteLine("Listening on port " + listeningPort2 + " and forwarding valid traffic to " + endpoint2.Address.ToString() + ":12000");

            Task.Run(listenForMessages);
            Task.Run(listenForMessagesOnSecondaryPort);
            Task.Run(queueEater);

            Process.GetCurrentProcess().WaitForExit(); // prevents program from immediately exiting on main completion.

        }

        async static void listenForMessages()
        {
            UdpClient listener = new UdpClient(listeningPort1);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort1);

            while (true)
            {
                byte[] bytes = (await listener.ReceiveAsync()).Buffer; // listener.RecieveAsync originally returns UdpReceiveResult
                Task.Run(() => {
                    verify(bytes, false);
                });

            }
        }

        async static void listenForMessagesOnSecondaryPort()
        {
            UdpClient listener = new UdpClient(listeningPort2);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort2);
            while (true)
            {
                byte[] bytes = (await listener.ReceiveAsync()).Buffer; // listener.RecieveAsync originally returns UdpReceiveResult

                Task.Run(() => {
                    verify(bytes, true);
                });

            }
        }

        async static Task verify(byte[] bytes, bool tf) // tf regulates bytesQueue1 or 2
        {
            if ((bytes.Length >= 12))
            { //&& (Encoding.ASCII.GetString(bytes, 8, 3).Equals("ack") || Encoding.ASCII.GetString(bytes, 13, 17).Equals("\0Request For Data"))){
                if (tf)
                {
                    bytesQueue2.Enqueue(bytes);

                }
                else
                {
                    bytesQueue1.Enqueue(bytes);

                }

                Console.WriteLine("added to queue");
            }
        }
        async static Task forwardBytes(byte[] bytes, EndPoint endpoint)
        {
            /*if (Encoding.ASCII.GetString(bytes, 8, 3).Equals("ack"))
            {
                sock.SendTo(bytes, endpoint);
                Console.WriteLine("Valid data forwarded.");
            }
            else if(bytes.Length >= 21 && Encoding.ASCII.GetString(bytes, 13, 17).Equals("\0Request For Data")) // 5+8 comes from 5 for mssg count + delim, 8 comes from time, \0 is null char, idk 
            {
                if(endpoint == endpoint2)
                {
                    sock.SendTo(bytes, endpoint2);
                    Console.WriteLine("Valid data forwarded.");
                }

            }
            else
            {
                Console.WriteLine("Invalid.");
            }*/
            if (bytes.Length >= 12)
            {
                sock.SendTo(bytes, endpoint);
                Console.WriteLine("Valid data forwarded.");
            }
        }

        async static Task queueEater()
        {
            while (true)
            {
                byte[] temp;

                if (bytesQueue1.TryDequeue(out temp))
                {
                    forwardBytes(temp, endpoint1);
                    Console.WriteLine("Dequeued1");
                }

                if (bytesQueue2.TryDequeue(out temp))
                {
                    forwardBytes(temp, endpoint2);
                    Console.WriteLine("Dequeued2");
                }

                Thread.Sleep(10);
            }

        }
    }
}
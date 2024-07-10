using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Threading;
using System.Text.RegularExpressions;

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


        public static int packetsCount;
        public static IPEndPoint endpoint1;
        public static IPEndPoint endpoint2;

        public static bool startQueue;
        public static Queue<byte[]> bytesQueue1 = new Queue<byte[]>();
        public static Queue<byte[]> bytesQueue2 = new Queue<byte[]>();

        public static Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static int maxPacketsRate = 1; // number of packets every 2 sec --> 10,000 pkts per second

        static void Main(string[] args)
        {
            //ask user where they'd like to send packets -- becomes the destination address
            //accept packets to certain port
            //store all packets recieved in a queue -- as fast as possible
            //process packets at a fixed rate -- should be pretty fast to ensure that queue doesn't introduce unnecessary latency
            //possibly onlys start storing in a queue when incomming packets gets too fast

            Thread clear = new Thread(new ThreadStart(clearPacketCount));
            clear.Start();

            Thread queueProcessor = new Thread(() => queueEater(1));
            queueProcessor.Start();

            Thread queueProcessor2 = new Thread(() => queueEater(2));
            queueProcessor2.Start();

            try
            {
                
                Console.WriteLine("Enter the IP Address listening on port 1543:");
                //recipient address and port
                endpoint1 = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort);
                Console.WriteLine("Listening on port " + listeningPort1 + " and forwarding valid traffic to "+ endpoint1.Address.ToString()+":1543");


                Console.WriteLine("Enter the IP Address listening on port 12000:");
                //recipient address and port
                endpoint2 = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort2);
                Console.WriteLine("Listening on port " + listeningPort2 + " and forwarding valid traffic to " + endpoint2.Address.ToString() + ":12000");

                Thread listener = new Thread(new ThreadStart(listenForMessages));
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }


        }
        static void clearPacketCount()
        {
            while (true)
            {
                packetsCount = 0; 
                Thread.Sleep(2000);
            }

        }
        static void listenForMessages()
        {
            UdpClient listener = new UdpClient(listeningPort1);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort1);

            Thread secondaryThread = new Thread(new ThreadStart(listenForMessagesOnSecondaryPort));
            secondaryThread.Start();

            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);              
                packetsCount++;
                if(packetsCount < maxPacketsRate)
                {
                    forwardBytes(bytes, endpoint1);
                }
                else
                {
                    bytesQueue1.Enqueue(bytes);
                    Console.WriteLine("added to queue");
                }

            }
        }

        static void listenForMessagesOnSecondaryPort()
        {
            UdpClient listener = new UdpClient(listeningPort2);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort2);
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                packetsCount++;
                if (packetsCount < maxPacketsRate)
                {
                    forwardBytes(bytes, endpoint2);
                }
                else
                {
                    bytesQueue2.Enqueue(bytes);
                    Console.WriteLine("added to queue");
                }

            }
        }

        static void forwardBytes(byte[] bytes, EndPoint endpoint)
        {
            if ((bytes.Length>= 12) && Encoding.ASCII.GetString(bytes, 8, 3).Equals("ack"))
            {
                //basic data validation -- valid date, valid ack, valid xml
                DateTime send = DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));

                byte[] XMLBytes = new byte[bytes.Length - 15];
                Buffer.BlockCopy(bytes, 15, XMLBytes, 0, XMLBytes.Length);
                string xmlString = Encoding.ASCII.GetString(XMLBytes, 0, XMLBytes.Length);
                try
                {
                    Console.WriteLine(XElement.Parse(xmlString).ToString() + "\n\n\n");
                }
                catch
                {
                    Console.WriteLine("Invalid XML Recieved");
                }

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
            }
            
        }

        static void queueEater(int whichOne)
        {
            if (whichOne == 1)
            {
                while (true)
                {
                    while (bytesQueue1.Count > 0)
                    {
                        forwardBytes(bytesQueue1.Dequeue(), endpoint1);
                        Console.WriteLine("Dequeued");
                        Thread.Sleep(50);
                    }
                    Thread.Sleep(500);
                }
            }
            else
            {
                while (true)
                {
                    while (bytesQueue2.Count > 0)
                    {
                        forwardBytes(bytesQueue2.Dequeue(), endpoint2);
                        Console.WriteLine("Dequeued2");
                        Thread.Sleep(50);
                    }
                    Thread.Sleep(500);
                }
            }
            
        }
    }
}

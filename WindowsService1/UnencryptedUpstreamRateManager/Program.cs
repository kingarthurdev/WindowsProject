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


        public static int packetsCount;
        public static IPEndPoint endpoint1;
        public static IPEndPoint endpoint2;

        public static bool startQueue;
        public static ConcurrentQueue<byte[]> bytesQueue1 = new ConcurrentQueue<byte[]>();
        public static ConcurrentQueue<byte[]> bytesQueue2 = new ConcurrentQueue<byte[]>();

        public static Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //public static int maxPacketsRate = 1; // number of packets every 2 sec --> 10,000 pkts per second

        async static Task Main(string[] args)
        {
                //ask user where they'd like to send packets -- becomes the destination address
                //accept packets to certain port
                //store all packets recieved in a queue -- as fast as possible
                //process packets at a fixed rate -- should be pretty fast to ensure that queue doesn't introduce unnecessary latency
                //possibly onlys start storing in a queue when incomming packets gets too fast

                // probably bad practice, instead do async??
                /*Thread clear = new Thread(new ThreadStart(clearPacketCount));
                clear.Start();*/

                Console.WriteLine("Enter the IP Address listening on port 1543:");
                //recipient address and port
                endpoint1 = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort);
                Console.WriteLine("Listening on port " + listeningPort1 + " and forwarding valid traffic to "+ endpoint1.Address.ToString()+":1543 \n\n");


                Console.WriteLine("Enter the IP Address listening on port 12000:");
                //recipient address and port
                endpoint2 = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort2);
                Console.WriteLine("Listening on port " + listeningPort2 + " and forwarding valid traffic to " + endpoint2.Address.ToString() + ":12000");

                Task.Run(queueEater);   //for some reason, this makes the program stop...
                                        //Thread queueProcessor = new Thread(new ThreadStart(queueEater));
                                        // queueProcessor.Start();
                Task.Run(listenForMessages);
                Task.Run(listenForMessagesOnSecondaryPort);


            /*Thread listener = new Thread(new ThreadStart(listenForMessages));
            listener.Start();

            Thread secondaryThread = new Thread(new ThreadStart(listenForMessagesOnSecondaryPort));
            secondaryThread.Start();
            */

            Process.GetCurrentProcess().WaitForExit(); // prevents program from immediately exiting on main completion.

        }

        async static void listenForMessages()
        {
            UdpClient listener = new UdpClient(listeningPort1);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort1);

            while (true)
            {
                byte[] bytes  = (await listener.ReceiveAsync()).Buffer; // listener.RecieveAsync originally returns UdpReceiveResult
                Task.Run(() => { verify(bytes, false); });

            }
        }

        async static void listenForMessagesOnSecondaryPort()
        {
            UdpClient listener = new UdpClient(listeningPort2);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort2);
            while (true)
            {
                byte[] bytes = (await listener.ReceiveAsync()).Buffer; // listener.RecieveAsync originally returns UdpReceiveResult

                Task.Run(() => { verify(bytes, true); });

             }
        }

        async static Task verify(byte[] bytes, bool tf) // tf regulates bytesQueue1 or 2
        {
            if ((bytes.Length >= 12))
            {//&& (Encoding.ASCII.GetString(bytes, 8, 3).Equals("ack") || Encoding.ASCII.GetString(bytes, 13, 17).Equals("\0Request For Data"))){
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
            if (Encoding.ASCII.GetString(bytes, 8, 3).Equals("ack"))
            {
                /*//basic data validation -- valid date, valid ack, valid xml
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
                }*/

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

        async static Task queueEater()
        {
            while (true)
            {
                byte[] temp;
                    if(bytesQueue1.Count > 0)
                    {
                        if(bytesQueue1.TryDequeue(out temp))
                        forwardBytes(temp, endpoint1);
                        Console.WriteLine("Dequeued1");
                    }
                    if (bytesQueue2.Count > 0)
                    {
                    if (bytesQueue2.TryDequeue(out temp))
                        forwardBytes(temp, endpoint2);
                    Console.WriteLine("Dequeued2");
                    }
            }
             
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using dotNetClassLibrary;
using System.Threading;
using System.Xml.Linq;

namespace RateFilter
{
    internal class Program
    {
        public static int listeningPort = 1543;
        public static int destinationPort = 12000;

        public static Dictionary<string, int> ipCount = new Dictionary<string, int>();
        public static List<string> Blacklist = new List<string>(); // chg to dict? 
        public static int clearCount = 0;
        static void Main(string[] args)
        {
            Thread clear = new Thread(new ThreadStart(clearDict));
            clear.Start();

            try
            {
                Thread listener = new Thread(new ThreadStart(listenForACK));
                listener.Start();

                byte[] sendBytes;
                uint count = 1;
                Console.WriteLine("Enter the IP of where you would like to send a message:");
                IPAddress ip = IPAddress.Parse(Console.ReadLine());

                Console.WriteLine("Enter the port of where you would like to send to (default = 12000, 12001 if you would like to use a proxy):");
                destinationPort = Int32.Parse(Console.ReadLine());

                //recipient address and port
                IPEndPoint endpoint = new IPEndPoint(ip, destinationPort);

                //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                while (true)
                {
                    sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");
                    sock.SendTo(sendBytes, endpoint);
                    Console.WriteLine("Ping sent");
                    count++;
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            
        }
        static void clearDict()
        {
            while (true)
            {
                if(clearCount == 120)
                {
                    clearCount = 0;
                    Blacklist.Clear();
                }
                ipCount.Clear();
                Thread.Sleep(2000);
                clearCount++;
                //Console.WriteLine("CLEAR");
            }
            
        }
        static void listenForACK()
        {
            UdpClient listener = new UdpClient(listeningPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort);
            
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                string ip = groupEP.Address.ToString();
                if (!Blacklist.Contains(ip))
                {
                    if (ipCount.ContainsKey(ip))
                    {
                        ipCount[ip] += 1;
                        if (ipCount[ip] > 500)
                        {
                            Console.WriteLine("IP Address Blocked!");
                            Blacklist.Add(ip);
                        }
                    }
                    else
                    {
                        ipCount.Add(ip, 1);
                    }
                    try
                    {
                        //Format: first 8 are for datetime, next 3 are ack, last 4 are message num
                        //indexes 0-14
                        //last 4 bytes are uint coding for message #
                        if (Encoding.ASCII.GetString(bytes, 8, 3).Equals("ack"))
                        {
                            DateTime send = DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
                            double latencyMilliseconds = (DateTime.Now - send).Milliseconds;
                            uint MessageNum = BitConverter.ToUInt32(bytes, 11); // start index comes from 8 +3 --> 8 = time, 3 = "ack"

                            byte[] XMLBytes = new byte[bytes.Length - 15];
                            Buffer.BlockCopy(bytes, 15, XMLBytes, 0, XMLBytes.Length);
                            string xmlString = Encoding.ASCII.GetString(XMLBytes, 0, XMLBytes.Length);


                            Console.WriteLine($"Response #{MessageNum} recieved from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");
                            ProcessContent.WriteToFile($"Response #{MessageNum} recieved from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");
                            try
                            {
                                Console.WriteLine(XElement.Parse(xmlString).ToString() + "\n\n\n");
                                ProcessContent.WriteToFile("Content:" + XElement.Parse(xmlString).ToString() + "\n\n\n");
                            }
                            catch
                            {
                                Console.WriteLine("Invalid XML Recieved");
                                ProcessContent.WriteToFile("Invalid XML Recieved");
                            }

                        }
                        else
                        {
                            Console.WriteLine("Invalid data sent to ack port.");
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Invalid data sent to ack port!");
                    }
                }


            }
        }
    }
}

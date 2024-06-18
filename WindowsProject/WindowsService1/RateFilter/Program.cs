using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using dotNetClassLibrary;
using System.Threading;

namespace RateFilter
{
    internal class Program
    {
        public static Dictionary<string, int> ipCount = new Dictionary<string, int>();
        public static List<string> Blacklist = new List<string>();

        static void Main(string[] args)
        {

            Thread clear = new Thread(new ThreadStart(clearDict));
            clear.Start();

            UdpClient listener = new UdpClient(1001);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 1001);
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                string ip = groupEP.Address.ToString();
                if (!Blacklist.Contains(ip))
                {
                    if (ipCount.ContainsKey(ip)) 
                    {
                        ipCount[ip] += 1;
                        if (ipCount[ip] > 50)
                        {
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
                            uint MessageNum = BitConverter.ToUInt32(bytes, bytes.Length - 4);

                            Console.WriteLine($"ACK #{MessageNum} recieved from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");
                            ProcessContent.WriteToFile($"ACK #{MessageNum} recieved from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");

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
        static void clearDict()
        {
            while (true)
            {
                ipCount.Clear();
                Thread.Sleep(2000);
                Console.WriteLine("CLEAR");
            }
            
        }
    }
}

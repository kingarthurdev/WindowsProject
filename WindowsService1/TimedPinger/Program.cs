using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using dotNetClassLibrary;
namespace TimedPinger
{
    internal class Pinger
    {
        public static int listeningPort = 1543;
        public static int destinationPort = 12000;
        static void Main(string[] args)
        {
            try
            {
                Thread listener = new Thread(new ThreadStart(listenForACK));
                listener.Start();
                
                byte[] sendBytes;
                uint count = 1;
                Console.WriteLine("Enter the IP of where you would like to send a message:");

                //recipient address and port
                IPAddress broadcast = IPAddress.Parse(Console.ReadLine());
                IPEndPoint endpoint = new IPEndPoint(broadcast, destinationPort);

                //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                while (true)
                {
                    sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "ping");
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

        public static void listenForACK()
        {
            Console.WriteLine($"Listening on port {listeningPort} for ack responses");
            UdpClient listener = new UdpClient(listeningPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort);
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);

                try
                {

                    //Format: first 8 are for datetime, next 3 are ack, last 4 are message num
                    //indexes 0-14
                    //last 4 bytes are uint coding for message #
                    if (Encoding.ASCII.GetString(bytes, 8, 3).Equals("ack"))
                    {
                        DateTime send = DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
                        double latencyMilliseconds = (DateTime.Now - send).Milliseconds;
                        uint MessageNum = BitConverter.ToUInt32(bytes, bytes.Length-4);

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
}

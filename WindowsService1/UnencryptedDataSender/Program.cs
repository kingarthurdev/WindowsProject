using System.Net.Sockets;
using System.Net;
using dotNetClassLibrary;
using System.Threading;
using System;


namespace dotNetService
{
    public class Service
    {

        static bool usingProxy = true;
        static int proxyListeningForAckPort = 1544;
        public static void Main(string[] args)
        {
            //ProcessContent.WriteToFile("Service started at " + DateTime.Now);

            try
            {
                Thread listenThread = new Thread(new ThreadStart(listen));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                ProcessContent.WriteToFile(exception.ToString());
            }

        }


        public static void listen()
        {
            ProcessContent.WriteToFile("Listening on port 12000");
            Console.WriteLine("Are you using a proxy? yes/no");
            if (!Console.ReadLine().Equals("yes"))
            {
                usingProxy = false;
            }

            Console.WriteLine("Listening on port 12000, sending responses to port " + proxyListeningForAckPort);


            UdpClient listener = new UdpClient(12000);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);

            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(bytes);


                string total = num + "" + delim + message;

                //console log where the data came from in the format ipaddr:port
                ProcessContent.WriteToFile($"\nReceived broadcast from {groupEP}");
                ProcessContent.WriteToFile($"Recieved time: {DateTime.Now}, Sent time: {sendTime}, Latency: {(DateTime.Now - sendTime).Milliseconds}ms");
                ProcessContent.WriteToFile($" {total}\n");

                if (usingProxy)
                {
                    ProcessContent.sendACK(bytes, groupEP.Address.ToString(), proxyListeningForAckPort);
                }
                else
                {
                    ProcessContent.sendACK(bytes, groupEP.Address.ToString());
                }

            }

        }

    }
}

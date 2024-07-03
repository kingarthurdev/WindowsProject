using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using dotNetClassLibrary;
using System.Xml.Linq;
using EncryptionDecryption;
namespace TimedPinger
{
    internal class Pinger
    {
        static int listeningPort = 1543;
        static int destinationPort = 12000;
        static byte[] sendBytes;
        static uint count = 1;
        static bool RSAEstablished = false;

        static void Main(string[] args)
        {
            string privkey;
            string pubkey;
            (pubkey, privkey) = EncryptionDecryption.EncryptionDecryption.GenerateRSAKeys();

            try
            {
                Thread listener = new Thread(new ThreadStart(listenForACK));
                listener.Start();
                                
                Console.WriteLine("Enter the IP of where you would like to send a message:");

                //recipient address and port
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort);

                //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                while (!RSAEstablished)
                {
                    sendBytes = Encoding.ASCII.GetBytes(pubkey);
                    sock.SendTo(sendBytes, endpoint);
                    Console.WriteLine("Public Key Sent");
                    Thread.Sleep(1000);
                }


                while (true)
                {
                    sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");
                    sock.SendTo(sendBytes, endpoint);
                    Console.WriteLine("Data request sent");
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
            while (!RSAEstablished)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                if(Encoding.ASCII.GetString(bytes).Equals("Public Key Recieved"))
                {
                    RSAEstablished = true;
                }
            }
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);

                try
                {
                    //Format: first 8 are for datetime, next 3 are ack, 4 are message num, last however many are xml data
                    //indexes 0-14

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

using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using dotNetClassLibrary;
using System.Xml.Linq;
using EncryptionDecryptionLibrary;
namespace TimedPingerWithEncryption
{
    internal class Pinger
    {
        static int listeningPort = 1543;
        static int destinationPort = 12000;
        static byte[] sendBytes;
        static uint count = 1;
        static bool RSAEstablished = false;
        static bool AESEstablished = false;
        static byte[] AESKey; 
        static void Main(string[] args)
        {
            string privkey;
            string pubkey;
            (pubkey, privkey) = EncryptionDecryption.GenerateRSAKeys();

            try
            {
                Thread listener = new Thread(() => listenForACK(privkey));
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

                    byte[] AESIV = EncryptionDecryption.generateRandomAESIV(); 
                    sendBytes = EncryptionDecryption.AESEncrypt(sendBytes, AESKey, AESIV);

                    //first 16 bytes reserved to add the IV
                    byte[] final = new byte[sendBytes.Length + 16];
                    Buffer.BlockCopy(AESIV, 0, final, 0, AESIV.Length);
                    Buffer.BlockCopy(sendBytes, 0 , final, 16, sendBytes.Length);

                    sock.SendTo(final, endpoint);
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

        public static void listenForACK(string privkey)
        {
            Console.WriteLine($"Listening on port {listeningPort} for ack responses");
            UdpClient listener = new UdpClient(listeningPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort);

            //check for "public key received" in the beginning of message
            while (!RSAEstablished)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                byte[] ack = new byte[19]; // length of "public key received" 
                byte[] data = new byte[bytes.Length - 19];
                Buffer.BlockCopy(bytes, 0, ack, 0, 19);
                Buffer.BlockCopy(bytes, 19, data, 0, data.Length);

                try
                {
                    //if public key received by other party, write down the aes key sent in ack message.
                    if (Encoding.ASCII.GetString(ack).Equals("Public Key Received"))
                    {
                        Console.WriteLine("AES Response received!");
                        RSAEstablished = true;
                        AESKey = EncryptionDecryption.RSADecryptMessage(data, privkey);

                        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(groupEP.Address.ToString()), destinationPort);

                        sock.SendTo(Encoding.ASCII.GetBytes("AES Key Received"), endpoint);
                        sock.Close();
                    }
                }
                catch(Exception error)
                {
                    Console.WriteLine(error);
                }
                
            }
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                bytes = EncryptionDecryption.parseAndDecrypt(bytes, AESKey);

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


                        Console.WriteLine($"Response #{MessageNum} received from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");
                        ProcessContent.WriteToFile($"Response #{MessageNum} received from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");
                        try
                        {
                            Console.WriteLine(XElement.Parse(xmlString).ToString() + "\n\n\n");
                            ProcessContent.WriteToFile("Content:" + XElement.Parse(xmlString).ToString() + "\n\n\n");
                        }
                        catch
                        {
                            Console.WriteLine("Invalid XML Received");
                            ProcessContent.WriteToFile("Invalid XML Received");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Invalid data sent to ack port.");
                        Console.WriteLine("Here's what was recieved: " + Encoding.ASCII.GetString(bytes));
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

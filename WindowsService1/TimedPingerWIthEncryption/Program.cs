using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using dotNetClassLibrary;
using System.Xml.Linq;
using EncryptionDecryptionLibrary;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;

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
        static bool authenticated = false; 
        static byte[] AESKey;
        static string credentials;
        static IPEndPoint endpoint;
        static Dictionary<string, string> dllHashes = new Dictionary<string, string>
        {
            { "dotNetClassLibrary", "F3-13-83-AA-28-C3-99-99-71-40-0F-67-F0-E8-8E-86-A6-A2-0A-2D" }
        };
        static void Main(string[] args)
        {
            Console.Title = "Encrypted Data Requester";
            overallDllValidation();
            string privkey;
            string pubkey;
            (pubkey, privkey) = EncryptionDecryption.GenerateRSAKeys();

            try
            {
                Thread listener = new Thread(() => listenForACK(privkey));
                listener.Start();

                Console.WriteLine("Enter the IP of the machine from which you would like to request data:");

                //recipient address and port
                endpoint = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), destinationPort);

                //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


                while (!RSAEstablished)
                {
                    sendBytes = Encoding.ASCII.GetBytes(pubkey);
                    sock.SendTo(sendBytes, endpoint);
                    Console.WriteLine("Public Key Sent");
                    Thread.Sleep(1000);
                }

                while (!authenticated)
                {
                    Thread.Sleep(2000);
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

        static void getAndVerifyDlls()
        {
            for (int i = 0; i < dllHashes.Count; i++)
            {
                var assembly = Assembly.Load(dllHashes.ElementAt(i).Key);

                // Get the directory of the loaded assembly, load file from that location, then use a sha1 sum to check if the dll is known/valid
                string dllPath = assembly.Location;
                using (FileStream fop = File.OpenRead(dllPath))
                {
                    string chksum = BitConverter.ToString(System.Security.Cryptography.SHA1.Create().ComputeHash(fop));
                    if (!chksum.Equals(dllHashes.ElementAt(i).Value))
                    {
                        //Console.WriteLine(chksum); // uncomment this for later testing.
                        throw new Exception("Invalid File Signiture");
                    }
                }
            }
        }
        public static void overallDllValidation()
        {
            try
            {
                Assembly assembly;
                Type type;
                assembly = Assembly.LoadFrom("C:/Users/E1495970/OneDrive - Emerson/Desktop/Dll Demo/Restricted Access DLL Folder/dotNetClassLibrary.dll");
                type = assembly.GetType("dotNetClassLibrary.ProcessContent");
            }
            catch
            {
                try
                {
                    getAndVerifyDlls();
                    //loadExactLocation = false;
                }
                catch
                {
                    Console.WriteLine("Invalid File Signiture or missing dll");
                    string message = "Invalid File Signiture Detected!";
                    string title = "Critical Error";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show(message, title, buttons);
                    System.Environment.Exit(1);
                }
            }
        }
        public static void listenForACK(string privkey)
        {
            Console.WriteLine($"Listening on port {listeningPort} for ack responses");
            UdpClient listener = new UdpClient(listeningPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

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

                        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(groupEP.Address.ToString()), destinationPort);

                        sock.SendTo(Encoding.ASCII.GetBytes("AES Key Received"), endpoint);
                    }
                }
                catch(Exception error)
                {
                    Console.WriteLine(error);
                }
                
            }

            askCredentials(sock);
            while (!authenticated)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                bytes = EncryptionDecryption.parseAndDecrypt(bytes, AESKey);
                if(Encoding.ASCII.GetString(bytes).Equals("Login Succeeded!"))
                {
                    Console.WriteLine("Login Succeeded!");
                    authenticated = true;
                    break;
                }else if(Encoding.ASCII.GetString(bytes).Equals("Login Failed!"))
                {
                    Console.WriteLine("Login Failed. Please try again.");
                    askCredentials(sock);
                }
            }
            sock.Close();

            //listens for actual encrypted messages, recieve ack
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                bytes = EncryptionDecryption.parseAndDecrypt(bytes, AESKey);
                //bytes = new byte[] { 20, 28, 46, 72, 225, 175, 220, 8, 97, 99, 107, 3, 0, 0, 0, 60, 88, 77, 76, 68, 105, 115, 112, 108, 97, 121, 68, 97, 116, 97, 32, 78, 97, 109, 101, 61, 34, 77, 111, 99, 107, 68, 105, 115, 112, 108, 97, 121, 34, 32, 80, 97, 116, 104, 61, 34, 71, 114, 97, 112, 104, 105, 99, 115, 46, 68, 105, 115, 112, 108, 97, 121, 115, 34, 32, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 61, 34, 49, 46, 49, 46, 49, 46, 50, 34, 62, 13, 10, 32, 32, 60, 77, 101, 116, 97, 100, 97, 116, 97, 62, 13, 10, 32, 32, 32, 32, 60, 76, 97, 115, 116, 69, 100, 105, 116, 85, 115, 101, 114, 62, 68, 86, 65, 68, 77, 73, 78, 60, 47, 76, 97, 115, 116, 69, 100, 105, 116, 85, 115, 101, 114, 62, 13, 10, 32, 32, 32, 32, 60, 76, 97, 115, 116, 69, 100, 105, 116, 84, 105, 109, 101, 62, 50, 48, 50, 52, 45, 48, 55, 45, 50, 57, 84, 49, 53, 58, 49, 53, 58, 50, 57, 46, 51, 50, 56, 50, 52, 50, 57, 45, 48, 53, 58, 48, 48, 60, 47, 76, 97, 115, 116, 69, 100, 105, 116, 84, 105, 109, 101, 62, 13, 10, 32, 32, 60, 47, 77, 101, 116, 97, 100, 97, 116, 97, 62, 13, 10, 32, 32, 60, 80, 111, 108, 121, 103, 111, 110, 32, 78, 97, 109, 101, 61, 34, 67, 111, 110, 116, 114, 111, 108, 80, 111, 108, 121, 103, 111, 110, 34, 62, 13, 10, 32, 32, 32, 32, 60, 72, 101, 105, 103, 104, 116, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 51, 50, 48, 56, 57, 54, 56, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 72, 101, 105, 103, 104, 116, 62, 13, 10, 32, 32, 32, 32, 60, 87, 105, 100, 116, 104, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 51, 50, 57, 49, 54, 50, 53, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 87, 105, 100, 116, 104, 62, 13, 10, 32, 32, 32, 32, 60, 88, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 52, 49, 49, 49, 54, 55, 55, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 88, 62, 13, 10, 32, 32, 32, 32, 60, 89, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 49, 54, 53, 53, 51, 57, 54, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 89, 62, 13, 10, 32, 32, 60, 47, 80, 111, 108, 121, 103, 111, 110, 62, 13, 10, 32, 32, 60, 79, 112, 101, 114, 97, 116, 105, 110, 103, 80, 101, 114, 99, 101, 110, 116, 97, 103, 101, 62, 56, 57, 60, 47, 79, 112, 101, 114, 97, 116, 105, 110, 103, 80, 101, 114, 99, 101, 110, 116, 97, 103, 101, 62, 13, 10, 32, 32, 60, 85, 110, 114, 101, 115, 111, 108, 118, 101, 100, 73, 115, 115, 117, 101, 115, 62, 49, 48, 60, 47, 85, 110, 114, 101, 115, 111, 108, 118, 101, 100, 73, 115, 115, 117, 101, 115, 62, 13, 10, 60, 47, 88, 77, 76, 68, 105, 115, 112, 108, 97, 121, 68, 97, 116, 97, 62 };
                

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

        static void askCredentials(Socket sock)
        {
            Console.WriteLine("Enter in your connection credentials in the format username:password");
            credentials = Console.ReadLine();
            sendBytes = Encoding.ASCII.GetBytes(credentials);
            byte[] AESIV = EncryptionDecryption.generateRandomAESIV();
            sendBytes = EncryptionDecryption.AESEncrypt(sendBytes, AESKey, AESIV);

            //first 16 bytes reserved to add the IV
            byte[] final = new byte[sendBytes.Length + 16];
            Buffer.BlockCopy(AESIV, 0, final, 0, AESIV.Length);
            Buffer.BlockCopy(sendBytes, 0, final, 16, sendBytes.Length);

            sock.SendTo(final, endpoint);

        }
    }
}

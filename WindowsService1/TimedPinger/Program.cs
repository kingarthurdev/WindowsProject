using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using dotNetClassLibrary;

namespace TimedPinger
{
    internal class Pinger
    {
        static int listeningPort = 1543;
        static int destinationPort = 12000;
        static byte[] sendBytes;
        static uint count = 1;
        static bool loadExactLocation = true;

        public static Assembly assembly;
        public static Type type;

        public static void WriteToFile(String s)
        {
            if (!loadExactLocation)
            {
                ProcessContent.WriteToFile(s);
            }
            else
            {
                var ProcessContent = Activator.CreateInstance(type);
                var method = type.GetMethod("WriteToFile");
                method.Invoke(ProcessContent, new object[] { s });
            }
            
        }
        public static byte[] convertToTimestampedBytes(uint a, char c, String s)
        {
            if (!loadExactLocation)
            {
                return ProcessContent.convertToTimestampedBytes(a, c, s);
            }
            else
            {
                var ProcessContent = Activator.CreateInstance(type);
                var method = type.GetMethod("convertToTimestampedBytes");
                return method.Invoke(ProcessContent, new object[] { a, c, s }) as byte[];
            }
        }

        static Dictionary<string,string> dllHashes = new Dictionary<string, string>
        {
            { "dotNetClassLibrary", "F3-13-83-AA-28-C3-99-99-71-40-0F-67-F0-E8-8E-86-A6-A2-0A-2D" }
        };
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

        static void Main(string[] args)
        {
            Console.Title = "Unencrypted Data Requester";

            try
            {
                assembly = Assembly.LoadFrom("C:/Users/E1495970/OneDrive - Emerson/Desktop/Dll Demo/Restricted Access DLL Folder/dotNetClassLibrary.dll");
                type = assembly.GetType("dotNetClassLibrary.ProcessContent");
            }
            catch
            {
                try
                {
                    getAndVerifyDlls();
                    loadExactLocation = false;
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
           
            try
            {
                         
                Console.WriteLine("Enter the IP of the computer from which to request data:");
                IPAddress ip = IPAddress.Parse(Console.ReadLine());

                Console.WriteLine("Enter the port of where you would like to send to (default = 12000, 12001 if you would like to use a proxy):");
                destinationPort = Int32.Parse(Console.ReadLine());

                //recipient address and port
                IPEndPoint endpoint = new IPEndPoint(ip, destinationPort);

                //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                Thread listener = new Thread(new ThreadStart(listenForACK));
                listener.Start();

                while (true)
                {
                    sendBytes = convertToTimestampedBytes(count, ';', "Request For Data");
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
                        WriteToFile($"Response #{MessageNum} recieved from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");
                        try
                        {
                            Console.WriteLine(XElement.Parse(xmlString).ToString() + "/n/n/n");
                            WriteToFile("Content:" + XElement.Parse(xmlString).ToString() + "/n/n/n");
                        }
                        catch
                        {
                            Console.WriteLine("Invalid XML Recieved");
                            WriteToFile("Invalid XML Recieved");
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

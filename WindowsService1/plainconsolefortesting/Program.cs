using System;
using System.Net.Sockets;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using dotNetClassLibrary;
using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Text;


namespace dotNetService
{
    public class Service
    {
        

        public static void Main(string[] args)
        {
            
            ProcessContent.WriteToFile("Service started at " + DateTime.Now);

            try
            {
                Thread listenThread = new Thread(new ThreadStart(listen));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                ProcessContent.WriteToFile(exception.ToString());
            }

            /*
            string data = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><note> <to>Tove</to>\r\n  <from>Jani</from>\r\n  <heading>Reminder</heading>\r\n  <body>Don't forget me this weekend!</body>\r\n</note>";

            if (data.Substring(0, 14).Equals("<?xml version="))
            {
                try
                {
                    var a = XElement.Parse(data).ToString();
                    Console.WriteLine(a);
                }
                catch (System.Xml.XmlException)
                {
                    Console.WriteLine("Invalid XML!");
                }
            }
            else
            {
                Console.WriteLine("Nope, not xml");
            }
            
            */



        }


        public static void listen()
        {
            ProcessContent.WriteToFile("Listening on port 12000");
            UdpClient listener = new UdpClient(12000);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);
            while (true)
            {

                byte[] bytes = listener.Receive(ref groupEP);
                //string representation = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(bytes);


                string total = num + "" + delim + message;

                //console log where the data came from in the format ipaddr:port
                ProcessContent.WriteToFile($"\nReceived broadcast from {groupEP}");
                ProcessContent.WriteToFile($"Recieved time: {DateTime.Now}, Sent time: {sendTime}, Latency: {(DateTime.Now - sendTime).Milliseconds}ms");
                ProcessContent.WriteToFile($" {total}\n");
                ProcessContent.sendACK(bytes, groupEP.Address.ToString());

            }

        }


    }
}



using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using dotNetClassLibrary;
using System.Windows;


// ProcessContent.convertToByteArray(1, ';', "asdf");   //This works well

/*
string filename = "C:\\Users\\E1495970\\Downloads\\examplenote.xml";
FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

byte[] ImageData = new byte[fs.Length];
fs.Read(ImageData, 0, (int) fs.Length);

//Close the File Stream
fs.Close();


Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

IPAddress broadcast = IPAddress.Parse("127.0.0.1");

IPEndPoint endpoint = new IPEndPoint(broadcast, 12000);
sock.SendTo(ImageData, endpoint);
*/

/*
byte[] a = ProcessContent.convertToByteArray(1, ':', "abcdefg");
(uint num, char delim, string message) = ProcessContent.convertFromByteArray(a);

Console.WriteLine(num+""+delim+message);

string message2 = "Simple MessageBox";
MessageBox.Show(message2);*/


/*public class test
{
    public static void Main(string[] args)
    {
        try
        {
            Thread listenThread = new Thread(new ThreadStart(listen));
            listenThread.Start();
        }
        catch (Exception exception)
        {
            WriteToFile(exception.ToString(), ".txt");
        }
    }

    static void listen()
    {
        UdpClient listener = new UdpClient(12000);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);
        while (true)
        {

            byte[] bytes = listener.Receive(ref groupEP);
            //string representation = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            (uint num, char delim, string message) = ProcessContent.convertFromByteArray(bytes);


            string total = num + "" + delim + message;








            //console log where the data came from in the format ipaddr:port
            WriteToFile($"Received broadcast from {groupEP} :");

            
            if (total.Substring(0, 6).Equals("<?xml "))
            {
                WriteToFile($" {total}", ".xml");
            }
            else
            {
                WriteToFile($" {total}");

            }
            WriteToFile($" {total}");

        }

    }
    static void WriteToFile(string message, string extension)
    {
        string path = AppDomain.CurrentDomain.BaseDirectory + "\\logs";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\servicelog" + extension;
        if (!File.Exists(filepath))
        {
            using (StreamWriter sw = File.CreateText(filepath)) { sw.WriteLine(message); }
        }
        else
        {
            using (StreamWriter sw = File.AppendText(filepath)) { sw.WriteLine(message); }
        }

    }
    static void WriteToFile(string message)
    {
        WriteToFile(message, ".txt");

    }
}

*/

DateTime dt = DateTime.Now;
byte[] arrProp = BitConverter.GetBytes(dt.Ticks);

Console.WriteLine(dt.ToString());

Console.WriteLine(arrProp.Length);
Console.WriteLine(DateTime.FromBinary(BitConverter.ToInt64(arrProp, 0)).Year);

string xml = "joe mamma";
byte[] xmlArr = Encoding.ASCII.GetBytes(xml);

Console.WriteLine(Encoding.ASCII.GetString(xmlArr));

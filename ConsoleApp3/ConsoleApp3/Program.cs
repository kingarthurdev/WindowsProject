using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
class Program
{
    static void Main(string[] args)
    {
        //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        Console.WriteLine("Enter in the ipaddr with which you would like to communicate");
        string ipaddr = Console.ReadLine();


        //recipient address and port
        IPAddress broadcast = IPAddress.Parse(ipaddr);
        IPEndPoint endpoint = new IPEndPoint(broadcast, 12000);

        //buffer array to send 
        byte[] sendBytes;

        uint num;
        char delim;
        string message;


        while (true)
        {
            try
            {
                Console.WriteLine("Enter in the number of your message (uint)");
                num = UInt32.Parse(Console.ReadLine());
                Console.WriteLine();

                Console.WriteLine("Enter in your delimeter of choice (char)");
                delim = Char.Parse(Console.ReadLine());
                Console.WriteLine();

                Console.WriteLine("Enter in your message (string)");
                message = Console.ReadLine();

                sendBytes = ProcessContent.convertToByteArray(num, delim, message);
                sock.SendTo(sendBytes, endpoint);
                Console.WriteLine("Message sent to the broadcast address");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }



        }

    }
}

public static class ProcessContent
{

    public static byte[] convertToByteArray(uint num, char delim, string xml)
    {
        byte[] xmlArr = new byte[xml.Length];
        xmlArr = Encoding.ASCII.GetBytes(xml);
        byte[] charArr = BitConverter.GetBytes(delim);

        //uint bigEndianNum = (uint)IPAddress.HostToNetworkOrder((int)num);   // this is just here to make sure that everyone is talking in the same way -- avoid issues later
        byte[] uintArr = BitConverter.GetBytes(num);

        byte[] finalArr = new byte[uintArr.Length + charArr.Length + xmlArr.Length];

        Buffer.BlockCopy(uintArr, 0, finalArr, 0, uintArr.Length);
        Buffer.BlockCopy(charArr, 0, finalArr, uintArr.Length, charArr.Length);
        Buffer.BlockCopy(xmlArr, 0, finalArr, uintArr.Length + charArr.Length, xmlArr.Length);

        return finalArr;
    }



    public static (uint, char, string) convertFromByteArray(byte[] input)
    {
        uint convertedUint = BitConverter.ToUInt32(input, 0);

        char convertedChar = BitConverter.ToChar(input, sizeof(uint));

        string convertedString = Encoding.ASCII.GetString(input, 5, input.Length - 5);

        return (convertedUint, convertedChar, convertedString);
    }

}
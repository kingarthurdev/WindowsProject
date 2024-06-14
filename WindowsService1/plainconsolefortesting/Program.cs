

using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using dotNetClassLibrary;
using System.Windows;
using System.Text.RegularExpressions;



try
{
    Thread listenThread = new Thread(new ThreadStart(listen));
    listenThread.Start();
}
catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}



static void listen()
{
    Console.WriteLine("Listening...");
    UdpClient listener = new UdpClient(12000);
    IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);
    while (true)
    {

        byte[] bytes = listener.Receive(ref groupEP);
        //string representation = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(bytes);


        string total = num + "" + delim + message;

        //console log where the data came from in the format ipaddr:port
        Console.WriteLine($"\nReceived broadcast from {groupEP}");
        Console.WriteLine($"Recieved time: {DateTime.Now}, Sent time: {sendTime}, Latency: {(DateTime.Now - sendTime).Milliseconds}ms");
        Console.WriteLine($" {total}\n");
        Console.WriteLine($"Sending ack to {groupEP.Address.ToString()}");
        ProcessContent.sendACK(bytes, groupEP.Address.ToString());

    }

}
/*
while(true)
{
    DateTime dt = DateTime.Now;
    byte[] bytes = BitConverter.GetBytes(dt.Ticks);
    ProcessContent.sendACK(bytes, "192.168.0.180");
    Thread.Sleep(1000);
}*/



using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using dotNetClassLibrary;
using System.Windows;


try
{
    Thread listenThread = new Thread(new ThreadStart(listen));
    listenThread.Start();
}
catch (Exception exception)
{
    ProcessContent.WriteToFile(exception.ToString());
}



static void listen()
{
    ProcessContent.WriteToFile("Listening...");
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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using dotNetClassLibrary;
class Program
{
    public static void Main(string[] args)
    {
        //ip addr, port
        var results = setup(12000);

        //send the packets based on user input from the console, starts listener for ack responses
        send(results.Item1, results.Item2);

    }

    public static (Socket, IPEndPoint) setup(int port)
    {
        Console.WriteLine("Enter the IP of where you would like to send a message:");
        //recipient address and port
        IPAddress broadcast = IPAddress.Parse(Console.ReadLine());
        IPEndPoint endpoint = new IPEndPoint(broadcast, port);

        //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        Thread listener = new Thread(new ThreadStart(listenForACK));
        listener.Start();

        return (sock, endpoint);
    }

    public static void send(Socket sock, IPEndPoint endpoint)
    {
        //buffer array to send 
        byte[] sendBytes;

        uint num;
        char delim;
        string message;


        while (true)
        {
            try
            {
                Console.WriteLine("\nEnter in the number of your message (uint)");
                num = UInt32.Parse(Console.ReadLine());
                Console.WriteLine();

                Console.WriteLine("Enter in your delimeter of choice (char)");
                delim = Char.Parse(Console.ReadLine());
                Console.WriteLine();

                Console.WriteLine("Enter in your message (string)");
                message = Console.ReadLine();

                sendBytes = ProcessContent.convertToTimestampedBytes(num, delim, message);
                sock.SendTo(sendBytes, endpoint);
                Console.WriteLine("_________________________________________");
                Console.WriteLine("\nMessage sent to the broadcast address");
                Console.WriteLine("_________________________________________\n");

                //Log the user inputs into the program
                ProcessContent.WriteToFile($"Timestamp: {DateTime.Now} User input: {num} was input as the uint, {delim} was input as the character delimeter, {message} was input as the string message.");
                
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
    public static void listenForACK()
    {
        Console.WriteLine("Listening on port 1543 for ack responses");
        UdpClient listener = new UdpClient(1543);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 1543);
        while (true)
        {
            Console.WriteLine("entered while loop");
            byte[] bytes = listener.Receive(ref groupEP);
            Console.WriteLine("Things recieved.");
            try
            {
                //last 4 bytes are uint coding for message #
                if (Encoding.ASCII.GetString(bytes, 8, bytes.Length - 8).Equals("ack"))
                {
                    DateTime send = DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
                    double latencyMilliseconds = (DateTime.Now - send).Milliseconds;
                    uint MessageNum = BitConverter.ToUInt32(bytes, bytes.Length - 4);

                    Console.WriteLine($"ACK #{MessageNum} recieved from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");
                    ProcessContent.WriteToFile($"ACK #{MessageNum} recieved from {groupEP.Address.ToString()}, " + latencyMilliseconds + "ms of latency");

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
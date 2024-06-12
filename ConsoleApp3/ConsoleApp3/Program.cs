using System;
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

        //send the packets based on user input from the console
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

                sendBytes = ProcessContent.convertToByteArray(num, delim, message);
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
}
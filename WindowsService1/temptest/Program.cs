using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
class Program
{
    public static void Main(string[] args)
    {
        // a malicious pass would be this: "; update users set passwordThatShouldHaveBeenHashed = "Thisissuchasecurepassword1234" where id = 1-- 


        //ip addr, port
        var results = setup();

        //send the packets based on user input from the console, starts listener for ack responses
        send(results.Item1, results.Item2);

    }

    public static (Socket, IPEndPoint) setup()
    {
        Console.WriteLine("Enter the IP of where you would like execute a command after authentication:");

        //recipient address and port
        IPAddress location = IPAddress.Parse(Console.ReadLine());

        Console.WriteLine("Enter the listening port:");
        IPEndPoint endpoint = new IPEndPoint(location, int.Parse(Console.ReadLine()));

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
                Console.WriteLine("Enter a message in this format -->   Username:password:command");
                message = Console.ReadLine();

                sock.SendTo(Encoding.ASCII.GetBytes(message), endpoint);
                Console.WriteLine("\n_________________________________________");
                Console.WriteLine("\nMessage sent to the broadcast address");
                Console.WriteLine("_________________________________________\n");



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
  
}
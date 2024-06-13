using System;
using System.Net.Sockets;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using dotNetClassLibrary;


namespace dotNetService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            ProcessContent.WriteToFile("Service started at " + DateTime.Now, AppDomain.CurrentDomain.BaseDirectory + "\\logs");

            try
            {
                Thread listenThread = new Thread(new ThreadStart(listen));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                ProcessContent.WriteToFile(exception.ToString(), AppDomain.CurrentDomain.BaseDirectory + "\\logs");
            }


        }

        protected override void OnStop()
        {
            ProcessContent.WriteToFile("Service stopped at " + DateTime.Now, AppDomain.CurrentDomain.BaseDirectory + "\\logs");
        }

        public static void listen()
        {
            UdpClient listener = new UdpClient(12000);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);
            while (true)
            {

                byte[] bytes = listener.Receive(ref groupEP);
                //string representation = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(bytes);


                string total = num+""+delim+message;

                //console log where the data came from in the format ipaddr:port
                ProcessContent.WriteToFile($"\nReceived broadcast from {groupEP}", AppDomain.CurrentDomain.BaseDirectory + "\\logs");
                ProcessContent.WriteToFile($"Recieved time: {DateTime.Now}, Sent time: {sendTime}, Latency: {(DateTime.Now - sendTime).Milliseconds}ms", AppDomain.CurrentDomain.BaseDirectory + "\\logs");
                ProcessContent.WriteToFile($" {total}\n", AppDomain.CurrentDomain.BaseDirectory + "\\logs");
                ProcessContent.sendACK(bytes, groupEP.Address.ToString());

            }
            
        }

        
    }
}

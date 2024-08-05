using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using dotNetClassLibrary;
/*using ConsoleControl;
using ConsoleControl.WPF;
using ConsoleControlAPI;
*/
namespace Frontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int listeningPort = 1543;
        static int destinationPort = 12000;
        static byte[] sendBytes;
        static uint count = 1;


        public Process process = new Process();
        
        public MainWindow()
        {
            InitializeComponent();
            /*process.StartInfo.FileName = "cmd.exe";
            //process.StartInfo.Arguments = "/c \"C:/Users/E1495970/source/repos/WindowsService1/TimedPinger/bin/Debug/TimedPinger.exe\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = false;
            richTextBoxConsole.AppendText("Program started.");
            richTextBoxConsole.TextChanged += forwardCommand;*/
        }
        /*
        public void forwardCommand(object sender, RoutedEventArgs e)
        {
            process.StandardInput.Write("ping 1.1.1.1 \n");
            Thread.Sleep(1000);
            process.StandardInput.Write("ping 1.1.1.2 \n");

        }*/
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Console.WriteLine("Enter the IP of the computer from which to request data:");
                IPAddress ip = IPAddress.Parse(Console.ReadLine());

                Console.WriteLine("Enter the port of where you would like to send to (default = 12000, 12001 if you would like to use a proxy):");
                destinationPort = Int32.Parse(Console.ReadLine());

                //recipient address and port
                IPEndPoint endpoint = new IPEndPoint(ip, destinationPort);

                //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                Thread listener = new Thread(new ThreadStart(listenForACK));
                listener.Start();

                while (true)
                {
                    sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");
                    sock.SendTo(sendBytes, endpoint);
                    Console.WriteLine("Data request sent");
                    count++;
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            try
            {

                //label.Content = "You clicked the button";

                /*
                process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {

                    richTextBoxConsole.AppendText(e.Data);

                });
                process.OutputDataReceived += (s, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Dispatcher.Invoke(() => richTextBoxConsole.AppendText(e.Data + Environment.NewLine));
                    }
                };

                process.Start();

                // Asynchronously read the standard output of the spawned process.
                // This raises OutputDataReceived events for each line of output.
                process.BeginOutputReadLine();
                //process.WaitForExit();

                //richTextBoxConsole.AppendText(s);
                */
                /*ConsoleControl.WPF.ConsoleControl a = new ConsoleControl.WPF.ConsoleControl();
                a.StartProcess("cmd.exe", "");*/
            }
            catch (Exception ex)
            {

            }


        }
        /*private void btn5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                label.Content = "You clicked the button";

                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c ping 1.1.1.1";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                
                process.OutputDataReceived += (s, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Dispatcher.Invoke(() => richTextBoxConsole.AppendText(e.Data + Environment.NewLine));
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }*/

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "You clickyed the buttyon";
            ProcessStartInfo start2 = new ProcessStartInfo("cmd.exe", "/c powershell -Command \"Start-Process cmd -Verb RunAs\"");
            Process.Start(start2);

        }
        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "You clickyed the buttyon";
            ProcessStartInfo start2 = new ProcessStartInfo("cmd.exe", "/c powershell -Command \"Start-Process cmd -Verb RunAs\"");
            Process.Start(start2);

        }
        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "You clickyed the buttyon";
            ProcessStartInfo start2 = new ProcessStartInfo("cmd.exe", "/c powershell -Command \"Start-Process cmd -Verb RunAs\"");
            Process.Start(start2);

        }


    }
}
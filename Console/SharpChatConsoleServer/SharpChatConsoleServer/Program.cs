using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpChatConsoleServer
{
    class Server
    {
        static HashSet<handleClient> clientsList = new HashSet<handleClient>();

        static TcpListener serverSocket { get; set; }

        static void Main(string[] args)
        {
            Console.Write("Enter IP to Create Server : ");
            String serverIPString = "10.8.101.4";// Console.ReadLine();

            Console.Write("\nEnter Port to Create Server : ");
            String serverPortString = "6969";// Console.ReadLine();
            
            IPAddress serverIP = IPAddress.Parse(serverIPString);
            serverSocket = new TcpListener(serverIP, Int32.Parse(serverPortString));
            serverSocket.Start();

            Console.WriteLine("\nServer Started at IP : {0}, Port : {1}", serverIPString, serverPortString);

            new Thread(getClientConnection).Start();
            new Thread(ServerChat).Start();
        }

        static void getClientConnection()
        {
            while (true)
            {
                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                handleClient newClient = new handleClient(clientSocket);
                clientsList.Add(newClient);
            }
        }

        static void ServerChat()
        {
            while (true)
            {   
                string outString = Console.ReadLine();

                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine(" > Me : " + outString);

                byte[] outStream = Encoding.ASCII.GetBytes(" > Server : " + outString + "$");

                foreach (var client in clientsList)
                {
                    client.FromServerChat(outStream);
                }
            }
        }

        public static void broadcastInputStream(byte[] broadcastStream,handleClient fromClient)
        {
            NetworkStream networkStream = null;
            foreach (var otherClients in clientsList)
            {
                if (otherClients == fromClient)
                    continue;
                networkStream = otherClients.clientSocket.GetStream();
                networkStream.Write(broadcastStream, 0, broadcastStream.Length);
            }
        }

        public static void broadcastInputString(String broadcastString, handleClient fromClient)
        {
            byte[] broadcastStream = Encoding.ASCII.GetBytes(broadcastString);
            NetworkStream networkStream = null;
            foreach (var otherClients in clientsList)
            {
                if (otherClients == fromClient)
                    continue;
                networkStream = otherClients.clientSocket.GetStream();
                networkStream.Write(broadcastStream, 0, broadcastStream.Length);
            }
        }
    }

    class handleClient
    {
        public TcpClient clientSocket { get; set; }
        public String MachineName { get; set; }
        public handleClient(TcpClient clientSocketTemp)
        {
            this.clientSocket = clientSocketTemp;

            NetworkStream networkStream = clientSocket.GetStream();
            
            byte[] clientNameByte = new byte[1000];
            networkStream.Read(clientNameByte, 0, clientNameByte.Length);
            this.MachineName = Encoding.ASCII.GetString(clientNameByte);
            this.MachineName = this.MachineName.Substring(0, this.MachineName.IndexOf('$'));
            Console.WriteLine("--- " + this.MachineName + " Connected ---");

            Server.broadcastInputString("--- " + this.MachineName + " Connected ---$", this);

            new Thread(FromClientChat).Start();
        }

        private void FromClientChat()
        {
            while (true)
            {
                byte[] inStream = new byte[10025];
                string inString = null;

                NetworkStream networkStream = this.clientSocket.GetStream();
                networkStream.Read(inStream, 0, inStream.Length);

                inString = Encoding.ASCII.GetString(inStream);
                inString = inString.Substring(0, inString.IndexOf('$'));

                Console.WriteLine(inString);

                Server.broadcastInputStream(inStream, this);
            }
        }

        public void FromServerChat(byte[] outStream)
        {
            NetworkStream networkStream = this.clientSocket.GetStream();
            networkStream.Write(outStream, 0, outStream.Length);
        }
    }
}

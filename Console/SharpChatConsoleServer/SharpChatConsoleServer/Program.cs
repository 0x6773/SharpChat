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

        static TcpListener serverSocket = null;

        static void Main(string[] args)
        {
            Console.Write("Enter IP to Create Server : ");
            String serverIPString = Console.ReadLine();

            Console.Write("Enter Port to Create Server : ");
            String serverPortString = Console.ReadLine();
            
            IPAddress serverIP = IPAddress.Parse(serverIPString);
            serverSocket = new TcpListener(serverIP, Int32.Parse(serverPortString));
            serverSocket.Start();

            Console.WriteLine("Server Started at IP : {0}, Port : {1}", serverIPString, serverPortString);

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
                byte[] outStream = Encoding.ASCII.GetBytes(" > " + outString + "$");

                foreach (var client in clientsList)
                {
                    client.FromServerChat(outStream);
                }
            }
        }
    }

    class handleClient
    {
        private TcpClient clientSocket = null;
        public handleClient(TcpClient clientSocketTemp)
        {
            this.clientSocket = clientSocketTemp;
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
            }
        }

        public void FromServerChat(byte[] outStream)
        {
            NetworkStream networkStream = this.clientSocket.GetStream();
            networkStream.Write(outStream, 0, outStream.Length);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpChatConsoleServer
{
    class Program
    {

        static NetworkStream networkStream = null;
        static void Main(string[] args)
        {
            Console.Write("Enter IP to Create Server : ");
            String serverIPString = Console.ReadLine();

            Console.Write("Enter Port to Create Server : ");
            String serverPortString = Console.ReadLine();
            
            IPAddress serverIP = IPAddress.Parse(serverIPString);
            TcpListener serverSocket = new TcpListener(serverIP, Int32.Parse(serverPortString));

            serverSocket.Start();
            Console.WriteLine("Server Started at IP : {0}, Port : {1}", serverIPString, serverPortString);

            TcpClient clientSocket = serverSocket.AcceptTcpClient();

            networkStream = clientSocket.GetStream();

            Console.WriteLine("Client1 Connected!");

            Thread t1 = new Thread(ServerChat);
            t1.Start();

            Thread t2 = new Thread(ClientChat);
            t2.Start();
            

        }

        static void ServerChat()
        {
            while(true)
            {
                byte[] inStream = new byte[10025];
                string inString = null;

                networkStream.Read(inStream, 0, inStream.Length);

                inString = Encoding.ASCII.GetString(inStream);
                inString = inString.Substring(0, inString.IndexOf('$'));

                Console.WriteLine(" > Client : " + inString);
            }
        }

        static void ClientChat()
        {
            while (true)
            {
                
                string outString = Console.ReadLine();
                byte[] outStream = Encoding.ASCII.GetBytes(outString + "$");

                networkStream.Write(outStream, 0, outStream.Length);
            }
        }
    }
}

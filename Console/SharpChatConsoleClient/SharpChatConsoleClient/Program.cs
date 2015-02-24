using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpChatConsoleClient
{
    class Program
    {
        static NetworkStream networkStream = null;
        static void Main(string[] args)
        {
            Console.Write("Enter IP to Connect to Server : ");
            String serverIPString = Console.ReadLine();

            Console.Write("Enter Port to Connect to Server : ");
            String serverPortString = Console.ReadLine();
            
            TcpClient clientSocket = new TcpClient();
            clientSocket.Connect(serverIPString, Int32.Parse(serverPortString));
            Console.WriteLine("Connected to Server!");

            networkStream = clientSocket.GetStream();

            Thread t1 = new Thread(ServerChat);
            t1.Start();

            Thread t2 = new Thread(ClientChat);
            t2.Start();
        }

        static void ClientChat()
        {
            while (true)
            {
                string outString = Console.ReadLine();
                byte[] outStream = Encoding.ASCII.GetBytes(" > " + outString + "$");
                Console.WriteLine(outString);

                networkStream.Write(outStream, 0, outStream.Length);
            }
        }

        static void ServerChat()
        {
            while (true)
            {
                byte[] inStream = new byte[10025];
                string inString = null;

                networkStream.Read(inStream, 0, inStream.Length);

                inString = Encoding.ASCII.GetString(inStream);
                inString = inString.Substring(0, inString.IndexOf('$'));

                Console.WriteLine(" > Server : " + inString);
            }
        }
    }
}

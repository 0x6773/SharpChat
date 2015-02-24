using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SharpChatConsoleClient
{
	class Program
	{
		static NetworkStream networkStream = null;
		static void Main(string[] args)
		{
			Console.Write("Enter IP to Connect to Server : ");
			String serverIPString = "10.8.101.4";// Console.ReadLine();
			
			Console.Write("\nEnter Port to Connect to Server : ");
			String serverPortString = "6969";//Console.ReadLine();
			
			TcpClient clientSocket = new TcpClient();
			clientSocket.Connect(serverIPString, Int32.Parse(serverPortString));
			Console.WriteLine("\nConnected to Server!");
			networkStream = clientSocket.GetStream();
			
			String MachineName = Environment.MachineName;
			
			byte[] MNStream = Encoding.ASCII.GetBytes(MachineName + "$");
			networkStream.Write(MNStream, 0, MNStream.Length);
			
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
				
				Console.SetCursorPosition(0, Console.CursorTop - 1);
				Console.WriteLine(" > Me : "+outString);
				
				outString = " > " + Environment.MachineName + " : " + outString;
				byte[] outStream = Encoding.ASCII.GetBytes(outString + "$");
				
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
				
				Console.WriteLine(inString);
			}
		}
	}
}

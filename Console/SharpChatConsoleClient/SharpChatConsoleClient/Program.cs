﻿/*
 *  SharpChat Console for Client
 *  https://github.com/mafiya69/SharpChat.git
 * 
 * Copyright (c) 2015 Govind Sahai (mafiya69)
 * Licensed under the MIT license.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SharpChatConsoleClient
{
    class Client
    {
        static NetworkStream networkStream = null;

        enum STATUS
        {
            CONNECTED,
            DISCONNECTED
        }

        static STATUS status { get; set; }
        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient();
            

            Console.Write("Enter IP to Connect to Server : ");
            String serverIPString = "10.8.101.4"; //Console.ReadLine();

            Console.Write("\nEnter Port to Connect to Server : ");
            String serverPortString = "6969"; //Console.ReadLine();

            try
            {
                clientSocket.Connect(serverIPString, Int32.Parse(serverPortString));
                status = STATUS.CONNECTED;
            }
            catch (SocketException)
            {
                Console.WriteLine("\nError Occurred in Connecting to Server!\n" +
                    "Try Changing IP Address or Port or both.\n" +
                    "Press Any Key to exit...\n");
                Console.ReadKey();
                status = STATUS.DISCONNECTED;
                Environment.Exit(0);
            }
            catch (Exception)
            {
                Console.WriteLine("\nEither IP or Port or both are not in correct Format." +
                    "Press Any Key to exit...\n");
                status = STATUS.DISCONNECTED;
                Console.ReadKey();
                Environment.Exit(0);
            }
            try
            {
                Console.WriteLine("\nConnected to Server!");
                networkStream = clientSocket.GetStream();
            }
            catch(Exception)
            {
                Console.WriteLine("Seems like Server Has Been closed!"+
                    "Press Any Key to exit...\n");
                Console.ReadKey();
                status = STATUS.DISCONNECTED;
                Environment.Exit(0);
            }

            String MachineName = Environment.MachineName;
            try
            {
                byte[] MNStream = Encoding.ASCII.GetBytes(MachineName + "$");
                networkStream.Write(MNStream, 0, MNStream.Length);

                new Thread(ServerChat).Start();
                new Thread(ClientChat).Start();
            }
            catch(Exception)
            {
                Console.WriteLine("Error Occurred While Making Chat Room For " + MachineName +
                    "\nPress Any Key to exit...\n");
                Console.ReadKey();
                status = STATUS.DISCONNECTED;
                Environment.Exit(0);
            }
        }

        static void ClientChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED)
                {
                    Console.WriteLine("Connection has been closed forcibly\n" +
                    "\nPress Any Key to exit...\n");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                Byte[] outStream = null;
                try
                {
                    String outString = Console.ReadLine();
                    if (outString.Length == 0)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        continue;
                    }
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine(" > Me : " + outString);

                    outString = " > " + Environment.MachineName + " : " + outString;
                    outStream = Encoding.ASCII.GetBytes(outString + "$");
                }
                catch(Exception)
                {
                    Console.WriteLine("Error while processing data\n" +
                        "If you ofter seeing this Message, Please Restart");
                    continue;
                }
                try
                {
                    networkStream.Write(outStream, 0, outStream.Length);
                }
                catch (Exception)
                {
                    status = STATUS.DISCONNECTED;
                }
            }
        }

        static void ServerChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED) 
                {
                    Console.WriteLine("Connection has been closed forcibly\n" +
                    "\nPress Any Key to exit...\n");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                byte[] inStream = new byte[10025];
                string inString = null;

                try
                {
                    networkStream.Read(inStream, 0, inStream.Length);

                    inString = Encoding.ASCII.GetString(inStream);
                    inString = inString.Substring(0, inString.IndexOf('$'));
                }
                catch (Exception)
                {
                    status = STATUS.DISCONNECTED;
                }
                Console.WriteLine(inString);
            }
        }
    }
}

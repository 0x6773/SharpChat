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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SharpChatConsoleClient
{
    class Client
    {
        //  TcpClient of client
        static TcpClient clientSocket { get; set; }

        //  Status of client/Server Connection
        static STATUS status { get; set; }

        //  enum for STATUS
        enum STATUS
        {
            CONNECTED,
            DISCONNECTED
        }
        
        //  Entry Point
        static void Main(string[] args)
        {            
            Console.Write("Enter IP to Connect to Server : ");
            String serverIPString = "127.0.0.1";// Console.ReadLine();
            serverIPString = serverIPString.Trim();

            Console.Write("\nEnter Port to Connect to Server : ");
            String serverPortString = "6969";// Console.ReadLine();
            serverPortString = serverPortString.Trim();

            try
            {
                clientSocket = new TcpClient();
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
                var networkStream = clientSocket.GetStream();
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

        //  ClientChat
        static void ClientChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED)
                {
                    return;
                }
                Byte[] outStream = null;
                try
                {
                    String outString = Console.ReadLine();
                    if (status == STATUS.DISCONNECTED)
                        return;
                    outString = outString.Trim();
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
                    var networkStream = clientSocket.GetStream();
                    networkStream.Write(outStream, 0, outStream.Length);
                }
                catch (Exception)
                {
                    status = STATUS.DISCONNECTED;
                }
            }
        }

        //  ServerChat
        static void ServerChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED) 
                {
                    Console.WriteLine("Connection has been closed forcibly\n" +
                    "\nPress Any Key to exit...\n");
                    return;
                }
                byte[] inStream = new byte[10025];
                string inString = null;

                try
                {
                    var networkStream = clientSocket.GetStream();
                    networkStream.Read(inStream, 0, inStream.Length);

                    inString = Encoding.ASCII.GetString(inStream);
                    inString = inString.Substring(0, inString.IndexOf('$'));
                }
                catch(IOException)
                {
                    status = STATUS.DISCONNECTED;
                    continue;
                }
                catch (Exception)
                {
                    status = STATUS.DISCONNECTED;
                    continue;
                }
                try
                {
                    inString = inString.Trim();
                    if (inString == "%%STOP%%")
                        throw new Exception();
                }
                catch(Exception)
                {
                    status = STATUS.DISCONNECTED;
                    continue;
                }
                Console.WriteLine(inString);
            }
        }
    }
}

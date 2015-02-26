﻿/*
 *  SharpChat Console for Server
 *  https://github.com/mafiya69/SharpChat.git
 * 
 * Copyright (c) 2015 Govind Sahai (mafiya69)
 * Licensed under the MIT license.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SharpChatConsoleServer
{    
    public static class staticClass
    {
        private static bool toBeSent { get; set; }
        private static void kickClient(string p)
        {
            if (Server.kickIfPossible(p))
                toBeSent = false;
            else
                toBeSent = true;
        }
        public static void checkCommand(String command)
        {
            var tempList = command.Split(' ');
            try
            {
                if (tempList[0].ToUpper() == "KICK")
                    kickClient(tempList[1].ToUpper());
            }
            catch(Exception)
            {
                toBeSent = true;
            }
        }

        public static String getCommand()
        {
            try
            {
                toBeSent = true;
                String cmd = Console.ReadLine();
                if (cmd[0] == '/')
                {
                    toBeSent = false;
                    staticClass.checkCommand(cmd.Substring(1));
                }
                if (toBeSent)
                    return cmd;
                else
                    return null;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }

    //  Class Server
    class Server
    {
        //  HashSet<> Storing Data of All Clients
        static List<handleClient> clientsList = new List<handleClient>();

        //  TcpLister for Server
        static TcpListener serverSocket { get; set; }

        //  Main Method
        static void Main(string[] args)
        {
            //  Get IP
            Console.Write("Enter IP to Create Server : ");
            String serverIPString = "10.3.31.253";//Console.ReadLine();

            //  Get Port
            Console.Write("\nEnter Port to Create Server : ");
            String serverPortString = "6969"; //Console.ReadLine();

            try
            {
                IPAddress serverIP = IPAddress.Parse(serverIPString);
                serverSocket = new TcpListener(serverIP, Int32.Parse(serverPortString));
            }
            catch(Exception)
            {
                Console.WriteLine("\nEither IP or Port or both are not in correct Format."+
                    "Press Any Key to exit...\n");
                Console.ReadKey();
                Environment.Exit(0);
            }
            try
            {
                //  Starting TcpLister
                serverSocket.Start();
            }
            catch(SocketException)
            {
                Console.WriteLine("\nError Occurred in Creating Server!\n"+
                    "Try Changing IP Address or Port or both.\n"+
                    "Press Any Key to exit...\n");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine("\nServer Started at IP : {0}, Port : {1}", serverIPString, serverPortString);
            try
            {
                //  Creating threads for get Connection From Client And Server Chatting
                new Thread(getClientConnection).Start();
                new Thread(ServerChat).Start();
            }
            catch (ThreadStateException)
            {
                Console.WriteLine("\nThe Thread Has already been started. Cannot Start Again.");
            }
            catch(OutOfMemoryException)
            {
                Console.WriteLine("\nOutOfMemoryException Thrown.\n"+
                    "There is not enough memory available to start this Process.\n"+
                    "Press Any Key To exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        //  Starting Chat Room For Client
        static void getClientConnection()
        {
            while (true)
            {
                try
                {
                    TcpClient clientSocket = serverSocket.AcceptTcpClient();
                    handleClient newClient = new handleClient(clientSocket);
                    clientsList.Add(newClient);
                }
                catch(Exception)
                {
                    Console.WriteLine("\nUnknown Error Occurred\n"+
                        "If you ofter seeing this Message, Please Restart Server");
                }
            }
        }

        //  Deleting Client Data from ClientList
        public static void deleteClientConnection(handleClient toDeleteClient)
        {
            try
            {
                clientsList.Remove(toDeleteClient);
            }
            catch(Exception)
            {
                
            }
        }

        //  Kick the Client and return if kicked
        public static bool kickIfPossible(String clientName)
        {
            try 
            {
                var tempHL = clientsList.Find(s => s.MachineName == clientName);
                deleteClientConnection(tempHL);
                tempHL.status = handleClient.STATUS.KICKED;
                tempHL.clientSocket.Close();
                Console.WriteLine("--- " + tempHL.MachineName + " KICKED ---");
                Server.broadcastInputString("--- " + tempHL.MachineName + " KICKED ---$", tempHL);
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        //  Server Chat : Method for Server Chatting
        static void ServerChat()
        {
            while (true)
            {
                try
                {
                    String outString = staticClass.getCommand();
                    if (outString == null)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        continue;
                    }
                    var size = outString.Length;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine(" > Me : " + outString);

                    byte[] outStream = Encoding.ASCII.GetBytes(" > Server : " + outString + "$");

                    foreach (var client in clientsList)
                    {
                        client.FromServerChat(outStream);
                    }
                }
                catch(IOException)
                {
                    Console.WriteLine("\nError Reading/Writing to Console.");
                }
                /*catch(Exception)
                {
                    Console.WriteLine("\nUnknown Error Occurred\n" +
                        "If you ofter seeing this Message, Please Restart Server");
                }*/
            }
        }

        //  Broadcasting Input from one Client to All other Clients
        public static void broadcastInputStream(byte[] broadcastStream,handleClient fromClient)
        {
            try
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
            catch(Exception)
            {
                Console.WriteLine("\nError Occured While BroadCasting the Message of " + fromClient.MachineName);
            }
        }

        //  Broadcasting Input from one Client to All other Clients
        public static void broadcastInputString(String broadcastString, handleClient fromClient)
        {
            try
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
            catch (Exception)
            {
                Console.WriteLine("\nError Occured While BroadCasting the Message of " + fromClient.MachineName);
            }
        }
    }
}
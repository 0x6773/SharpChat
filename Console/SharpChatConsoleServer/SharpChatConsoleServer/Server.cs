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
    //  StaticClass for handling Commands
    public static class staticClass
    {
        //  String input in getCommand() toBeSent or not
        private static bool toBeSent { get; set; }

        //  Method for /kick Command
        private static void kickClient(string p)
        {
            try
            {
                if (Server.kickIfPossible(p))
                    toBeSent = false;
                else
                    toBeSent = true;
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
            }
        }

        //  ShutdownServer
        private static void shutdown()
        {
            try
            {
                Server.broadcastInputString("The Server is going to shut down in$");
                for (int i = 10; i > 0; --i)
                {
                    Server.broadcastInputString(i.ToString() + " sec...$");
                    Console.WriteLine(i.ToString() + " sec...");
                    Thread.Sleep(990);
                }
                Server.broadcastInputString("Server ShutDown...$");
                Console.WriteLine("Server ShutDown...");
                Environment.Exit(0);
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
            }
        }

        //  Method to show Help
        private static void viewHelp()
        {
            try
            {
                Console.WriteLine("Help for SharpChat Server\n\n" +
                    "Type \"/kick <CLIENTNAME>\" to kick the client\n" +
                    "Type \"/help\" to get this help \n" +
                    "Type \"/shutdown\" to shutdown chatServer \n" +
                    "Type \"/showall\" to show names of all connected clients \n\n" +
                    "More Commands coming soon.\n\n");
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
            }
        }

        //  Check if any command is present or a simple string
        public static void checkCommand(String command)
        {
            var tempList = command.Split(' ');
            try
            {
                if (tempList[0].ToUpper() == "KICK")
                    kickClient(tempList[1].ToUpper());
                else if (tempList[0].ToUpper() == "HELP")
                    viewHelp();
                else if (tempList[0].ToUpper() == "SHUTDOWN")
                    shutdown();
                else if (tempList[0].ToUpper() == "SHOWALL")
                    showall();
                else
                    toBeSent = true;
            }
            catch(Exception)
            {
                toBeSent = true;
            }
        }

        //  Show all connected Clients
        private static void showall()
        {
            try
            {
                StringBuilder toShow = new StringBuilder("\n--- Connected Clients ---\n\n");
                foreach (var clients in Server.clientsList)
                {
                    toShow.AppendFormat("{0}\n", clients.MachineName);
                }
                toShow.AppendFormat("\nTotal Number of clients Connected : {0}\n-----------------------------------\n", Server.clientsList.Count);
                Console.WriteLine(toShow.ToString());
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
            }
        }

        //  Method to get input from User
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
        public static List<Client> clientsList = new List<Client>();

        //  TcpLister for Server
        static TcpListener serverSocket { get; set; }

        //  Main Method
        static void Main(string[] args)
        {
            //  Get IP
            Console.Write("Enter IP to Create Server : ");
            String serverIPString = "10.8.101.4";// Console.ReadLine();
            serverIPString = serverIPString.Trim();

            //  Get Port
            Console.Write("\nEnter Port to Create Server : ");
            String serverPortString = "44";//Console.ReadLine();
            serverPortString = serverPortString.Trim();

            try
            {
                IPAddress serverIP = IPAddress.Parse(serverIPString);
                serverSocket = new TcpListener(serverIP, Int32.Parse(serverPortString));
            }
            catch(Exception)
            {
                Console.WriteLine("\nEither IP or Port or both are not in correct Format." +
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
                Console.WriteLine("\nError Occurred in Creating Server!\n" +
                    "Try Changing IP Address or Port or both.\n" +
                    "Press Any Key to exit...\n");
                Console.ReadKey();
                Environment.Exit(0);
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
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
                Console.WriteLine("\nOutOfMemoryException Thrown.\n" +
                    "There is not enough memory available to start this Process.\n" +
                    "Press Any Key To exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
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
                    Client newClient = new Client(clientSocket);
                    clientsList.Add(newClient);
                }
                catch(Exception)
                {
                    Console.WriteLine("\nUnknown Error Occurred\n" +
                        "If you often seeing this Message, Please Restart Server");
                }
            }
        }

        //  Deleting Client Data from ClientList
        public static void deleteClientConnection(Client toDeleteClient)
        {
            try
            {
                clientsList.Remove(toDeleteClient);
                toDeleteClient.clientSocket.GetStream().Close();
                toDeleteClient.clientSocket.Close();                
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
            }
        }

        //  Kick the Client and return if kicked
        public static bool kickIfPossible(String clientName)
        {
            try 
            {
                var tempHL = clientsList.Find(s => s.MachineName == clientName);
                deleteClientConnection(tempHL);
                tempHL.status = Client.STATUS.KICKED;
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
                    outString = outString.Trim();
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
                catch(Exception)
                {
                    Console.WriteLine("\nUnknown Error Occurred\n" +
                        "If you ofter seeing this Message, Please Restart Server");
                }
            }
        }

        //  Broadcasting Input from one Client to All other Clients
        public static void broadcastInputStream(byte[] broadcastStream,Client fromClient = null)
        {
            try
            {
                foreach (var otherClients in clientsList)
                {
                    if (otherClients == fromClient)
                        continue;
                    var networkStream = otherClients.clientSocket.GetStream();
                    networkStream.Write(broadcastStream, 0, broadcastStream.Length);
                }
            }
            catch(Exception)
            {
                Console.WriteLine("\nError Occured While BroadCasting the Message of " + fromClient.MachineName);
            }
        }

        //  Broadcasting Input from one Client to All other Clients
        public static void broadcastInputString(String broadcastString, Client fromClient = null)
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
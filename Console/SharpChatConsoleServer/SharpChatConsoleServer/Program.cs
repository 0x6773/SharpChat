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
    class Server
    {
        static HashSet<handleClient> clientsList = new HashSet<handleClient>();

        static TcpListener serverSocket { get; set; }

        static void Main(string[] args)
        {
            Console.Write("Enter IP to Create Server : ");
            String serverIPString = Console.ReadLine();

            Console.Write("\nEnter Port to Create Server : ");
            String serverPortString = Console.ReadLine();

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

        static void ServerChat()
        {
            while (true)
            {
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

    class handleClient
    {
        public TcpClient clientSocket { get; set; }
        public String MachineName { get; set; }

        private enum STATUS
        {
            CONNECTED,
            DISCONNECTED
        }
        private STATUS status { get; set; }
        public handleClient(TcpClient clientSocketTemp)
        {
            try
            {
                this.clientSocket = clientSocketTemp;

                NetworkStream networkStream = clientSocket.GetStream();

                byte[] clientNameByte = new byte[1000];
                networkStream.Read(clientNameByte, 0, clientNameByte.Length);
                this.MachineName = Encoding.ASCII.GetString(clientNameByte);
                this.MachineName = this.MachineName.Substring(0, this.MachineName.IndexOf('$'));
                this.status = STATUS.CONNECTED;
            }
            catch(Exception)
            {
                Console.WriteLine("--- " + this.MachineName + " Tried to Connect ---");
                Server.broadcastInputString("--- " + this.MachineName + " Tried to Connect ---$", this);
                        
                this.status = STATUS.DISCONNECTED;
            }
            try
            {
                if (status == STATUS.CONNECTED)
                {
                    Console.WriteLine("--- " + this.MachineName + " Connected ---");
                    Server.broadcastInputString("--- " + this.MachineName + " Connected ---$", this);
                }
            }
            catch(Exception)
            {
                Console.WriteLine("\nUnknown Error Occured But You Can continue.");
            }
            try
            {
                new Thread(FromClientChat).Start();
            }
            catch(Exception)
            {
                Console.WriteLine("--- " + this.MachineName + " DISConnected ---");
                Server.broadcastInputString("--- " + this.MachineName + " DISConnected ---$", this);
                this.status = STATUS.DISCONNECTED;
            }
            if (status == STATUS.DISCONNECTED)
                Server.deleteClientConnection(this);
        }

        private void FromClientChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED) 
                {
                    Server.deleteClientConnection(this);
                    break;
                }
                byte[] inStream = new byte[10025];
                string inString = null;
                try
                {
                    NetworkStream networkStream = this.clientSocket.GetStream();
                    networkStream.Read(inStream, 0, inStream.Length);

                    inString = Encoding.ASCII.GetString(inStream);
                    inString = inString.Substring(0, inString.IndexOf('$'));
                    Console.WriteLine(inString);
                    Server.broadcastInputStream(inStream, this);
                }
                catch (Exception)
                {
                    Console.WriteLine("--- " + this.MachineName + " DISConnected ---");
                    Server.broadcastInputString("--- " + this.MachineName + " DISConnected ---$", this);
                    this.status = STATUS.DISCONNECTED;
                }
            }
        }

        public void FromServerChat(byte[] outStream)
        {
            if (status == STATUS.DISCONNECTED)
                Server.deleteClientConnection(this);
            try
            {
                NetworkStream networkStream = this.clientSocket.GetStream();
                networkStream.Write(outStream, 0, outStream.Length);
            }
            catch(Exception)
            {
                //  Unknown Error
            }
        }
    }
}
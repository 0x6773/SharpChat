﻿/*
 *  SharpChat UI for Client
 *  https://github.com/mafiya69/SharpChat.git
 * 
 * Copyright (c) 2015 Govind Sahai (mafiya69)
 * Licensed under the MIT license.
 * 
 */

using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace SharpChatUIClientVS
{
    public partial class MainWindow : Window
    {
        TcpClient clientSocket = new TcpClient();

        NetworkStream networkStream = null;

        Thread sc = null;

        private STATUS status { get; set; }
        private enum STATUS
        {
            CONNECTED,
            DISCONNECTED
        }

        public MainWindow()
        {
            InitializeComponent();
            currentWin.Title = Environment.MachineName + " Chat on " + Environment.OSVersion.ToString();
            ConnectionData.serverIPString = "127.0.0.1";
            ConnectionData.serverPortString = "6969";
        }

        private void connectToServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clientSocket.Close();
                clientSocket = new TcpClient();
                clientSocket.Connect(ConnectionData.serverIPString.Trim(), Int32.Parse(ConnectionData.serverPortString.Trim()));
                status = STATUS.CONNECTED;
            }
            catch(FormatException)
            {
                MessageBox.Show("Seems like IP or Port are not in Correct Format!", "Attention");
                return;
            }
            catch(SocketException)
            {
                MessageBox.Show("No connection to the host has been made!\nCheck You LAN.", "Attention");
                return;
            }
            catch(Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message); 
                MessageBox.Show(error, "Attention");
                return;
            }
            changeAllConnectionItemsTo(false);
            changeAllChatItemsTo(true);
            try
            {
                networkStream = clientSocket.GetStream();
            }
            catch(Exception)
            {
                MessageBox.Show("Connection to the Host has been Lost!","Error");
                changeAllConnectionItemsTo(true);
                changeAllChatItemsTo(false);
            }
            String MachineName = Environment.MachineName;
            try
            {
                byte[] MNStream = Encoding.ASCII.GetBytes(MachineName + "$");
                networkStream.Write(MNStream, 0, MNStream.Length);                

                sc = new Thread(ServerChat);
                sc.Name = "ServerChatThread";
                sc.Start();
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                MessageBox.Show(error, "Attention");
                changeAllConnectionItemsTo(true);
                changeAllChatItemsTo(false);
                status = STATUS.DISCONNECTED;
            }
            chatWindow.Text = "";
        }

        private void sendChat_Click(object sender, RoutedEventArgs e)
        {
            if (status == STATUS.DISCONNECTED) 
            {
                chatWindow.Text += chatWindow.Text + "\nDisconnected From Server.";
                MessageBox.Show("Connection has been closed. Message is Not Sent\nPlease Connect Again", "Connection Ended");
                changeAllConnectionItemsTo(true);
                changeAllChatItemsTo(false);
                return;
            }
            Byte[] outStream = null;
            String outString = chatInputWindow.Text;

            try
            {
                if (outString.Length == 0)
                    return;
                outStream = Encoding.ASCII.GetBytes(" > " + Environment.MachineName + " : " + outString + "$");
            }
            catch (Exception)
            {
                MessageBox.Show("Error while processing data\nIf you ofter seeing this Message, Please ReConnect", "Attention");
                return;
            }
            try
            {
                networkStream.Write(outStream, 0, outStream.Length);
                if (!clientSocket.Connected || status == STATUS.DISCONNECTED) 
                    throw new Exception();
                chatWindow.Text += "\n > Me : " + outString;
                chatWindow.ScrollToEnd();
            }
            catch (Exception)
            {
                clientSocket.Close();
                status = STATUS.DISCONNECTED;
                return;
            }
            chatInputWindow.Text = "";
        }

        private void ServerChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED) 
                {
                    MessageBox.Show("Connection has been closed or Server is down.\nPlease Connect Again after Some Time", "Error");
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        changeAllConnectionItemsTo(true);
                        changeAllChatItemsTo(false);
                    }));                    
                    Thread.CurrentThread.Abort();
                    return;
                }
                byte[] inStream = new byte[10025];
                string inString = null;
                try
                {
                    //networkStream.ReadTimeout = 100;
                    networkStream.Read(inStream, 0, inStream.Length);

                    inString = Encoding.ASCII.GetString(inStream);
                    inString = inString.Substring(0, inString.IndexOf('$'));
                }
                catch (ArgumentException)
                {
                    //  Do Nothing
                    continue;
                }
                catch (IOException)
                {
                    //  Do Nothing
                    continue;
                }
                catch (Exception)
                {
                    clientSocket.Close();
                    continue;
                }
                inString = inString.Trim();
                try
                {
                    if(inString=="%%STOP%%")
                    {
                        status = STATUS.DISCONNECTED;
                        continue;
                    }
                    
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        chatWindow.Text += "\n" + inString;
                        chatWindow.ScrollToEnd();
                    }));
                }
                catch (Exception)
                {
                    //  Main thread exited
                }
            }
        }

        private void changeAllChatItemsTo(bool toChange)
        {
            try
            {
                sendChat.IsEnabled = toChange;
                chatInputWindow.IsReadOnly = !toChange;
                sendChat.IsDefault = true;
                enterToSendCheckBox.IsEnabled = toChange;
                if (toChange)
                    statusText.Text = "STATUS : CONNECTED TO " + ConnectionData.serverIPString + ":" + ConnectionData.serverPortString;
                else
                    statusText.Text = "STATUS : DISCONNECTED";
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                MessageBox.Show(error, "Attention");
                return;
            }
        }

        private void changeAllConnectionItemsTo(bool toChange)
        {
            try
            {
                connectToServer.IsEnabled = toChange;
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                MessageBox.Show(error, "Attention");
                return;
            }
        }

        private void enterToSendCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                chatInputWindow.AcceptsReturn = false;
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                MessageBox.Show(error, "Attention");
                return;
            }
        }

        private void enterToSendCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                chatInputWindow.AcceptsReturn = true;
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                MessageBox.Show(error, "Attention");
                return;
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Hide();
                String about = "Made by : Govind Sahai\nGH : https://github.com/mafiya69/SharpChat.git \nStudent at Indian Institute of Technology\nBHU, Varanasi";
                MessageBox.Show(about, "About");
                this.Show();
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                MessageBox.Show(error, "Attention");
                return;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSettings cs = new ConnectionSettings();
            cs.Show();
        }
    }
}
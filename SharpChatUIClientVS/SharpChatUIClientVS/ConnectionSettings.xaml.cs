﻿/*
 *  SharpChat UI for Client
 *  https://github.com/mafiya69/SharpChat.git
 * 
 * Copyright (c) 2015 Govind Sahai (mafiya69)
 * Licensed under the MIT license.
 * 
 */

using System;
using System.Windows;

namespace SharpChatUIClientVS
{
    /// <summary>
    /// Interaction logic for ConnectionSettings.xaml
    /// </summary>
    public partial class ConnectionSettings : Window
    {
        public ConnectionSettings()
        {
            InitializeComponent();
            serverPort.Text = ConnectionData.serverPortString;
            serverIP.Text = ConnectionData.serverIPString;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectionData.serverIPString = serverIP.Text;
                ConnectionData.serverPortString = serverPort.Text;
                this.Close();
            }
            catch(Exception)
            {

            }
        }
    }
}

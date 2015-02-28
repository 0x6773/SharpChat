using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpChatConsoleServer
{
    //  Class to handle Clients
    class Client
    {
        //  TcpClient of User
        public TcpClient clientSocket { get; set; }

        //  MachineName of User
        public String MachineName { get; set; }

        //  status of Client
        public STATUS status { get; set; }

        //  enum of status
        public enum STATUS
        {
            CONNECTED,
            DISCONNECTED,
            KICKED
        }      

        //  Constructor
        public Client(TcpClient clientSocketTemp)
        {
            try
            {
                this.clientSocket = clientSocketTemp;
                var clientNameByte = new byte[1000];

                var networkStream = clientSocket.GetStream();
                networkStream.Read(clientNameByte, 0, clientNameByte.Length);
                
                this.MachineName = Encoding.ASCII.GetString(clientNameByte);
                this.MachineName = this.MachineName.Substring(0, this.MachineName.IndexOf('$'));
                this.MachineName = this.MachineName.Trim();
                this.status = STATUS.CONNECTED;
            }
            catch (Exception)
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
            catch (Exception)
            {
                Console.WriteLine("\nUnknown Error Occured But You Can continue.");
            }
            try
            {
                new Thread(FromClientChat).Start();
            }
            catch (Exception)
            {
                Console.WriteLine("--- " + this.MachineName + " DISConnected ---");
                Server.broadcastInputString("--- " + this.MachineName + " DISConnected ---$", this);
                this.status = STATUS.DISCONNECTED;
            }
            if (status == STATUS.DISCONNECTED)
                Server.deleteClientConnection(this);
        }

        //  Method for Recieving Data from Client
        private void FromClientChat()
        {
            while (true)
            {
                if (status == STATUS.DISCONNECTED)
                {
                    Server.deleteClientConnection(this);
                    break;
                }
                else if (status == STATUS.KICKED)
                {
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                    break;
                }
                var inStream = new byte[10025];
                string inString = null;
                try
                {
                    var networkStream = this.clientSocket.GetStream();
                    networkStream.Read(inStream, 0, inStream.Length);
                    inString = Encoding.ASCII.GetString(inStream);
                    inString = inString.Substring(0, inString.IndexOf('$'));
                    Console.WriteLine(inString);
                    Server.broadcastInputStream(inStream, this);
                }
                catch (Exception)
                {
                    if (status == STATUS.CONNECTED)
                    {
                        Console.WriteLine("--- " + this.MachineName + " DISConnected ---");
                        Server.broadcastInputString("--- " + this.MachineName + " DISConnected ---$", this);
                        this.status = STATUS.DISCONNECTED;
                    }
                }
            }
        }

        //  Send Message to Client from Server
        public void FromServerChat(byte[] outStream)
        {
            if (status == STATUS.DISCONNECTED)
                Server.deleteClientConnection(this);
            try
            {
                var networkStream = this.clientSocket.GetStream();
                networkStream.Write(outStream, 0, outStream.Length);
            }
            catch (Exception err)
            {
                String error = String.Format("Unknown Exception of Type : {0}", err.Message);
                Console.WriteLine(error);
            }
        }
    }
}

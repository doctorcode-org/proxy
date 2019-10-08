using DoctorProxy;
using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer.Ftp
{
    public sealed class FtpClient : Client
    {
        public FtpClient(Socket ClientSocket, DestroyDelegate Destroyer) : base(ClientSocket, Destroyer) { }

        public override void StartHandshake()
        {
            try
            {
                string ToSend = "220 Mentalis.org FTP proxy server ready.\r\n";
                ClientSocket.BeginSend(Encoding.ASCII.GetBytes(ToSend), 0, ToSend.Length, SocketFlags.None, new AsyncCallback(this.OnHelloSent), ClientSocket);
            }
            catch
            {
                Dispose();
            }
        }

        private void OnHelloSent(IAsyncResult ar)
        {
            try
            {
                if (ClientSocket.EndSend(ar) <= 0)
                {
                    Dispose();
                    return;
                }
                ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveCommand), ClientSocket);
            }
            catch
            {
                Dispose();
            }
        }

        private void OnReceiveCommand(IAsyncResult ar)
        {
            try
            {
                int Ret = ClientSocket.EndReceive(ar);
                string Command;
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                FtpCommand += Encoding.ASCII.GetString(Buffer, 0, Ret);
                if (FtpClient.IsValidCommand(FtpCommand))
                {
                    Command = FtpCommand;
                    if (ProcessCommand(Command))
                        DestinationSocket.BeginSend(Encoding.ASCII.GetBytes(Command), 0, Command.Length, SocketFlags.None, new AsyncCallback(this.OnCommandSent), DestinationSocket);
                    FtpCommand = "";
                }
                else
                {
                    ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveCommand), ClientSocket);
                }
            }
            catch
            {
                Dispose();
            }
        }

        private bool ProcessCommand(string Command)
        {
            try
            {
                int Ret = Command.IndexOf(' ');
                if (Ret < 0)
                    Ret = Command.Length;
                switch (Command.Substring(0, Ret).ToUpper().Trim())
                {
                    case "OPEN":
                        ConnectTo(ParseIPPort(Command.Substring(Ret + 1)));
                        break;
                    case "USER":
                        Ret = Command.IndexOf('@');
                        if (Ret < 0)
                        {
                            return true;
                        }
                        else
                        {
                            User = Command.Substring(0, Ret).Trim() + "\r\n";
                            ConnectTo(ParseIPPort(Command.Substring(Ret + 1)));
                        }
                        break;
                    case "PORT":
                        ProcessPortCommand(Command.Substring(5).Trim());
                        break;
                    case "PASV":
                        DataForward = new FtpDataConnection();
                        DataForward.ProcessPasv(this);
                        return true;
                    default:
                        return true;
                }
                return false;
            }
            catch
            {
                Dispose();
                return false;
            }
        }

        private void ProcessPortCommand(string Input)
        {
            try
            {
                string[] Parts = Input.Split(',');
                if (Parts.Length == 6)
                {
                    DataForward = new FtpDataConnection();
                    string Reply = DataForward.ProcessPort(new IPEndPoint(IPAddress.Parse(String.Join(".", Parts, 0, 4)), int.Parse(Parts[4]) * 256 + int.Parse(Parts[5])));
                    DestinationSocket.BeginSend(Encoding.ASCII.GetBytes(Reply), 0, Reply.Length, SocketFlags.None, new AsyncCallback(this.OnCommandSent), DestinationSocket);
                }
            }
            catch
            {
                Dispose();
            }
        }

        private IPEndPoint ParseIPPort(string Input)
        {
            Input = Input.Trim();
            int Ret = Input.IndexOf(':');
            if (Ret < 0)
                Ret = Input.IndexOf(' ');
            try
            {
                if (Ret > 0)
                {
                    return new IPEndPoint(Dns.GetHostEntry(Input.Substring(0, Ret)).AddressList[0], int.Parse(Input.Substring(Ret + 1)));
                }
                else
                {
                    return new IPEndPoint(Dns.GetHostEntry(Input).AddressList[0], 21);
                }
            }
            catch
            {
                return null;
            }
        }

        private void ConnectTo(IPEndPoint RemoteServer)
        {
            if (DestinationSocket != null)
            {
                try
                {
                    DestinationSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                finally
                {
                    DestinationSocket.Close();
                }
            }
            try
            {
                DestinationSocket = new Socket(RemoteServer.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                DestinationSocket.BeginConnect(RemoteServer, new AsyncCallback(this.OnRemoteConnected), DestinationSocket);
            }
            catch
            {
                throw new SocketException();
            }
        }

        private void OnRemoteConnected(IAsyncResult ar)
        {
            try
            {
                DestinationSocket.EndConnect(ar);
                ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveCommand), ClientSocket);
                if (User.Equals(""))
                    DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnReplyReceived), DestinationSocket);
                else
                    DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnIgnoreReply), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }

        private void OnIgnoreReply(IAsyncResult ar)
        {
            try
            {
                int Ret = DestinationSocket.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                FtpReply += Encoding.ASCII.GetString(RemoteBuffer, 0, Ret);
                if (FtpClient.IsValidReply(FtpReply))
                {
                    DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnReplyReceived), DestinationSocket);
                    DestinationSocket.BeginSend(Encoding.ASCII.GetBytes(User), 0, User.Length, SocketFlags.None, new AsyncCallback(this.OnCommandSent), DestinationSocket);
                }
                else
                {
                    DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnIgnoreReply), DestinationSocket);
                }
            }
            catch
            {
                Dispose();
            }
        }

        private void OnCommandSent(IAsyncResult ar)
        {
            try
            {
                if (DestinationSocket.EndSend(ar) <= 0)
                {
                    Dispose();
                    return;
                }
                ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveCommand), ClientSocket);
            }
            catch
            {
                Dispose();
            }
        }

        private void OnReplyReceived(IAsyncResult ar)
        {
            try
            {
                int Ret = DestinationSocket.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                if (DataForward != null && DataForward.ExpectsReply)
                {
                    if (!DataForward.ProcessPasvReplyRecv(Encoding.ASCII.GetString(RemoteBuffer, 0, Ret)))
                        DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnReplyReceived), DestinationSocket);
                }
                else
                {
                    ClientSocket.BeginSend(RemoteBuffer, 0, Ret, SocketFlags.None, new AsyncCallback(this.OnReplySent), ClientSocket);
                }
            }
            catch
            {
                Dispose();
            }
        }

        private void OnReplySent(IAsyncResult ar)
        {
            try
            {
                int Ret = ClientSocket.EndSend(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnReplyReceived), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }

        internal void SendCommand(string Command)
        {
            ClientSocket.BeginSend(Encoding.ASCII.GetBytes(Command), 0, Command.Length, SocketFlags.None, new AsyncCallback(this.OnReplySent), ClientSocket);
        }

        internal static bool IsValidCommand(string Command)
        {
            return (Command.IndexOf("\r\n") >= 0);
        }

        internal static bool IsValidReply(string Input)
        {
            string[] Lines = Input.Split('\n');
            try
            {
                if (Lines[Lines.Length - 2].Trim().Substring(3, 1).Equals(" "))
                    return true;
            }
            catch { }
            return false;
        }

        private string FtpCommand
        {
            get
            {
                return m_FtpCommand;
            }
            set
            {
                m_FtpCommand = value;
            }
        }

        private string FtpReply
        {
            get
            {
                return m_FtpReply;
            }
            set
            {
                m_FtpReply = value;
            }
        }

        private string User
        {
            get
            {
                return m_User;
            }
            set
            {
                m_User = value;
            }
        }

        internal FtpDataConnection DataForward
        {
            get
            {
                return m_DataForward;
            }
            set
            {
                m_DataForward = value;
            }
        }

        public override string ToString()
        {
            try
            {
                return "FTP connection from " + ((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString() + " to " + ((IPEndPoint)DestinationSocket.RemoteEndPoint).Address.ToString();
            }
            catch
            {
                return "Incoming FTP connection";
            }
        }

        private string m_User = "";
        private string m_FtpCommand = "";
        private string m_FtpReply = "";
        private FtpDataConnection m_DataForward;


    }
}

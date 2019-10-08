using DoctorProxy;
using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;

namespace ProxyServer.Ftp
{
    internal sealed class FtpDataConnection : Client
    {
        public FtpDataConnection() : base() { }

        public string ProcessPort(IPEndPoint RemoteAddress)
        {
            try
            {
                ListenSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                ListenSocket.Listen(1);
                ListenSocket.BeginAccept(new AsyncCallback(this.OnPortAccept), ListenSocket);
                ClientSocket = new Socket(RemoteAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ClientSocket.BeginConnect(RemoteAddress, new AsyncCallback(this.OnPortConnected), ClientSocket);
                return "PORT " + Listener.GetLocalExternalIP().ToString().Replace('.', ',') + "," + Math.Floor(((IPEndPoint)ListenSocket.LocalEndPoint).Port / 256d).ToString() + "," + (((IPEndPoint)ListenSocket.LocalEndPoint).Port % 256).ToString() + "\r\n";
            }
            catch
            {
                Dispose();
                return "PORT 0,0,0,0,0,0\r\n";
            }
        }

        private void OnPortConnected(IAsyncResult ar)
        {
            try
            {
                ClientSocket.EndConnect(ar);
                StartHandshake();
            }
            catch
            {
                Dispose();
            }
        }

        private void OnPortAccept(IAsyncResult ar)
        {
            try
            {
                DestinationSocket = ListenSocket.EndAccept(ar);
                ListenSocket.Close();
                ListenSocket = null;
                StartHandshake();
            }
            catch
            {
                Dispose();
            }
        }

        public override void StartHandshake()
        {
            if (DestinationSocket != null && ClientSocket != null && DestinationSocket.Connected && ClientSocket.Connected)
                StartRelay();
        }

        private Socket ListenSocket
        {
            get
            {
                return m_ListenSocket;
            }
            set
            {
                if (m_ListenSocket != null)
                    m_ListenSocket.Close();
                m_ListenSocket = value;
            }
        }

        private FtpClient Parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                m_Parent = value;
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

        internal bool ExpectsReply
        {
            get
            {
                return m_ExpectsReply;
            }
            set
            {
                m_ExpectsReply = value;
            }
        }

        public void ProcessPasv(FtpClient Parent)
        {
            this.Parent = Parent;
            ExpectsReply = true;
        }

        internal bool ProcessPasvReplyRecv(string Input)
        {
            FtpReply += Input;
            if (FtpClient.IsValidReply(FtpReply))
            {
                ExpectsReply = false;
                ProcessPasvReply(FtpReply);
                FtpReply = "";
                return true;
            }
            return false;
        }

        private void ProcessPasvReply(string Reply)
        {
            try
            {
                IPEndPoint ConnectTo = ParsePasvIP(Reply);
                DestinationSocket = new Socket(ConnectTo.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                DestinationSocket.BeginConnect(ConnectTo, new AsyncCallback(this.OnPasvConnected), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }

        private IPEndPoint ParsePasvIP(string Reply)
        {
            int StartIndex, StopIndex;
            string IPString;
            StartIndex = Reply.IndexOf("(");
            if (StartIndex == -1)
            {
                return null;
            }
            else
            {
                StopIndex = Reply.IndexOf(")", StartIndex);
                if (StopIndex == -1)
                    return null;
                else
                    IPString = Reply.Substring(StartIndex + 1, StopIndex - StartIndex - 1);
            }
            string[] Parts = IPString.Split(',');
            if (Parts.Length == 6)
                return new IPEndPoint(IPAddress.Parse(String.Join(".", Parts, 0, 4)), int.Parse(Parts[4]) * 256 + int.Parse(Parts[5]));
            else
                return null;
        }

        private void OnPasvConnected(IAsyncResult ar)
        {
            try
            {
                DestinationSocket.EndConnect(ar);
                ListenSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                ListenSocket.Listen(1);
                ListenSocket.BeginAccept(new AsyncCallback(this.OnPasvAccept), ListenSocket);
                Parent.SendCommand("227 Entering Passive Mode (" + Listener.GetLocalInternalIP().ToString().Replace('.', ',') + "," + Math.Floor(((IPEndPoint)ListenSocket.LocalEndPoint).Port / 256d).ToString() + "," + (((IPEndPoint)ListenSocket.LocalEndPoint).Port % 256).ToString() + ").\r\n");
            }
            catch
            {
                Dispose();
            }
        }

        private void OnPasvAccept(IAsyncResult ar)
        {
            try
            {
                ClientSocket = ListenSocket.EndAccept(ar);
                StartHandshake();
            }
            catch
            {
                Dispose();
            }
        }

        private Socket m_ListenSocket;
        private FtpClient m_Parent;
        private string m_FtpReply = "";
        private bool m_ExpectsReply = false;

    }
}

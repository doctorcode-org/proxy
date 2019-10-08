using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;

namespace ProxyServer.Socks
{
    public delegate void NegotiationCompleteDelegate(bool Success, Socket Remote);

    public abstract class SocksHandler
    {
        public SocksHandler(Socket ClientConnection, NegotiationCompleteDelegate Callback)
        {
            if (Callback == null)
                throw new ArgumentNullException();
            Connection = ClientConnection;
            Signaler = Callback;
        }

        internal string Username
        {
            get
            {
                return m_Username;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_Username = value;
            }
        }

        protected Socket Connection
        {
            get
            {
                return m_Connection;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_Connection = value;
            }
        }

        protected byte[] Buffer
        {
            get
            {
                return m_Buffer;
            }
        }

        protected byte[] Bytes
        {
            get
            {
                return m_Bytes;
            }
            set
            {
                m_Bytes = value;
            }
        }

        protected Socket RemoteConnection
        {
            get
            {
                return m_RemoteConnection;
            }
            set
            {
                m_RemoteConnection = value;
                try
                {
                    m_RemoteConnection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                }
                catch { }
            }
        }

        protected Socket AcceptSocket
        {
            get
            {
                return m_AcceptSocket;
            }
            set
            {
                m_AcceptSocket = value;
            }
        }

        protected IPAddress RemoteBindIP
        {
            get
            {
                return m_RemoteBindIP;
            }
            set
            {
                m_RemoteBindIP = value;
            }
        }

        protected void Dispose(bool Success)
        {
            if (AcceptSocket != null)
                AcceptSocket.Close();
            Signaler(Success, RemoteConnection);
        }

        public void StartNegotiating()
        {
            try
            {
                Connection.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveBytes), Connection);
            }
            catch
            {
                Dispose(false);
            }
        }

        protected void OnReceiveBytes(IAsyncResult ar)
        {
            try
            {
                int Ret = Connection.EndReceive(ar);

                if (Ret <= 0)
                    Dispose(false);

                AddBytes(Buffer, Ret);

                if (IsValidRequest(Bytes))
                    ProcessRequest(Bytes);
                else
                    Connection.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveBytes), Connection);
            }
            catch
            {
                Dispose(false);
            }
        }

        protected void OnDisposeGood(IAsyncResult ar)
        {
            try
            {
                if (Connection.EndSend(ar) > 0)
                {
                    Dispose(true);
                    return;
                }
            }
            catch { }
            Dispose(false);
        }

        protected void OnDisposeBad(IAsyncResult ar)
        {
            try
            {
                Connection.EndSend(ar);
            }
            catch { }
            Dispose(false);
        }

        protected void AddBytes(byte[] NewBytes, int Cnt)
        {
            if (Cnt <= 0 || NewBytes == null || Cnt > NewBytes.Length)
                return;
            if (Bytes == null)
            {
                Bytes = new byte[Cnt];
            }
            else
            {
                byte[] tmp = Bytes;
                Bytes = new byte[Bytes.Length + Cnt];
                Array.Copy(tmp, 0, Bytes, 0, tmp.Length);
            }
            Array.Copy(NewBytes, 0, Bytes, Bytes.Length - Cnt, Cnt);
        }

        protected void OnStartAccept(IAsyncResult ar)
        {
            try
            {
                if (Connection.EndSend(ar) <= 0)
                    Dispose(false);
                else
                    AcceptSocket.BeginAccept(new AsyncCallback(this.OnAccept), AcceptSocket);
            }
            catch
            {
                Dispose(false);
            }
        }

        protected abstract void OnAccept(IAsyncResult ar);
        protected abstract void Dispose(byte Value);
        protected abstract bool IsValidRequest(byte[] Request);
        protected abstract void ProcessRequest(byte[] Request);

        private string m_Username;
        private byte[] m_Buffer = new byte[1024];
        private byte[] m_Bytes;
        private Socket m_RemoteConnection;
        private Socket m_Connection;
        private Socket m_AcceptSocket;
        private IPAddress m_RemoteBindIP;
        private NegotiationCompleteDelegate Signaler;

    }

}

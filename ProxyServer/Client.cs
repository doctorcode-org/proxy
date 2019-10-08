using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;

namespace DoctorProxy
{
    public delegate void DestroyDelegate(Client client);

    public abstract class Client : IDisposable
    {
        public Client(Socket ClientSocket, DestroyDelegate Destroyer)
        {
            this.ClientSocket = ClientSocket;
            this.Destroyer = Destroyer;
        }

        public Client()
        {
            this.ClientSocket = null;
            this.Destroyer = null;
        }

        internal Socket ClientSocket
        {
            get
            {
                return m_ClientSocket;
            }
            set
            {
                if (m_ClientSocket != null)
                    m_ClientSocket.Close();
                m_ClientSocket = value;
            }
        }

        internal Socket DestinationSocket
        {
            get
            {
                return m_DestinationSocket;
            }
            set
            {
                if (m_DestinationSocket != null)
                    m_DestinationSocket.Close();
                m_DestinationSocket = value;
            }
        }

        protected byte[] Buffer
        {
            get
            {
                return m_Buffer;
            }
        }

        protected byte[] RemoteBuffer
        {
            get
            {
                return m_RemoteBuffer;
            }
        }

        public void Dispose()
        {
            try
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try
            {
                DestinationSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            //Close the sockets
            if (ClientSocket != null)
                ClientSocket.Close();
            if (DestinationSocket != null)
                DestinationSocket.Close();
            //Clean up
            ClientSocket = null;
            DestinationSocket = null;
            if (Destroyer != null)
                Destroyer(this);
        }

        public override string ToString()
        {
            try
            {
                return "Incoming connection from " + ((IPEndPoint)DestinationSocket.RemoteEndPoint).Address.ToString();
            }
            catch
            {
                return "Client connection";
            }
        }

        public void StartRelay()
        {
            try
            {
                ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
                DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnRemoteReceive), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }

        protected void OnClientReceive(IAsyncResult ar)
        {
            try
            {
                int Ret = ClientSocket.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                DestinationSocket.BeginSend(Buffer, 0, Ret, SocketFlags.None, new AsyncCallback(this.OnRemoteSent), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }

        protected void OnRemoteSent(IAsyncResult ar)
        {
            try
            {
                int Ret = DestinationSocket.EndSend(ar);
                if (Ret > 0)
                {
                    ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
                    return;
                }
            }
            catch { }
            Dispose();
        }

        protected void OnRemoteReceive(IAsyncResult ar)
        {
            try
            {
                int Ret = DestinationSocket.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                ClientSocket.BeginSend(RemoteBuffer, 0, Ret, SocketFlags.None, new AsyncCallback(this.OnClientSent), ClientSocket);
            }
            catch
            {
                Dispose();
            }
        }

        protected void OnClientSent(IAsyncResult ar)
        {
            try
            {
                int Ret = ClientSocket.EndSend(ar);
                if (Ret > 0)
                {
                    DestinationSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnRemoteReceive), DestinationSocket);
                    return;
                }
            }
            catch { }
            Dispose();
        }

        public abstract void StartHandshake();
        private DestroyDelegate Destroyer;
        private Socket m_ClientSocket;
        private Socket m_DestinationSocket;
        private byte[] m_Buffer = new byte[4096]; //0<->4095 = 4096
        private byte[] m_RemoteBuffer = new byte[1024];
    }
}

using DoctorProxy.EventLoger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Linq;

namespace DoctorProxy
{
    public abstract class Listener : IDisposable
    {
        public abstract void OnAccept(IAsyncResult ar);
        private int _Port;
        private IPAddress _Address;
        private Socket _ListenSocket;
        private List<Client> _Clients = new List<Client>();
        private bool _IsDisposed = false;

        public Listener(int Port, IPAddress Address)
        {
            this.Port = Port;
            this.Address = Address;
        }

        protected int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentException();
                _Port = value;
                Restart();
            }
        }

        protected IPAddress Address
        {
            get
            {
                return _Address;
            }
            set
            {
                _Address = value ?? throw new ArgumentNullException();
                Restart();
            }
        }

        protected Socket ListenSocket
        {
            get
            {
                return _ListenSocket;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                _ListenSocket = value;
            }
        }

        protected List<Client> Clients
        {
            get
            {
                return _Clients;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
        }

        public void Start()
        {
            try
            {
                ListenSocket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(new IPEndPoint(Address, Port));
                ListenSocket.Listen(50);
                ListenSocket.BeginAccept(new AsyncCallback(this.OnAccept), ListenSocket);
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
                ListenSocket = null;
                throw new SocketException();
            }
        }

        protected void Restart()
        {
            if (ListenSocket == null)
                return;
            ListenSocket.Close();
            Start();
        }

        protected void AddClient(Client client)
        {
            if (Clients.IndexOf(client) == -1)
                Clients.Add(client);
        }

        protected void RemoveClient(Client client)
        {
            Clients.Remove(client);
        }

        public int GetClientCount()
        {
            return Clients.Count;
        }

        public Client GetClientAt(int Index)
        {
            if (Index < 0 || Index >= GetClientCount())
                return null;
            return Clients[Index];
        }

        public bool Listening
        {
            get
            {
                return ListenSocket != null;
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            while (Clients.Count > 0)
            {
                Clients[0].Dispose();
            }

            try
            {
                ListenSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }

            if (ListenSocket != null)
                ListenSocket.Close();

            _IsDisposed = true;
        }

        ~Listener()
        {
            Dispose();
        }

        public static IPAddress GetLocalExternalIP()
        {
            try
            {
                IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());
                for (int i = 0; i < he.AddressList.Length; i++)
                {
                    if (IsRemoteIP(he.AddressList[i]))
                        return he.AddressList[i];
                }
                return he.AddressList[0];
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
                return IPAddress.Any;
            }
        }

        protected static bool IsRemoteIP(IPAddress IP)
        {
            var bytes = IP.GetAddressBytes();
            byte First = bytes[0];
            byte Second = bytes[1];
            return (First != 10) &&
                (First != 172 || (Second < 16 || Second > 31)) &&
                (First != 192 || Second != 168) &&
                (!IP.Equals(IPAddress.Any)) &&
                (!IP.Equals(IPAddress.Loopback)) &&
                (!IP.Equals(IPAddress.Broadcast));
        }

        protected static bool IsLocalIP(IPAddress IP)
        {
            var bytes = IP.GetAddressBytes();
            byte First = bytes[0];
            byte Second = bytes[1];

            return (First == 10) ||
                (First == 172 && (Second >= 16 && Second <= 31)) ||
                (First == 192 && Second == 168);
        }

        public static IPAddress GetLocalInternalIP()
        {
            try
            {
                IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());
                for (int Cnt = 0; Cnt < he.AddressList.Length; Cnt++)
                {
                    if (IsLocalIP(he.AddressList[Cnt]))
                        return he.AddressList[Cnt];
                }
                return he.AddressList[0];
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
                return IPAddress.Any;
            }
        }



    }
}

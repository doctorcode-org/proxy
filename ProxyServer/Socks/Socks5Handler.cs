using DoctorProxy;
using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer.Socks
{
    public class Socks5Handler : SocksHandler
    {
        private AuthBase m_AuthMethod;
        private IValidator Validator;

        public Socks5Handler(Socket ClientConnection, NegotiationCompleteDelegate Callback)
            : this(ClientConnection, Callback, null)
        {
        }

        public Socks5Handler(Socket ClientConnection, NegotiationCompleteDelegate Callback, IValidator validator)
            : base(ClientConnection, Callback)
        {
            Validator = validator;
        }


        protected override bool IsValidRequest(byte[] Request)
        {
            try
            {
                return (Request.Length == Request[0] + 1);
            }
            catch
            {
                return false;
            }
        }


        protected override void ProcessRequest(byte[] Methods)
        {
            try
            {
                byte result = 255;

                for (int i = 1; i < Methods.Length; i++)
                {
                    //0 = No authentication
                    if (Methods[i] == 0 && Validator == null)
                    {
                        result = 0;
                        AuthMethod = new AuthNone();
                        break;
                    }
                    //2 = Username/Password
                    else if (Methods[i] == 2 && Validator != null)
                    {
                        result = 2;
                        AuthMethod = new AuthUserPass(Validator);
                    }
                }

                Connection.BeginSend(new byte[] { 5, result }, 0, 2, SocketFlags.None, new AsyncCallback(this.OnAuthSent), Connection);
            }
            catch
            {
                Dispose(false);
            }
        }

        private void OnAuthSent(IAsyncResult ar)
        {
            try
            {
                if (Connection.EndSend(ar) <= 0 || AuthMethod == null)
                {
                    Dispose(false);
                    return;
                }
                AuthMethod.StartAuthentication(Connection, new AuthenticationCompleteDelegate(this.OnAuthenticationComplete));
            }
            catch
            {
                Dispose(false);
            }
        }

        private void OnAuthenticationComplete(bool Success)
        {
            try
            {
                if (Success)
                {
                    Bytes = null;
                    Connection.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnRecvRequest), Connection);
                }
                else
                {
                    Dispose(false);
                }
            }
            catch
            {
                Dispose(false);
            }
        }

        private void OnRecvRequest(IAsyncResult ar)
        {
            try
            {
                int Ret = Connection.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose(false);
                    return;
                }
                AddBytes(Buffer, Ret);
                if (IsValidQuery(Bytes))
                    ProcessQuery(Bytes);
                else
                    Connection.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnRecvRequest), Connection);
            }
            catch
            {
                Dispose(false);
            }
        }

        private bool IsValidQuery(byte[] Query)
        {
            try
            {
                switch (Query[3])
                {
                    case 1: //IPv4 address
                        return (Query.Length == 10);
                    case 3: //Domain name
                        return (Query.Length == Query[4] + 7);
                    case 4: //IPv6 address
                        //Not supported
                        Dispose(8);
                        return false;
                    default:
                        Dispose(false);
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void ProcessQuery(byte[] Query)
        {
            try
            {
                var strData = Encoding.UTF8.GetString(Query);

                switch (Query[1])
                {
                    case 1: //CONNECT
                        IPAddress RemoteIP = null;
                        int RemotePort = 0;
                        if (Query[3] == 1)
                        {
                            RemoteIP = IPAddress.Parse(Query[4].ToString() + "." + Query[5].ToString() + "." + Query[6].ToString() + "." + Query[7].ToString());
                            RemotePort = Query[8] * 256 + Query[9];
                        }
                        else if (Query[3] == 3)
                        {
                            RemoteIP = Dns.GetHostEntry(Encoding.ASCII.GetString(Query, 5, Query[4])).AddressList[0];
                            RemotePort = Query[4] + 5;
                            RemotePort = Query[RemotePort] * 256 + Query[RemotePort + 1];
                        }

                        RemoteConnection = new Socket(RemoteIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        RemoteConnection.BeginConnect(new IPEndPoint(RemoteIP, RemotePort), new AsyncCallback(this.OnConnected), RemoteConnection);
                        break;
                    case 2: //BIND
                        byte[] Reply = new byte[10];
                        var LocalIP = Listener.GetLocalExternalIP().GetAddressBytes();

                        AcceptSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        AcceptSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                        AcceptSocket.Listen(50);
                        Reply[0] = 5;  //Version 5
                        Reply[1] = 0;  //Everything is ok :)
                        Reply[2] = 0;  //Reserved
                        Reply[3] = 1;  //We're going to send a IPv4 address
                        Reply[4] = LocalIP[0];  //IP Address/1
                        Reply[5] = LocalIP[1];  //IP Address/2
                        Reply[6] = LocalIP[2];  //IP Address/3
                        Reply[7] = LocalIP[3];  //IP Address/4
                        Reply[8] = (byte)(Math.Floor(((IPEndPoint)AcceptSocket.LocalEndPoint).Port / 256d));  //Port/1
                        Reply[9] = (byte)(((IPEndPoint)AcceptSocket.LocalEndPoint).Port % 256);  //Port/2
                        Connection.BeginSend(Reply, 0, Reply.Length, SocketFlags.None, new AsyncCallback(this.OnStartAccept), Connection);
                        break;
                    case 3: //ASSOCIATE
                        //ASSOCIATE is not implemented (yet?)
                        Dispose(7);
                        break;
                    default:
                        Dispose(7);
                        break;
                }
            }
            catch
            {
                Dispose(1);
            }
        }

        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                RemoteConnection.EndConnect(ar);
                Dispose(0);
            }
            catch
            {
                Dispose(1);
            }
        }

        protected override void OnAccept(IAsyncResult ar)
        {
            try
            {
                RemoteConnection = AcceptSocket.EndAccept(ar);
                AcceptSocket.Close();
                AcceptSocket = null;
                Dispose(0);
            }
            catch
            {
                Dispose(1);
            }
        }

        protected override void Dispose(byte Value)
        {
            byte[] ToSend;
            try
            {
                var endPoint = (IPEndPoint)RemoteConnection.LocalEndPoint;
                //var address = endPoint.Address.Address;

                var bytes = endPoint.Address.GetAddressBytes();

                //var byte4 = (byte)(address % 256);
                //var byte5 = (byte)(Math.Floor((address % 65536) / 256d));
                //var byte6 = (byte)(Math.Floor((address % 16777216) / 65536d));
                //var byte7 = (byte)(Math.Floor((address % 16777216) / 16777216d));

                var byte8 = (byte)(Math.Floor(endPoint.Port / 256d));
                var byte9 = (byte)(endPoint.Port % 256);

                ToSend = new byte[] { 5, Value, 0, 1, bytes[0], bytes[1], bytes[2], bytes[3], byte8, byte9 };
                //ToSend = new byte[] { 5, Value, 0, 1, byte4, byte5, byte6, byte7, byte8, byte9 };
            }
            catch
            {
                ToSend = new byte[] { 5, 1, 0, 1, 0, 0, 0, 0, 0, 0 };
            }
            try
            {
                Connection.BeginSend(ToSend, 0, ToSend.Length, SocketFlags.None, (AsyncCallback)(ToSend[1] == 0 ? new AsyncCallback(this.OnDisposeGood) : new AsyncCallback(this.OnDisposeBad)), Connection);
            }
            catch
            {
                Dispose(false);
            }
        }

        private AuthBase AuthMethod
        {
            get
            {
                return m_AuthMethod;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_AuthMethod = value;
            }
        }




    }
}

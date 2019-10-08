using DoctorProxy;
using DoctorProxy.EventLoger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Socks
{
    public sealed class SocksClient : Client
    {
        private IValidator Validator;
        private SocksHandler m_Handler;

        public SocksClient(Socket ClientSocket, DestroyDelegate Destroyer, IValidator validator)
            : base(ClientSocket, Destroyer)
        {
            Validator = validator;
        }

        internal SocksHandler Handler
        {
            get
            {
                return m_Handler;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_Handler = value;
            }
        }


        public override void StartHandshake()
        {
            try
            {
                ClientSocket.BeginReceive(Buffer, 0, 1, SocketFlags.None, new AsyncCallback(this.OnStartSocksProtocol), ClientSocket);
            }
            catch
            {
                Dispose();
            }
        }


        private void OnStartSocksProtocol(IAsyncResult ar)
        {
            int Ret;
            try
            {
                Ret = ClientSocket.EndReceive(ar);
                if (Ret <= 0 || Buffer[0] != 5)
                {
                    Dispose();
                    return;
                }

                Handler = new Socks5Handler(ClientSocket, new NegotiationCompleteDelegate(this.OnEndSocksProtocol), Validator);
                Handler.StartNegotiating();
            }
            catch
            {
                Dispose();
            }
        }

        private void OnEndSocksProtocol(bool Success, Socket Remote)
        {
            DestinationSocket = Remote;
            if (Success)
                StartRelay();
            else
                Dispose();
        }



        public override string ToString()
        {
            try
            {
                if (Handler != null)
                    return Handler.Username + " (" + ((IPEndPoint)ClientSocket.LocalEndPoint).Address.ToString() + ") connected to " + DestinationSocket.RemoteEndPoint.ToString();
                else
                    return "SOCKS connection from " + ((IPEndPoint)ClientSocket.LocalEndPoint).Address.ToString();
            }
            catch
            {
                return "Incoming SOCKS connection";
            }
        }


    }
}

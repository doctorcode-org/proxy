using DoctorProxy;
using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;

namespace ProxyServer.Socks
{
    public sealed class SocksListener : Listener
    {
        private IValidator Validator;

        public SocksListener(int Port) : this(IPAddress.Any, Port, null) { }

        public SocksListener(IPAddress Address, int Port) : this(Address, Port, null) { }

        public SocksListener(int Port, IValidator validator) : this(IPAddress.Any, Port, validator) { }

        public SocksListener(IPAddress Address, int Port, IValidator validator)
            : base(Port, Address)
        {
            Validator = validator;
        }

        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket NewSocket = ListenSocket.EndAccept(ar);
                if (NewSocket != null)
                {
                    SocksClient NewClient = new SocksClient(NewSocket, new DestroyDelegate(this.RemoveClient), Validator);
                    AddClient(NewClient);
                    NewClient.StartHandshake();
                }
            }
            catch { }

            try
            {
                //Restart Listening
                ListenSocket.BeginAccept(new AsyncCallback(this.OnAccept), ListenSocket);
            }
            catch
            {
                Dispose();
            }
        }


    }
}

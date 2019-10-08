using DoctorProxy;
using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;

namespace ProxyServer.Ftp
{
    public sealed class FtpListener : Listener
    {
        public FtpListener(int Port) : this(IPAddress.Any, Port) { }
        public FtpListener(IPAddress Address, int Port) : base(Port, Address) { }

        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket NewSocket = ListenSocket.EndAccept(ar);
                if (NewSocket != null)
                {
                    FtpClient NewClient = new FtpClient(NewSocket, new DestroyDelegate(this.RemoveClient));
                    AddClient(NewClient);
                    NewClient.StartHandshake();
                }
            }
            catch { }
            try
            {
                ListenSocket.BeginAccept(new AsyncCallback(this.OnAccept), ListenSocket);
            }
            catch
            {
                Dispose();
            }
        }


    }
}

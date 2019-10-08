using DoctorProxy.EventLoger;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace DoctorProxy.Http
{
    public sealed class HttpListener : Listener
    {
        IValidator Validator = null;

        public HttpListener(int Port) : this(IPAddress.Any, Port, null) { }

        public HttpListener(int Port, IValidator validator) : this(IPAddress.Any, Port, validator) { }

        public HttpListener(IPAddress Address, int Port, IValidator validator)
            : base(Port, Address)
        {
            Validator = validator;
        }

        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket newSocket = ListenSocket.EndAccept(ar);
                if (newSocket != null)
                {
                    HttpClient NewClient = new HttpClient(newSocket, new DestroyDelegate(this.RemoveClient), Validator);
                    AddClient(NewClient);
                    NewClient.StartHandshake();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }

            try
            {
                ListenSocket.BeginAccept(new AsyncCallback(this.OnAccept), ListenSocket);
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
                Dispose();
            }
        }

 
    }
}

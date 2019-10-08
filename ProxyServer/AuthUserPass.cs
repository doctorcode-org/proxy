using DoctorProxy.EventLoger;
using System;
using System.Net.Sockets;
using System.Text;

namespace DoctorProxy
{
    internal sealed class AuthUserPass : AuthBase
    {
        private IValidator Validator;

        public AuthUserPass(IValidator validator)
        {
            Validator = validator;
        }

        internal override void StartAuthentication(Socket Connection, AuthenticationCompleteDelegate Callback)
        {
            this.Connection = Connection;
            this.Callback = Callback;
            try
            {
                Bytes = null;
                Connection.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnRecvRequest), Connection);
            }
            catch
            {
                Callback(false);
            }
        }

        private void OnRecvRequest(IAsyncResult ar)
        {
            try
            {
                int Ret = Connection.EndReceive(ar);
                if (Ret <= 0)
                {
                    Callback(false);
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
                Callback(false);
            }
        }

        private bool IsValidQuery(byte[] Query)
        {
            try
            {
                return (Query.Length == Query[1] + Query[Query[1] + 2] + 3);
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
                string User = Encoding.ASCII.GetString(Query, 2, Query[1]);
                string Pass = Encoding.ASCII.GetString(Query, Query[1] + 3, Query[Query[1] + 2]);
                byte[] ToSend;
                if (Validator == null || Validator.IsValid(User, Pass))
                {
                    ToSend = new byte[] { 5, 0 };
                    Connection.BeginSend(ToSend, 0, ToSend.Length, SocketFlags.None, new AsyncCallback(this.OnOkSent), Connection);
                }
                else
                {
                    ToSend = new Byte[] { 5, 1 };
                    Connection.BeginSend(ToSend, 0, ToSend.Length, SocketFlags.None, new AsyncCallback(this.OnUhohSent), Connection);
                }
            }
            catch
            {
                Callback(false);
            }
        }

        private void OnOkSent(IAsyncResult ar)
        {
            try
            {
                if (Connection.EndSend(ar) <= 0)
                    Callback(false);
                else
                    Callback(true);
            }
            catch
            {
                Callback(false);
            }
        }

        private void OnUhohSent(IAsyncResult ar)
        {
            try
            {
                Connection.EndSend(ar);
            }
            catch { }
            Callback(false);
        }



    }
}

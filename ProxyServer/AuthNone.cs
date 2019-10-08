using System.Net.Sockets;

namespace DoctorProxy
{
    internal sealed class AuthNone : AuthBase
    {
        public AuthNone() { }
        internal override void StartAuthentication(Socket Connection, AuthenticationCompleteDelegate Callback)
        {
            Callback(true);
        }
    }
}

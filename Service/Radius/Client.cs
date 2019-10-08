using System;
using System.Text;
using System.Reflection;
using System.Net.Sockets;

namespace DoctorProxy.Service.Radius
{
    public class Client
    {
        private const int UDP_TTL = 20;
        private int pUDPTimeout = 5;

        private string _Server;
        private string _Secret;
        private int _Port;
        private byte[] secretBytes;

        public Client(string server, int port, string secret)
        {
            _Server = server;
            _Secret = secret;
            _Port = port;
            secretBytes = Encoding.Default.GetBytes(secret);
        }

        public bool Authenticate(string username, string password)
        {
            Random pRandonNumber = new Random();
            var RA = GenerateRA();

            var requestPacket = new RadiusPacket()
            {
                CodeType = RadiusCodeType.AccessRequest,
                Identifier = Convert.ToByte(pRandonNumber.Next(0, 32000) % 256),
                Authenticator = RA
            };

            requestPacket.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, username));
            requestPacket.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserPassword, PapAuthenticator.GeneratePAPPassword(password, _Secret, RA)));
            requestPacket.Serialize(secretBytes);

            if (requestPacket.Parse() == RadiusPacket.ParseError.None)
            {
                try
                {
                    using (var server = new UdpClient())
                    {
                        server.Client.SendTimeout = pUDPTimeout;
                        server.Client.ReceiveTimeout = pUDPTimeout;
                        server.Client.Ttl = UDP_TTL;
                        server.Connect(_Server, _Port);
                        server.Send(requestPacket.Data, requestPacket.DataLength);

                        var RemoteIpEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                        var response = server.Receive(ref RemoteIpEndPoint);
                        server.Close();

                        var result = new RadiusPacket() { Data = response, DataLength = response.Length };
                        return (result.Parse() == RadiusPacket.ParseError.None && result.CodeType == RadiusCodeType.AccessAccept);
                    }                   
                }
                catch (Exception ex)
                {
                    EventLoger.Log.Write(MethodInfo.GetCurrentMethod(), ex);
                }
            }

            return false;
        }

        private static byte[] GenerateRA()
        {
            Random pRandonNumber = new Random();
            byte[] pRA = new byte[16];
            for (int i = 0; i < 15; i++)
            {
                pRA[i] = Convert.ToByte(1 + pRandonNumber.Next() % 255);
                pRandonNumber.Next();
            }

            return pRA;
        }

    }
}

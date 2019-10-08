using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DoctorProxy.Service.Radius
{
    public class Server
    {
        IPAddress _IP;
        int _Port;
        string _Secret;
        bool listen = false;
        IValidator Validator;

        public Server(string secret, IValidator validator)
        {
            _IP = IPAddress.Any;
            _Port = 1812;
            _Secret = secret;
            Validator = validator;
        }

        public Server(string server, int port, string secret, IValidator validator)
        {
            _IP = Dns.GetHostAddresses(server)[0];
            _Port = port;
            _Secret = secret;
            Validator = validator;
        }

        public async Task Start()
        {
            listen = true;

            using (var udpServer = new UdpClient(new IPEndPoint(_IP, _Port)))
            {
                while (listen)
                {
                    try
                    {
                        var result = await udpServer.ReceiveAsync();
                        var packet = new RadiusPacket()
                        {
                            DataLength = result.Buffer.Length,
                            Data = result.Buffer
                        };

                        if (packet.Parse() == RadiusPacket.ParseError.None && packet.CodeType == RadiusCodeType.AccessRequest)
                        {
                            var secretBytes = Encoding.Default.GetBytes(_Secret);

                            var responsePacket = new RadiusPacket()
                            {
                                Identifier = packet.Identifier,
                                CodeType = RadiusCodeType.AccessReject,
                                Authenticator = packet.Authenticator
                            };

                            foreach (var attr in packet.Attributes)
                            {
                                responsePacket.Attributes.Add(attr);
                            }

                            var falts = packet.ValidateRfcComplianceOnReceive();
                            if (falts != null)
                            {
                                responsePacket.Attributes.Add(falts);
                            }
                            else
                            {
                                var userNameAttrib = packet.Attributes.Find(RadiusAttributeType.UserName);
                                var passwordAttrib = packet.Attributes.Find(RadiusAttributeType.UserPassword);
                                var username = userNameAttrib.TextValue;
                                var password = PapAuthenticator.Reverse(secretBytes, packet.Authenticator, passwordAttrib.StringValue);

                                if (Validator == null || Validator.IsValid(username, password))
                                    responsePacket.CodeType = RadiusCodeType.AccessAccept;
                            }

                            responsePacket.Serialize(secretBytes);
                            udpServer.Send(responsePacket.Data, responsePacket.Length, result.RemoteEndPoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLoger.Log.Write(MethodInfo.GetCurrentMethod(), ex);
                    }
                }
            }
        }


        public void Stop()
        {
            listen = false;
        }

    }
}

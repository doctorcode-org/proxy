using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace DoctorProxy.Service.Radius
{
    public class RadiusPacket
    {
        public enum ParseError
        {
            None,
            InvalidPacketLength,
            InvalidCodeField,
            InvalidLengthField,
            InvalidAttributeLengthField
        }

        public const int BUFFER_SIZE = 4224;
        public RadiusAttributeCollection Attributes { get; set; }

        public byte[] Data { get; set; }
        public int DataLength { get; set; }



        public RadiusPacket()
        {
            Data = new byte[BUFFER_SIZE];
            Attributes = new RadiusAttributeCollection();
        }

        public RadiusPacket(Stream source)
        {
            DataLength = (int)source.Length;
            Data = new byte[DataLength];
            source.Read(Data, 0, DataLength);
            Attributes = new RadiusAttributeCollection();
        }


        public RadiusCodeType CodeType
        {
            get
            {
                if (Enum.IsDefined(typeof(RadiusCodeType), Data[0]))
                    return (RadiusCodeType)(Data[0]);
                return RadiusCodeType.Unknown;
            }
            set { Data[0] = (byte)value; }
        }

        public byte Code
        {
            get { return Data[0]; }
            set { Data[0] = value; }
        }

        public byte Identifier
        {
            get { return Data[1]; }
            set { Data[1] = value; }
        }

        public short Length
        {
            get { return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(Data, 2)); }
            protected set { Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 0, Data, 2, 2); }
        }

        public byte[] Authenticator
        {
            get
            {
                var ba = new byte[16];
                Buffer.BlockCopy(Data, 4, ba, 0, 16);
                return ba;
            }
            set { Buffer.BlockCopy(value, 0, Data, 4, 16); }
        }

        public ParseError Parse()
        {
            // is the received data length valid?
            if (DataLength < 20 || DataLength > 4096)
                return ParseError.InvalidPacketLength;

            // parse the code field
            if (CodeType == RadiusCodeType.Unknown)
                return ParseError.InvalidCodeField;

            // is the RADIUS length field valid?
            if (Length != DataLength)
                return ParseError.InvalidLengthField;

            // parse any attributes
            short length = Length; // avoid NetworkToHostOrder overhead
            int dataRemaining = length - 20;
            while (dataRemaining > 0)
            {
                if (dataRemaining < 2)
                {
                    return ParseError.InvalidPacketLength;
                }

                // parse the length field
                byte attrLengthField = Data[length - dataRemaining + 1];

                // is the ATTRIBUTE length field valid?
                if (attrLengthField > dataRemaining || attrLengthField < 2)
                    return ParseError.InvalidAttributeLengthField;

                var ra = new RadiusAttribute();
                ra.Parse(Data, (short)(length - dataRemaining));
                Attributes.Add(ra);
                dataRemaining -= attrLengthField;
            }

            return ParseError.None;
        }

        public void Serialize()
        {
            short offset = 20;

            foreach (RadiusAttribute ra in Attributes)
            {
                if (ra.LengthField != 0) // drop zero-length attributes
                    offset += ra.Serialize(Data, offset);
            }

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(offset)), 0, Data, 2, 2);
            DataLength = offset;
        }

        public void Serialize(byte[] sharedSecret)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Serialize();
            Buffer.BlockCopy(sharedSecret, 0, Data, DataLength, sharedSecret.Length);
            Buffer.BlockCopy(md5.ComputeHash(Data, 0, DataLength + sharedSecret.Length), 0, Data, 4, 16);
        }

        public void Clear()
        {
            Array.Clear(Data, 0, BUFFER_SIZE);
            DataLength = 0;
        }

        public bool HasAttribute(RadiusAttributeType attrib)
        {
            return this.Attributes.Any(attr => attr.AttributeType != null && attr.AttributeType.Value == attrib);
        }

        public RadiusAttribute ValidateRfcComplianceOnReceive()
        {
            //if (!this.HasAttribute(RadiusAttributeType.NASIdentifier) && !this.HasAttribute(RadiusAttributeType.NASIPAddress))
            //{
            //    return new RadiusAttribute(RadiusAttributeType.ReplyMessage, "Access-Request did not contain NAS-IP-Address or NAS-Identifier.");
            //}

            //if (!this.HasAttribute(RadiusAttributeType.UserPassword) &&
            //    !this.HasAttribute(RadiusAttributeType.CHAPPassword) &&
            //    !this.HasAttribute(RadiusAttributeType.State))
            //{
            //    return new RadiusAttribute(RadiusAttributeType.ReplyMessage, "Access-Request did not contain User-Password, CHAP-Password, or State.");
            //}

            //if (this.HasAttribute(RadiusAttributeType.UserPassword) && this.HasAttribute(RadiusAttributeType.CHAPPassword))
            //{
            //    return new RadiusAttribute(RadiusAttributeType.ReplyMessage, "Access-Request cannot contain both User-Password and CHAP-Password.");
            //}


            //For PAP
            if (!this.HasAttribute(RadiusAttributeType.UserName))
            {
                return new RadiusAttribute(RadiusAttributeType.ReplyMessage, "Access-Request must specify User-Name in this implementation.");
            }

            if (!this.HasAttribute(RadiusAttributeType.UserPassword))
            {
                return new RadiusAttribute(RadiusAttributeType.ReplyMessage, "Access-Request must specify User-Password in this implementation.");
            }

            return null;
        }
    }
}

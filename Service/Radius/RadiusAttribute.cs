using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.Service.Radius
{
    public class RadiusAttribute : IEquatable<RadiusAttribute>, IComparable<RadiusAttribute>
    {
        public const int BUFFER_SIZE = 255;

        public static readonly List<RadiusAttributeType> IntegerAttributes = new List<RadiusAttributeType>
                                                                                 {
                                                                                     RadiusAttributeType.NASPortType,
                                                                                     RadiusAttributeType.NASPort,
                                                                                     RadiusAttributeType.NASPortId
                                                                                 };

        public static readonly List<RadiusAttributeType> StringAttributes = new List<RadiusAttributeType>
                                                                                {
                                                                                    RadiusAttributeType.UserName,
                                                                                    RadiusAttributeType.NASIdentifier
                                                                                };

        protected byte[] data;

        #region constructors

        public RadiusAttribute()
        {
            data = new byte[BUFFER_SIZE];
        }

        public RadiusAttribute(RadiusAttributeType type, string value)
        {
            data = new byte[BUFFER_SIZE];
            TypeField = (byte)type;
            TextValue = value;
        }

        public RadiusAttribute(RadiusAttributeType type, byte[] value)
        {
            data = new byte[BUFFER_SIZE];
            TypeField = (byte)type;
            StringValue = value;
        }

        public RadiusAttribute(RadiusAttributeType type, IPAddress value)
        {
            data = new byte[BUFFER_SIZE];
            TypeField = (byte)type;
            AddressValue = value;
        }

        public RadiusAttribute(RadiusAttributeType type, Int32 value)
        {
            data = new byte[BUFFER_SIZE];
            TypeField = (byte)type;
            IntegerValue = value;
        }

        #endregion

        public int VendorId { get; set; }

        #region AttributeType

        public RadiusAttributeType? AttributeType
        {
            get
            {
                if (Enum.IsDefined(typeof(RadiusAttributeType), TypeField))
                    return (RadiusAttributeType)TypeField;

                return null;
            }
        }

        #endregion

        #region TypeField

        public byte TypeField
        {
            get { return data[0]; }
            set { data[0] = value; }
        }

        #endregion

        #region LengthField

        public byte LengthField
        {
            get { return data[1]; }
            protected set { data[1] = value; }
        }

        #endregion

        #region DataLength

        public byte DataLength
        {
            get { return (byte)(LengthField - 2); }
        }

        #endregion

        #region Parse

        public void Parse(byte[] inData, short offset)
        {
            TypeField = inData[offset];
            LengthField = inData[offset + 1];
            Buffer.BlockCopy(inData, offset + 2, this.data, 2, LengthField - 2);
        }

        #endregion

        #region Serialize

        public byte Serialize(byte[] inData, short offset)
        {
            Buffer.BlockCopy(this.data, 0, inData, offset, LengthField);
            return LengthField;
        }

        #endregion

        #region Clear

        public void Clear()
        {
            Array.Clear(data, 0, BUFFER_SIZE);
        }

        #endregion

        #region TextValue

        public string TextValue
        {
            get { return Encoding.ASCII.GetString(data, 2, LengthField - 2); }
            set
            {
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(value), 0, data, 2, value.Length);
                LengthField = (byte)(value.Length + 2);
            }
        }


        public string Base64EncodedValue
        {
            get { return Convert.ToBase64String(data, 2, LengthField - 2); }
        }

        public object TypedValue
        {
            get
            {
                if (StringAttributes.Contains(AttributeType.Value))
                    return TextValue;

                if (IntegerAttributes.Contains(AttributeType.Value))
                    return IntegerValue;

                return StringValue;
            }
        }

        #endregion

        #region StringValue

        public byte[] StringValue
        {
            get
            {
                var ba = new byte[LengthField - 2];
                Buffer.BlockCopy(data, 2, ba, 0, LengthField - 2);
                return ba;
            }
            set
            {
                Buffer.BlockCopy(value, 0, data, 2, value.Length);
                LengthField = (byte)(value.Length + 2);
            }
        }

        #endregion

        #region AddressValue

        protected IPAddress AddressValue
        {
            get
            {
                long longAddress = BitConverter.ToInt32(data, 2);
                return new IPAddress(longAddress);
            }

            set
            {
                Buffer.BlockCopy(value.GetAddressBytes(), 0, data, 2, 4);
                LengthField = 6;
            }
        }

        #endregion

        #region IntegerValue

        public Int32 IntegerValue
        {
            get { return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 2)); }

            set
            {
                Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 0, data, 2, 4);
                LengthField = 6;
            }
        }

        #endregion

        #region IComparable<RadiusAttribute> Members

        public int CompareTo(RadiusAttribute other)
        {
            return Equals(other) ? 0 : -1;
        }

        #endregion

        #region IEquatable<RadiusAttribute> Members

        public bool Equals(RadiusAttribute ra)
        {
            return (ra.TypeField == TypeField && ra.LengthField == LengthField);
        }

        #endregion
    }
}

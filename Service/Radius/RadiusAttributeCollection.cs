using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.Service.Radius
{
    public class RadiusAttributeCollection : List<RadiusAttribute>
    {
        public int CountType(RadiusAttributeType type)
        {
            int count = 0;

            foreach (RadiusAttribute ra in this)
            {
                if (ra.TypeField == (byte)type)
                    count++;
            }
            return count;
        }

        public RadiusAttribute Find(RadiusAttributeType type)
        {
            foreach (RadiusAttribute ra in this)
            {
                if (ra.AttributeType == type)
                    return ra;
            }
            return null;
        }
    }
}

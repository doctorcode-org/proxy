using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.DataModel
{
    public enum AuthenticationTypes
    {
        AllowAll = 0,
        UsernamePassword = 1,
        RemoteRadius = 2
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DoctorProxy.EventLoger
{
    public class Log
    {
        public static void Write(MethodBase method, Exception error)
        {
            Write(method, string.Format("[Error] {0}", error.Message));
        }

        public static void Write(MethodBase method, string msg)
        {
            try
            {
                var path = string.Format("{0}\\log.log", AppDomain.CurrentDomain.BaseDirectory);
                var data = string.Format("[{0}][{1}]=> {2}", DateTime.Now, method.Name, msg);
                File.AppendAllLines(path, new List<string>() { data });
            }
            catch { }
        }
    }
}

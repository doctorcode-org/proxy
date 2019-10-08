using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Reflection;
using DoctorProxy.EventLoger;

namespace DoctorProxy.Service
{
    public enum ServiceStatus
    {
        NotExist = 0,
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7
    }

    public enum ServiceCommands
    {
        StopWorker = 128,
        RestartWorker,
        CheckWorker
    };

    public class Manager
    {

        #region DLLImport

        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);

        [DllImport("advapi32.dll")]
        public static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName, int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName, string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

        [DllImport("advapi32.dll")]
        public static extern void CloseServiceHandle(IntPtr SCHANDLE);

        [DllImport("advapi32.dll")]
        private static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);

        [DllImport("advapi32.dll")]
        public static extern int DeleteService(IntPtr SVHANDLE);

        //[DllImport("kernel32.dll")]
        //public static extern int GetLastError();

        #endregion DLLImport

        public bool IsServiceExist(string serviceName)
        {
            var services = ServiceController.GetServices();
            return services.Any(s => s.ServiceName == serviceName);
        }

        public ServiceStatus GetServiceState(string serviceName)
        {
            var services = ServiceController.GetServices();
            var target = services.Where(s => s.ServiceName == serviceName).FirstOrDefault();
            if (target != null)
            {
                var state = (int)target.Status;
                return (ServiceStatus)state;
            }

            return ServiceStatus.NotExist;
        }

        public bool StartService(string serviceName, int timeout)
        {
            var services = ServiceController.GetServices();
            var service = services.Where(s => s.ServiceName == serviceName).FirstOrDefault();
            if (service != null)
            {
                try
                {
                    if (service.Status == ServiceControllerStatus.Running)
                        return true;

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(timeout));
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodInfo.GetCurrentMethod(), ex);
                }
            }

            return false;
        }

        public bool StopService(string serviceName, int timeout)
        {
            var services = ServiceController.GetServices();
            var service = services.Where(s => s.ServiceName == serviceName).FirstOrDefault();
            if (service != null)
            {
                try
                {
                    if (service.Status == ServiceControllerStatus.Stopped)
                        return true;

                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(timeout));
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodInfo.GetCurrentMethod(), ex);
                }

                return false;
            }

            return true;
        }

        public bool ExecuteCommand(string serviceName, ServiceCommands command)
        {
            try
            {
                var services = ServiceController.GetServices();
                var service = services.Where(s => s.ServiceName == serviceName).FirstOrDefault();
                if (service != null)
                {
                    service.ExecuteCommand((int)command);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }

            return false;
        }

        public bool InstallService(string servicePath, string serviceName, string serviceDispName)
        {

            #region Constants declaration.
            int SC_MANAGER_CREATE_SERVICE = 0x0002;
            int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
            //int SERVICE_DEMAND_START = 0x00000003;
            int SERVICE_ERROR_NORMAL = 0x00000001;
            int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            int SERVICE_QUERY_CONFIG = 0x0001;
            int SERVICE_CHANGE_CONFIG = 0x0002;
            int SERVICE_QUERY_STATUS = 0x0004;
            int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            int SERVICE_START = 0x0010;
            int SERVICE_STOP = 0x0020;
            int SERVICE_PAUSE_CONTINUE = 0x0040;
            int SERVICE_INTERROGATE = 0x0080;
            int SERVICE_USER_DEFINED_CONTROL = 0x0100;
            int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL);
            int SERVICE_AUTO_START = 0x00000002;
            #endregion Constants declaration.
            try
            {
                IntPtr sc_handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
                if (sc_handle.ToInt32() != 0)
                {
                    IntPtr sv_handle = CreateService(sc_handle, serviceName, serviceDispName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_AUTO_START, SERVICE_ERROR_NORMAL, servicePath, null, 0, null, null, null);
                    if (sv_handle.ToInt32() == 0)
                    {
                        CloseServiceHandle(sc_handle);
                        return false;
                    }
                    else
                    {
                        //int i = StartService(sv_handle, 0, null);
                        CloseServiceHandle(sv_handle);
                        CloseServiceHandle(sc_handle);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }

            return false;
        }

        public bool UninstallService(string serviceName)
        {
            try
            {
                int GENERIC_WRITE = 0x40000000;
                IntPtr sc_hndl = OpenSCManager(null, null, GENERIC_WRITE);
                if (sc_hndl.ToInt32() != 0)
                {
                    int DELETE = 0x10000;
                    IntPtr svc_hndl = OpenService(sc_hndl, serviceName, DELETE);
                    if (svc_hndl.ToInt32() != 0)
                    {
                        var result = (DeleteService(svc_hndl) != 0);
                        CloseServiceHandle(svc_hndl);
                        CloseServiceHandle(sc_hndl);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }

            return false;
        }

    }
}

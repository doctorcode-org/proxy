using DoctorProxy.Data;
using DoctorProxy.DataModel;
using DoctorProxy.EventLoger;
using DoctorProxy.Http;
using ProxyServer.Ftp;
using ProxyServer.Socks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.Service
{

    public partial class MainService : ServiceBase
    {
        private HttpListener httpListner;
        private SocksListener socksListener;
        private FtpListener ftpListener;
        private Settings settings;
        private DbContext dbContext;
        private Radius.Server radiusServer;

        private IValidator validator;

        private class AuthUsers : IValidator
        {
            Settings _Settings;

            public AuthUsers(Settings setting)
            {
                _Settings = setting;
            }

            public bool IsValid(string username, string password)
            {
                try
                {
                    if (_Settings.AuthenticationType == AuthenticationTypes.AllowAll)
                        return true;

                    if (_Settings.AuthenticationType == AuthenticationTypes.UsernamePassword)
                    {
                        //برو از دیتابیس بخون و چک کن
                    }

                    if (_Settings.AuthenticationType == AuthenticationTypes.RemoteRadius)
                    {
                        var client = new Radius.Client(_Settings.ExternalRadiusServerUrl, _Settings.ExternalRadiusServerPort.Value, _Settings.ExternalRadiusServerSharedSecret);
                        return client.Authenticate(username, password);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(MethodInfo.GetCurrentMethod(), ex);
                }

                return false;
            }
        }



        public MainService()
        {
            InitializeComponent();
            dbContext = new DbContext();
            settings = dbContext.GetSettings();
            validator = settings.AuthenticationType == AuthenticationTypes.AllowAll ? null : new AuthUsers(settings);
        }


        protected override void OnCustomCommand(int command)
        {

        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (settings.HttpProxy)
                    StartProxyService();

                if (settings.SocksProxy)
                {
                    //StartSocksService();
                }

                if (settings.FtpProxy)
                {
                    //StartFtpService();
                }

                if (settings.OpenVPN)
                {
                    //StartOpenVpnService();
                }

                if (settings.EnableInternalRadiusServer)
                {
                    StartRadiusServer();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod(), ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                httpListner?.Dispose();
                radiusServer?.Stop();
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod(), ex);
            }
        }

        /// <summary>
        /// Start HTTP Proxy
        /// </summary>
        private void StartProxyService()
        {
            try
            {
                if (IsPortAvailable(settings.HttpProxyPort.Value) == false)
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "HttpProxyPort in use!");
                    return;
                }

                httpListner = new HttpListener(settings.HttpProxyPort.Value, validator);
                httpListner.Start();
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod(), ex);
            }
        }

        /// <summary>
        /// Start Soucks Proxy
        /// </summary>
        private void StartSocksService()
        {
            try
            {
                socksListener = new SocksListener(settings.SocksProxyPort.Value, validator);
                socksListener.Start();
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod(), ex);
            }
        }

        /// <summary>
        /// Start OpenVPN
        /// </summary>
        private void StartOpenVpnService()
        {

        }

        /// <summary>
        /// Start FTP Proxy
        /// </summary>
        private void StartFtpService()
        {
            try
            {
                ftpListener = new FtpListener(settings.FtpProxyPort.Value);
                ftpListener.Start();
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void StartRadiusServer()
        {
            Task.Run(async () =>
            {
                radiusServer = new Radius.Server(settings.InternalRadiusServerUrl, settings.InternalRadiusServerPort.Value, settings.InternalRadiusServerSharedSecret, validator);
                await radiusServer.Start();
            });
        }

        private bool IsPortAvailable(int port)
        {
            var ipGlobal = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = ipGlobal.GetActiveTcpConnections();
            return !tcpConnections.Any(tcp => tcp.LocalEndPoint.Port == port);
        }

    }
}

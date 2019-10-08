using DoctorProxy.DataModel;
using System.Collections.Generic;
using System.Web.Caching;
using System.Reflection;
using System.Data.SQLite;

namespace DoctorProxy.Data
{
    public class DbContext : SQLiteDataProvider
    {
        //private CacheDependency dependency = null;
        private const string SettingsCacheKey = "Settings";

        public Settings GetSettings()
        {
            var result = CacheManager.GetDataFromCache<Settings>(SettingsCacheKey);

            if (result == null)
            {
                var sql = "SELECT * FROM tbl_Settings";

                var dt = GetData(sql);
                result = dt.ToObjectOf<Settings>();
                CacheManager.AddDataToCache(SettingsCacheKey, result, null);
            }

            return result;
        }


        public bool SaveSettings(Settings newSettings)
        {
            var sql = "UPDATE tbl_Settings SET HttpProxy = @HttpProxy, " +
                                              "HttpProxyPort = @HttpProxyPort, " +
                                              "SslProxy = @SslProxy, " +
                                              "SslProxyPort = @SslProxyPort, " +
                                              "TunnelPlus = @TunnelPlus, " +
                                              "TunnelPlusPort = @TunnelPlusPort, " +
                                              "FtpProxy = @FtpProxy, " +
                                              "FtpProxyPort = @FtpProxyPort, " +
                                              "SocksProxy = @SocksProxy, " +
                                              "SocksProxyPort = @SocksProxyPort, " +
                                              "pSocks = @pSocks, " +
                                              "pSocksPort = @pSocksPort, " +
                                              "tSocks = @tSocks, " +
                                              "tSocksPort = @tSocksPort, " +
                                              "OpenVPN = @OpenVPN, " +
                                              "ServerName = @ServerName, " +
                                              "OpenVpnUdp = @OpenVpnUdp, " +
                                              "OpenVpnUdpPort = @OpenVpnUdpPort, " +
                                              "OpenVpnTcp = @OpenVpnTcp, " +
                                              "OpenVpnTcpPort = @OpenVpnTcpPort, " +
                                              "SslVpn = @SslVpn, " +
                                              "SslVpnPort = @SslVpnPort, " +
                                              "VpnPlus = @VpnPlus, " +
                                              "VpnPlusPort = @VpnPlusPort, " +
                                              "EnableInternalRadiusServer = @EnableInternalRadiusServer, " +
                                              "InternalRadiusServerUrl = @InternalRadiusServerUrl, " +
                                              "InternalRadiusServerPort = @InternalRadiusServerPort, " +
                                              "InternalRadiusServerSharedSecret = @InternalRadiusServerSharedSecret, " +
                                              "EnableExternalRadiusServer = @EnableExternalRadiusServer, " +
                                              "ExternalRadiusServerUrl = @ExternalRadiusServerUrl, " +
                                              "ExternalRadiusServerPort = @ExternalRadiusServerPort, " +
                                              "ExternalRadiusServerSharedSecret = @ExternalRadiusServerSharedSecret";

            var param = new List<SQLiteParameter>()
            {
                new SQLiteParameter("@HttpProxy", newSettings.HttpProxy),
                new SQLiteParameter("@HttpProxyPort", newSettings.HttpProxyPort),
                new SQLiteParameter("@SslProxy", newSettings.SslProxy),
                new SQLiteParameter("@SslProxyPort", newSettings.SslProxyPort),
                new SQLiteParameter("@TunnelPlus", newSettings.TunnelPlus),
                new SQLiteParameter("@TunnelPlusPort", newSettings.TunnelPlusPort),
                new SQLiteParameter("@FtpProxy", newSettings.FtpProxy),
                new SQLiteParameter("@FtpProxyPort", newSettings.FtpProxyPort),
                new SQLiteParameter("@SocksProxy", newSettings.SocksProxy),
                new SQLiteParameter("@SocksProxyPort", newSettings.SocksProxyPort),
                new SQLiteParameter("@pSocks", newSettings.pSocks),
                new SQLiteParameter("@pSocksPort", newSettings.pSocksPort),
                new SQLiteParameter("@tSocks", newSettings.tSocks),
                new SQLiteParameter("@tSocksPort", newSettings.tSocksPort),
                new SQLiteParameter("@OpenVPN", newSettings.OpenVPN),
                new SQLiteParameter("@ServerName", newSettings.ServerName),
                new SQLiteParameter("@OpenVpnUdp", newSettings.OpenVpnUdp),
                new SQLiteParameter("@OpenVpnUdpPort", newSettings.OpenVpnUdpPort),
                new SQLiteParameter("@OpenVpnTcp", newSettings.OpenVpnTcp),
                new SQLiteParameter("@OpenVpnTcpPort", newSettings.OpenVpnTcpPort),
                new SQLiteParameter("@SslVpn", newSettings.SslVpn),
                new SQLiteParameter("@SslVpnPort", newSettings.SslVpnPort),
                new SQLiteParameter("@VpnPlus", newSettings.VpnPlus),
                new SQLiteParameter("@VpnPlusPort", newSettings.VpnPlusPort),
                new SQLiteParameter("@EnableInternalRadiusServer", newSettings.EnableInternalRadiusServer),
                new SQLiteParameter("@InternalRadiusServerUrl", newSettings.InternalRadiusServerUrl),
                new SQLiteParameter("@InternalRadiusServerPort", newSettings.InternalRadiusServerPort),
                new SQLiteParameter("@InternalRadiusServerSharedSecret", newSettings.InternalRadiusServerSharedSecret),
                new SQLiteParameter("@EnableExternalRadiusServer", newSettings.EnableExternalRadiusServer),
                new SQLiteParameter("@ExternalRadiusServerUrl", newSettings.ExternalRadiusServerUrl),
                new SQLiteParameter("@ExternalRadiusServerPort", newSettings.ExternalRadiusServerPort),
                new SQLiteParameter("@ExternalRadiusServerSharedSecret", newSettings.ExternalRadiusServerSharedSecret)
            };

            if (Run(param, sql) == true)
            {
                CacheManager.RemoveDataFromCache(SettingsCacheKey);
                return true;
            }

            return false;
        }

        private string MakeUpdateSql(object obj)
        {
            var result = "UPDATE tbl_Settings SET ";
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                result += prop.Name + " = @" + prop.Name + " ";
            }
            return result;
        }
    }

}

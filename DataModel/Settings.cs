using System.Collections.Generic;
using System.ComponentModel;

namespace DoctorProxy.DataModel
{
    public class Settings : INotifyPropertyChanged , INotifyDataErrorInfo
    {
        private bool _HttpProxy;
        public bool HttpProxy
        {
            get { return _HttpProxy; }
            set
            {
                _HttpProxy = value;
                if (value == false)
                {
                    SslProxy = false;
                    TunnelPlus = false;
                }
                OnPropertyChanged(new PropertyChangedEventArgs("HttpProxy"));
            }
        }

        private int? _HttpProxyPort;
        public int? HttpProxyPort
        {
            get { return _HttpProxyPort; }
            set
            {
                _HttpProxyPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HttpProxyPort"));
            }
        }


        private bool _SslProxy;
        public bool SslProxy
        {
            get { return _SslProxy; }
            set
            {
                _SslProxy = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SslProxy"));
            }
        }

        private int? _SslProxyPort;
        public int? SslProxyPort
        {
            get { return _SslProxyPort; }
            set
            {
                _SslProxyPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SslProxyPort"));
            }
        }

        private bool _TunnelPlus;
        public bool TunnelPlus
        {
            get { return _TunnelPlus; }
            set
            {
                _TunnelPlus = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TunnelPlus"));
            }
        }

        private int? _TunnelPlusPort;
        public int? TunnelPlusPort
        {
            get { return _TunnelPlusPort; }
            set
            {
                _TunnelPlusPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TunnelPlusPort"));
            }
        }

        //_________________________________________________________ FTP Proxy ________________________________________________________________

        private bool _FtpProxy;
        public bool FtpProxy
        {
            get { return _FtpProxy; }
            set
            {
                _FtpProxy = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FtpProxy"));
            }
        }

        private int? _FtpProxyPort;
        public int? FtpProxyPort
        {
            get { return _FtpProxyPort; }
            set
            {
                _FtpProxyPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FtpProxyPort"));
            }
        }

        //_______________________________________________________ Socks __________________________________________________________________

        private bool _SocksProxy;
        public bool SocksProxy
        {
            get { return _SocksProxy; }
            set
            {
                _SocksProxy = value;
                if (value == false)
                {
                    pSocks = false;
                    tSocks = false;
                }
                OnPropertyChanged(new PropertyChangedEventArgs("SocksProxy"));
            }
        }

        private int? _SocksProxyPort;
        public int? SocksProxyPort
        {
            get { return _SocksProxyPort; }
            set
            {
                _SocksProxyPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SocksProxyPort"));
            }
        }

        private bool _pSocks;
        public bool pSocks
        {
            get { return _pSocks; }
            set
            {
                _pSocks = value;
                OnPropertyChanged(new PropertyChangedEventArgs("pSocks"));
            }
        }

        private int? _pSocksPort;
        public int? pSocksPort
        {
            get { return _pSocksPort; }
            set
            {
                _pSocksPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("pSocksPort"));
            }
        }

        private bool _tSocks;
        public bool tSocks
        {
            get { return _tSocks; }
            set
            {
                _tSocks = value;
                OnPropertyChanged(new PropertyChangedEventArgs("tSocks"));
            }
        }

        private int? _tSocksPort;
        public int? tSocksPort
        {
            get { return _tSocksPort; }
            set
            {
                _tSocksPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("tSocksPort"));
            }
        }

        //_____________________________________________________ OPEN VPN ____________________________________________________________________

        private bool _OpenVPN;
        public bool OpenVPN
        {
            get { return _OpenVPN; }
            set
            {
                _OpenVPN = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OpenVPN"));
            }
        }

        private string _ServerName;
        public string ServerName
        {
            get { return _ServerName; }
            set
            {
                _ServerName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ServerName"));
            }
        }

        private bool _OpenVpnUdp;
        public bool OpenVpnUdp
        {
            get { return _OpenVpnUdp; }
            set
            {
                _OpenVpnUdp = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OpenVpnUdp"));
            }
        }

        private int? _OpenVpnUdpPort;
        public int? OpenVpnUdpPort
        {
            get { return _OpenVpnUdpPort; }
            set
            {
                _OpenVpnUdpPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OpenVpnUdpPort"));
            }
        }

        private bool _OpenVpnTcp;
        public bool OpenVpnTcp
        {
            get { return _OpenVpnTcp; }
            set
            {
                _OpenVpnTcp = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OpenVpnTcp"));
            }
        }

        private int? _OpenVpnTcpPort;
        public int? OpenVpnTcpPort
        {
            get { return _OpenVpnTcpPort; }
            set
            {
                _OpenVpnTcpPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OpenVpnTcpPort"));
            }
        }

        private bool _SslVpn;
        public bool SslVpn
        {
            get { return _SslVpn; }
            set
            {
                _SslVpn = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SslVpn"));
            }
        }

        private int? _SslVpnPort;
        public int? SslVpnPort
        {
            get { return _SslVpnPort; }
            set
            {
                _SslVpnPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SslVpnPort"));
            }
        }

        private bool _VpnPlus;
        public bool VpnPlus
        {
            get { return _VpnPlus; }
            set
            {
                _VpnPlus = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VpnPlus"));
            }
        }

        private int? _VpnPlusPort;
        public int? VpnPlusPort
        {
            get { return _VpnPlusPort; }
            set
            {
                _VpnPlusPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VpnPlusPort"));
            }
        }

        //______________________________________________________________ Internal Radius Server ___________________________________________________________

        private bool _EnableInternalRadiusServer;
        public bool EnableInternalRadiusServer
        {
            get { return _EnableInternalRadiusServer; }
            set
            {
                _EnableInternalRadiusServer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("EnableInternalRadiusServer"));
            }
        }

        private string _InternalRadiusServerUrl;
        public string InternalRadiusServerUrl
        {
            get { return _InternalRadiusServerUrl; }
            set
            {
                _InternalRadiusServerUrl = value;
                OnPropertyChanged(new PropertyChangedEventArgs("InternalRadiusServerUrl"));
            }
        }

        private int? _InternalRadiusServerPort;
        public int? InternalRadiusServerPort
        {
            get { return _InternalRadiusServerPort; }
            set
            {
                _InternalRadiusServerPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("InternalRadiusServerPort"));
            }
        }

        private string _InternalRadiusServerSharedSecret;
        public string InternalRadiusServerSharedSecret
        {
            get { return _InternalRadiusServerSharedSecret; }
            set
            {
                _InternalRadiusServerSharedSecret = value;
                OnPropertyChanged(new PropertyChangedEventArgs("InternalRadiusServerSharedSecret"));
            }
        }

        //______________________________________________________________ External Radius Server ___________________________________________________________

        private bool _EnableExternalRadiusServer;
        public bool EnableExternalRadiusServer
        {
            get { return _EnableExternalRadiusServer; }
            set
            {
                _EnableExternalRadiusServer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("EnableExternalRadiusServer"));
            }
        }

        private string _ExternalRadiusServerUrl;
        public string ExternalRadiusServerUrl
        {
            get { return _ExternalRadiusServerUrl; }
            set
            {
                _ExternalRadiusServerUrl = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ExternalRadiusServerUrl"));
            }
        }

        private int? _ExternalRadiusServerPort;
        public int? ExternalRadiusServerPort
        {
            get { return _ExternalRadiusServerPort; }
            set
            {
                _ExternalRadiusServerPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ExternalRadiusServerPort"));
            }
        }

        private string _ExternalRadiusServerSharedSecret;
        public string ExternalRadiusServerSharedSecret
        {
            get { return _ExternalRadiusServerSharedSecret; }
            set
            {
                _ExternalRadiusServerSharedSecret = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ExternalRadiusServerSharedSecret"));
            }
        }

        //______________________________________________________________ Accounting ___________________________________________________________

        private AuthenticationTypes _AuthenticationType = AuthenticationTypes.AllowAll;
        public AuthenticationTypes AuthenticationType
        {
            get { return _AuthenticationType; }
            set
            {
                _AuthenticationType = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AuthenticationType"));
            }
        }

        //_________________________________________________________________________________________________________________________________________________

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        //_________________________________________________________________________________________________________________________________________________

        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        public event System.EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                // Provide all the error collections.
                return (errors.Values);
            }
            else
            {
                // Provice the error collection for the requested property
                // (if it has errors).
                if (errors.ContainsKey(propertyName))
                {
                    return (errors[propertyName]);
                }
                else
                {
                    return null;
                }
            }
        }

        public bool HasErrors
        {
            get
            {
                // Indicate whether the entire Product object is error-free.
                return (errors.Count > 0);
            }
        }

        private void SetErrors(string propertyName, List<string> propertyErrors)
        {
            // Clear any errors that already exist for this property.
            errors.Remove(propertyName);
            // Add the list collection for the specified property.
            errors.Add(propertyName, propertyErrors);
            // Raise the error-notification event.
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void ClearErrors(string propertyName)
        {
            // Remove the error list for this property.
            errors.Remove(propertyName);
            // Raise the error-notification event.
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void Validate()
        {

        }
    }
}

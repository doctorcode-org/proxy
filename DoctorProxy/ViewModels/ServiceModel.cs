using DoctorProxy.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.ViewModels
{
    public class ServiceModel : INotifyPropertyChanged
    {
        private string _ServiceName = "DoctorProxyService";
        
        public event PropertyChangedEventHandler PropertyChanged;

        public ServiceModel()
        {
            var svcManager = new Manager();
            _Installed = svcManager.IsServiceExist(_ServiceName);
            _IsRunning = (svcManager.GetServiceState(_ServiceName) == ServiceStatus.Running);
        }

        private bool _Installed;
        public bool Installed
        {
            get { return _Installed; }
            set
            {
                _Installed = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Installed"));
            }
        }

        private bool _IsRunning;
        public bool IsRunning
        {
            get { return _IsRunning; }
            set
            {
                _IsRunning = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsRunning"));
            }
        }

        public string ServiceName
        {
            get { return _ServiceName; }
        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }
}

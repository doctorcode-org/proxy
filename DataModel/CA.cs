using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.DataModel
{
    public class CA : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;


        private string _CommonName;
        public string CommonName
        {
            get { return _CommonName; }
            set
            {
                _CommonName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CommonName"));
            }
        }

        private string _Organization;
        public string Organization
        {
            get { return _Organization; }
            set
            {
                _Organization = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Organization"));
            }
        }

        private string _OrganizationUnit;
        public string OrganizationUnit
        {
            get { return _OrganizationUnit; }
            set
            {
                _OrganizationUnit = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OrganizationUnit"));
            }
        }

        private string _Country;
        public string Country
        {
            get { return _Country; }
            set
            {
                _Country = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Country"));
            }
        }

        private string _State;
        public string State
        {
            get { return _State; }
            set
            {
                _State = value;
                OnPropertyChanged(new PropertyChangedEventArgs("State"));
            }
        }

        private string _Locale;
        public string Locale
        {
            get { return _Locale; }
            set
            {
                _Locale = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Locale"));
            }
        }

        private int _ExpireDays = 3650;
        public int ExpireDays
        {
            get { return _ExpireDays; }
            set
            {
                _ExpireDays = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ExpireDays"));
            }
        }

        private int _KeyLenght = 2048;
        public int KeyLenght
        {
            get { return _KeyLenght; }
            set
            {
                if (value < 512 || value > 4096 || (value % 4 != 0))
                {
                    SetErrors("KeyLenght", new List<string>() { "طول کلید رمزنگاری معتبر نیست." });
                    return;
                }

                _KeyLenght = value;
                OnPropertyChanged(new PropertyChangedEventArgs("KeyLenght"));
            }
        }


        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        //_________________________________________________________________________________________________________________________________________________

        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return (errors.Values);

            if (errors.ContainsKey(propertyName))
                return (errors[propertyName]);

            return null;
        }

        public bool HasErrors
        {
            get { return (errors.Count > 0); }
        }

        private void SetErrors(string propertyName, List<string> propertyErrors)
        {
            errors.Remove(propertyName);
            errors.Add(propertyName, propertyErrors);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void ClearErrors(string propertyName)
        {
            errors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }


        public IEnumerable Validate()
        {
            if (string.IsNullOrEmpty(CommonName))
                yield return "CN را وارد کنید.";

            if (KeyLenght < 512 || KeyLenght > 4096)
                yield return "طول کلید رمزنگاری معتبر نیست.";


        }

    }
}

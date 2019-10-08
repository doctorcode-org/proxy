using System;
using System.Linq;
using System.Windows.Data;

namespace DoctorProxy.Converters
{
    public class OrConditionalMultiBinding : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return values.Any(v => (bool)v == true);
            }
            catch (Exception)
            {
                return false;
            }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

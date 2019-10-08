using System;
using System.Windows.Data;

namespace DoctorProxy.Converters
{
    public class NullableValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            else if (value is string)
                return value;
            else
                return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                var s = (string)value;
                int result;

                if (int.TryParse(s, out result))
                    return result;
                else
                    return null;
            }

            return value;
        }
    }
}

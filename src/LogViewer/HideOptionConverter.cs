using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogViewer
{
    internal class HideOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bv)
            {
                return bv ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vb)
            {
                return vb == Visibility.Collapsed;
            }

            return false;
        }
    }
}

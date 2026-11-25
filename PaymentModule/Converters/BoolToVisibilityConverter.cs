using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace PaymentModule.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        //показывает элемент только при истинном условии
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool flag && flag)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}

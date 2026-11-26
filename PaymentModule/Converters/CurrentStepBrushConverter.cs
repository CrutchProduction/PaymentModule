using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace PaymentModule.Converters
{
    public class CurrentStepBrushConverter : IValueConverter
    {
        //подсвечивает текущий шаг в истории
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool current && current)
            {
                return new SolidColorBrush(Color.FromArgb(45, 255, 176, 32));
            }
            return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

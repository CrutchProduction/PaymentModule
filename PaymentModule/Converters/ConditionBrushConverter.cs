using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace PaymentModule.Converters
{
    public class ConditionBrushConverter : IValueConverter
    {
        /// <summary>Выбирает цвет квадрата по результату проверки.</summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Зелёный означает, что условие выполнено.
            if (value is bool condition && condition)
            {
                return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Red);
        }

        /// <summary>Запрещает обратное преобразование цвета в условие.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

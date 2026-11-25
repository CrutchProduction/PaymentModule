using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PaymentModule.ViewModels;
using System;
using Windows.UI;

namespace PaymentModule.Converters
{
    public class CellStateBrushConverter : IValueConverter
    {
        //цвет ячейки массива зависит от её состояния
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CellState state)
            {
                //текущий индекс j
                if (state == CellState.Current)
                {
                    return new SolidColorBrush(Color.FromArgb(80, 255, 176, 32));
                }
                //ячейка, прочитанная на последнем шаге
                if (state == CellState.Changed)
                {
                    return new SolidColorBrush(Color.FromArgb(70, 74, 222, 128));
                }
                //ячейка уже вошла в префикс
                if (state == CellState.InPrefix)
                {
                    return new SolidColorBrush(Color.FromArgb(28, 74, 222, 128));
                }
            }
            return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PaymentModule.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>Сообщает интерфейсу об изменении свойства.</summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Пустое имя обновляет все свойства этой модели.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace PaymentModule.ViewModels
{
    public class ProofRowViewModel : ViewModelBase
    {
        private string statusText = "?";
        private SolidColorBrush statusBrush = new SolidColorBrush(Colors.Gray);

        public string Title { get; set; } = "";
        public string Formula { get; set; } = "";
        public string Explanation { get; set; } = "";

        public string StatusText
        {
            get { return statusText; }
            set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush StatusBrush
        {
            get { return statusBrush; }
            set
            {
                statusBrush = value;
                OnPropertyChanged();
            }
        }
    }
}

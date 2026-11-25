using Microsoft.UI.Xaml.Controls;
using PaymentModule.ViewModels;

namespace PaymentModule.Views
{
    public sealed partial class LoopLabView : UserControl
    {
        public LoopLabView()
        {
            InitializeComponent();
            ViewModel = new LoopLabViewModel();
            DataContext = ViewModel;
        }

        public LoopLabViewModel ViewModel { get; }
    }
}

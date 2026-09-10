using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PaymentModule
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //скрыть панели
            Panel1.Visibility = Visibility.Collapsed;
            Panel2.Visibility = Visibility.Collapsed;
            Panel3.Visibility = Visibility.Collapsed;
            Panel4.Visibility = Visibility.Collapsed;

            //показываем
            switch (NavList.SelectedIndex)
            {
                case 0: Panel1.Visibility = Visibility.Visible; break;
                case 1: Panel2.Visibility = Visibility.Visible; break;
                case 2: Panel3.Visibility = Visibility.Visible; break;
                case 3: Panel4.Visibility = Visibility.Visible; break;
            }
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        public static bool contractIsOpen = false;
        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //скрыть панели
            Panel1.Visibility = Visibility.Collapsed;
            Panel2.Visibility = Visibility.Collapsed;
            Panel3.Visibility = Visibility.Collapsed;
            Panel4.Visibility = Visibility.Collapsed;
            //скрываем контракты
            contract1.Visibility = Visibility.Collapsed;
            contract2.Visibility = Visibility.Collapsed;
            contract3.Visibility = Visibility.Collapsed;
            contract4.Visibility = Visibility.Collapsed;

            contractV1.Visibility = Visibility.Collapsed;
            contractV2.Visibility = Visibility.Collapsed;
            contractV3.Visibility = Visibility.Collapsed;
            contractV4.Visibility = Visibility.Collapsed;

            PanelV1.Visibility = Visibility.Collapsed;
            PanelV2.Visibility = Visibility.Collapsed;
            PanelV3.Visibility = Visibility.Collapsed;
            PanelV4.Visibility = Visibility.Collapsed;



            panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            contractIsOpen = false;
            
            //показываем
            switch (NavList.SelectedIndex)
            {
                case 0: 
                    Panel1.Visibility = Visibility.Visible;
                    PanelV1.Visibility = Visibility.Visible;
                    break;
                case 1: 
                    Panel2.Visibility = Visibility.Visible;
                    PanelV2.Visibility = Visibility.Visible;
                    break;
                case 2: 
                    Panel3.Visibility = Visibility.Visible;
                    PanelV3.Visibility = Visibility.Visible;
                    break;
                case 3: 
                    Panel4.Visibility = Visibility.Visible;
                    PanelV4.Visibility = Visibility.Visible; 
                    break;
            }
        }

        private void contract1_Click(object sender, RoutedEventArgs e)
        {
            if (contractIsOpen)
            {
                //скрываем контракт        
                contract1.Visibility = Visibility.Collapsed;
                contractV1.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }else
            {
                //показываем контракт       
                contract1.Visibility = Visibility.Visible;
                contractV1.Visibility = Visibility.Visible;
                panelContract.Width = new GridLength(1.5, GridUnitType.Star);
            }
            contractIsOpen = contractIsOpen == false;
        }

        private void contract2_Click(object sender, RoutedEventArgs e)
        {
            if (contractIsOpen)
            {
                //скрываем контракт        
                contract2.Visibility = Visibility.Collapsed;
                contractV2.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                //показываем контракт       
                contract2.Visibility = Visibility.Visible;
                contractV2.Visibility= Visibility.Visible;
                panelContract.Width = new GridLength(1.5, GridUnitType.Star);
            }
            contractIsOpen = contractIsOpen == false;
        }


        private void contract3_Click(object sender, RoutedEventArgs e)
        {
            if (contractIsOpen)
            {
                //скрываем контракт        
                contract3.Visibility = Visibility.Collapsed;
                contractV3.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                //показываем контракт       
                contract3.Visibility = Visibility.Visible;
                contractV3.Visibility = Visibility.Visible;
                panelContract.Width = new GridLength(1.5, GridUnitType.Star);
            }
            contractIsOpen = contractIsOpen == false;
        }


        private void contract4_Click(object sender, RoutedEventArgs e)
        {
            if (contractIsOpen)
            {
                //скрываем контракт        
                contract4.Visibility = Visibility.Collapsed;
                contractV4.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                //показываем контракт       
                contract4.Visibility = Visibility.Visible;
                contractV4.Visibility = Visibility.Visible;
                panelContract.Width = new GridLength(1.5, GridUnitType.Star);
            }
            contractIsOpen = contractIsOpen == false;
        }

    }
}

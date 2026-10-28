using PaymentModule.ViewModels;
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
using PaymentClassLibrary.Managers;
using PaymentClassLibrary;

namespace PaymentModule
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RootGrid.Loaded += RootGrid_Loaded;
            NavList.SelectedIndex = 0;
        }
        private bool contractIsOpen = false;
        private bool aboutAppIsOpen = true;
        private bool initialized;

        /// <summary>Загружает счета после создания окна.</summary>
        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (initialized) { return; }
            initialized = true;
            try
            {
                AccountsManager.LoadAccount();
                MoneyManager.LoadMoneyScales();
                currency.Items.Clear();
                foreach (string moneyName in MoneyManager.GetMoneyNames())
                {
                    currency.Items.Add(moneyName);
                }
                MainViewModel viewModel = new MainViewModel(AccountsManager.FindAccountById(MainViewModel.CurrentAccountId));
                viewModel.ErrorOccurred += ShowError;
                RootGrid.DataContext = viewModel;
                Logger.StartNewLog();
            }
            catch (Exception error)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = RootGrid.XamlRoot;
                dialog.Title = "Ошибка";
                dialog.Content = error.Message;
                dialog.CloseButtonText = "ОК";
                await dialog.ShowAsync();
            }
        }

        /// <summary>Показывает сообщение об ошибке операции.</summary>
        private async void ShowError(string message)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = RootGrid.XamlRoot;
            dialog.Title = "Ошибка";
            dialog.Content = message;
            dialog.CloseButtonText = "ОК";
            await dialog.ShowAsync();
        }

        /// <summary>Переключает видимую панель операции.</summary>
        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Panel1.Visibility = Visibility.Collapsed;
            Panel2.Visibility = Visibility.Collapsed;
            Panel3.Visibility = Visibility.Collapsed;
            Panel4.Visibility = Visibility.Collapsed;
            CalculatorPanel.Visibility = Visibility.Collapsed;
            listboxitem5.IsEnabled = true;

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

            switch (NavList.SelectedIndex)
            {
                case 0:
                    Panel1.Visibility = Visibility.Visible;
                    PanelV1.Visibility = Visibility.Visible;
                    listboxitem1.IsEnabled = false;
                    listboxitem2.IsEnabled = true;
                    listboxitem3.IsEnabled = true;
                    listboxitem4.IsEnabled = true;
                    break;
                case 1:
                    Panel2.Visibility = Visibility.Visible;
                    PanelV2.Visibility = Visibility.Visible;
                    listboxitem1.IsEnabled = true;
                    listboxitem2.IsEnabled = false;
                    listboxitem3.IsEnabled = true;
                    listboxitem4.IsEnabled = true;
                    break;
                case 2:
                    Panel3.Visibility = Visibility.Visible;
                    PanelV3.Visibility = Visibility.Visible;
                    listboxitem1.IsEnabled = true;
                    listboxitem2.IsEnabled = true;
                    listboxitem3.IsEnabled = false;
                    listboxitem4.IsEnabled = true;
                    break;
                case 3:
                    Panel4.Visibility = Visibility.Visible;
                    PanelV4.Visibility = Visibility.Visible;
                    listboxitem1.IsEnabled = true;
                    listboxitem2.IsEnabled = true;
                    listboxitem3.IsEnabled = true;
                    listboxitem4.IsEnabled = false;
                    break;
                case 4:
                    CalculatorPanel.Visibility = Visibility.Visible;
                    listboxitem1.IsEnabled = true;
                    listboxitem2.IsEnabled = true;
                    listboxitem3.IsEnabled = true;
                    listboxitem4.IsEnabled = true;
                    listboxitem5.IsEnabled = false;
                    break;
            }
        }

        private void contract1_Click(object sender, RoutedEventArgs e)
        {
            if (contractIsOpen)
            {
                contract1.Visibility = Visibility.Collapsed;
                contractV1.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
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
                contract2.Visibility = Visibility.Collapsed;
                contractV2.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                contract2.Visibility = Visibility.Visible;
                contractV2.Visibility = Visibility.Visible;
                panelContract.Width = new GridLength(1.5, GridUnitType.Star);
            }
            contractIsOpen = contractIsOpen == false;
        }

        private void contract3_Click(object sender, RoutedEventArgs e)
        {
            if (contractIsOpen)
            {
                contract3.Visibility = Visibility.Collapsed;
                contractV3.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
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
                contract4.Visibility = Visibility.Collapsed;
                contractV4.Visibility = Visibility.Collapsed;
                panelContract.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                contract4.Visibility = Visibility.Visible;
                contractV4.Visibility = Visibility.Visible;
                panelContract.Width = new GridLength(1.5, GridUnitType.Star);
            }
            contractIsOpen = contractIsOpen == false;
        }

        private async void AboutApp_Click(object sender, RoutedEventArgs e)
        {
            if (aboutAppIsOpen)
            {
                General.Visibility = Visibility.Collapsed;
                About.Visibility = Visibility.Visible;
                aboutAppIsOpen = false;
            }
            else
            {
                About.Visibility = Visibility.Collapsed;
                General.Visibility = Visibility.Visible;
                aboutAppIsOpen = true;
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            AccountsManager.ImportData();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            AccountsManager.ExportData();
        }

        private void Button_ClickPanel3(object sender, RoutedEventArgs e)
        {
            balance.Text = ((Button)sender).Content.ToString().Replace(" ", "");
        }

        private void Button_ClickPanel4(object sender, RoutedEventArgs e)
        {
            withDrawMoney.Text = ((Button)sender).Content.ToString().Replace(" ", "");
        }

        private void withdrawMoney_Clik(object sender, RoutedEventArgs e)
        {
            withDrawMoney.Text = amount.Text;
        }

        // Кнопка "Max из двух" — просто заполняет поля готовым примером.
        private void MaxBtn_Click(object sender, RoutedEventArgs e)
        {
            CodeBox.Text = "if (x1 >= x2) { max := x1; } else { max := x2; }";
            PostBox.Text = "max >= x1 && max >= x2";
            ClearOutputs();
        }

        // Кнопка "Квадратное уравнение".
        private void QuadraticBtn_Click(object sender, RoutedEventArgs e)
        {
            CodeBox.Text = "if (D >= 0 && a != 0) { x1 := (-b + sqrt(D)) / (2 * a); } else { x1 := -999; }";
            PostBox.Text = "(D >= 0 && a != 0 && a * x1 * x1 + b * x1 + c == 0) || ((D < 0 || a == 0) && x1 == -999)";
            ClearOutputs();
        }

        // Кнопка "Последовательность присваиваний".
        private void SequenceBtn_Click(object sender, RoutedEventArgs e)
        {
            CodeBox.Text = "x := x + 10; y := x + 1;";
            PostBox.Text = "y == x + 1 && x > 15";
            ClearOutputs();
        }

        // Стирает все поля вывода — чтобы не путаться между запусками.
        private void ClearOutputs()
        {
            StepsBox.Text = "";
            WpBox.Text = "";
            WpWordsBox.Text = "";
            DefinedBox.Text = "";
            HoareBox.Text = "";
        }

        // Кнопка "Рассчитать wp" — зовёт движок и раскладывает ответ по полям.
        private void CalculateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Одна строка — вызов движка. Внутри он вернёт сразу 4 строки.
                WpResult r = WpEngine.Analyze(CodeBox.Text, PostBox.Text);

                StepsBox.Text = r.Steps;                // пошаговые преобразования
                WpBox.Text = r.Wp;                   // краткая запись
                WpWordsBox.Text = "Формула: " + r.Wp;     // словами
                DefinedBox.Text = r.Defined;              // условия определённости
                HoareBox.Text = r.Hoare;                // триада Хоара
            }
            catch (Exception error)
            {
                ClearOutputs();
                StepsBox.Text = "Ошибка: " + error.Message;
            }
        }
    }
}
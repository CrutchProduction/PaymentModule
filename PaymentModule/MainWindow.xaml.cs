using PaymentModule.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using PaymentClassLibrary.Managers;
using PaymentClassLibrary;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PaymentModule
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>Подключает модели операций и выбирает первую панель.</summary>
        public MainWindow()
        {
            InitializeComponent();
            // Поля счета заполняются один раз и недоступны для редактирования.
            RootGrid.Loaded += RootGrid_Loaded;
            ScenarioPicker.SelectedIndex = 0;
            LaboratoryTabs.SelectedIndex = 0;
            NavList.SelectedIndex = 0;
        }
        private bool contractIsOpen = false;
        private bool aboutAppIsOpen;
        private bool initialized;
        private readonly HashSet<ButtonBase> laserButtons = new();
        private Storyboard? laserAnimation;
        private bool photoEffectsVisible = false;

        private void TogglePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            if (photoEffectsVisible)
            {
                photoEffectsVisible = false;
                PhotoPanel.Visibility = Visibility.Collapsed;
                CalculationResultsPanel.Visibility = Visibility.Visible;
                laserAnimation?.Stop();
                LaserOverlay.Children.Clear();
            }
            else
            {
                photoEffectsVisible = true;
                CalculationResultsPanel.Visibility = Visibility.Collapsed;
                PhotoPanel.Visibility = Visibility.Visible;
            }
        }

        private void ConnectLaserButtons(DependencyObject parent)
        {
            if (parent is ButtonBase button && laserButtons.Add(button))
            {
                button.Click += LaserButton_Click;
            }
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                ConnectLaserButtons(VisualTreeHelper.GetChild(parent, index));
            }
        }

        private void LaserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ButtonBase button || button == TogglePhotoButton ||
                !photoEffectsVisible || aboutAppIsOpen ||
                LaboratoryTabs.SelectedIndex != 1 || CalculatorPhoto.ActualWidth <= 0 ||
                CalculatorPhoto.ActualHeight <= 0 || CalculatorPhoto.Source is not BitmapSource photo ||
                photo.PixelWidth <= 0 || photo.PixelHeight <= 0) return;

            laserAnimation?.Stop();
            LaserOverlay.Children.Clear();

            double scale = Math.Min(CalculatorPhoto.ActualWidth / photo.PixelWidth,
                                    CalculatorPhoto.ActualHeight / photo.PixelHeight);
            double width = photo.PixelWidth * scale;
            double height = photo.PixelHeight * scale;
            double left = (CalculatorPhoto.ActualWidth - width) / 2;
            double top = (CalculatorPhoto.ActualHeight - height) / 2;
            var imageTransform = CalculatorPhoto.TransformToVisual(LaserOverlay);
            var target = button.TransformToVisual(LaserOverlay).TransformPoint(
                new Point(button.ActualWidth / 2, button.ActualHeight / 2));

            // Eye positions in the original photo, independent of its displayed size.
            foreach (double eyeX in new[] { 0.745, 0.874 })
            {
                Point eye = imageTransform.TransformPoint(new Point(left + width * eyeX, top + height * 0.394));
                AddLaserLine(eye, target, 16, 0.18, Microsoft.UI.Colors.Red);
                AddLaserLine(eye, target, 7, 0.9, Microsoft.UI.Colors.Red);
                AddLaserLine(eye, target, 2, 1, Microsoft.UI.Colors.White);
            }

            var fade = new DoubleAnimationUsingKeyFrames();
            fade.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 0 });
            fade.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(30), Value = 1 });
            fade.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(400), Value = 1 });
            fade.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(500), Value = 0 });
            Storyboard.SetTarget(fade, LaserOverlay);
            Storyboard.SetTargetProperty(fade, "Opacity");
            laserAnimation = new Storyboard();
            laserAnimation.Children.Add(fade);
            laserAnimation.Completed += (_, _) => LaserOverlay.Children.Clear();
            laserAnimation.Begin();
        }

        private void AddLaserLine(Point eye, Point target, double thickness, double opacity, Windows.UI.Color color)
        {
            LaserOverlay.Children.Add(new Line
            {
                X1 = eye.X, Y1 = eye.Y, X2 = target.X, Y2 = target.Y,
                Stroke = new SolidColorBrush(color), StrokeThickness = thickness, Opacity = opacity,
                StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
            });
        }

        private async void LaboratoryTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            laserAnimation?.Stop();
            LaserOverlay?.Children.Clear();
            if (LaboratoryOne == null || Calculator == null) return;
            LaboratoryOne.Visibility = LaboratoryTabs.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            Calculator.Visibility = LaboratoryTabs.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            if (LaboratoryTabs?.SelectedIndex == 1 && Calculator?.IsLoaded == true)
            {
                await ShowHelpOnceAsync();
            }
        }

        private async void Calculator_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectLaserButtons(RootGrid);
            if (LaboratoryTabs?.SelectedIndex == 1)
            {
                await ShowHelpOnceAsync();
            }
        }

        private bool helpShown;
        private bool helpOpen;

        private async Task ShowHelpOnceAsync()
        {
            if (helpShown || Calculator.XamlRoot == null) return;
            helpShown = true;
            await ShowHelpAsync();
        }

        private async void Help_Click(object sender, RoutedEventArgs e)
        {
            await ShowHelpAsync();
        }

        private async Task ShowHelpAsync()
        {
            if (helpOpen || Calculator.XamlRoot == null) return;
            helpOpen = true;
            try
            {
                var instructions = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 20,
                    Text = "Для чего нужна вкладка\n" +
                        "Задайте финансовый план и требование к его результату. Калькулятор предусловий предназначен для поиска условий, при которых план достигнет цели.\n\n" +
                        "1. Выберите, что хотите рассчитать. Заполните поля самостоятельно или нажмите «Заполнить пример». Эта кнопка заменяет введённые данные.\n\n" +
                        "2. Для расходов укажите баланс и желаемый остаток. Для перевода также задайте сумму, процент и минимальную комиссию: применяется большее из процента от суммы и минимума. Для накоплений — начальную и желаемую суммы через два года. Деньги со счёта не списываются.\n\n" +
                        "3. Запишите цель (Post): например, balance >= 5000 означает, что после операций должно остаться не менее 5 000 ₽. Добавьте объяснение словами.\n\n" +
                        "4. Введите код. Например, balance = balance - 20000; описывает расход 20 000 ₽. Команды выполнялись бы сверху вниз, а условие выводилось бы с конца.\n\n" +
                        "5. В результате должны появиться требования к исходным данным. Раздел «Как получено условие» предназначен для шагов, wp и триады Хоара.\n\n" +
                        "Пример: аренда 20 000 ₽, закупка 15 000 ₽ и доставка 2 000 ₽ при резерве 5 000 ₽ требуют начального баланса не менее 42 000 ₽.\n\n" +
                        "Сейчас реализован только интерфейс. Расчёт недоступен, значения полей не связаны автоматически. Подсказку можно снова открыть кнопкой «Как пользоваться»."
                };
                var dialog = new ContentDialog
                {
                    XamlRoot = Calculator.XamlRoot,
                    Title = "Как пользоваться калькулятором",
                    CloseButtonText = "Понятно",
                    Content = new ScrollViewer
                    {
                        Content = instructions,
                        MaxHeight = 460,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                    }
                };
                await dialog.ShowAsync();
            }
            finally
            {
                helpOpen = false;
            }
        }

        private void Scenario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigureScenario(false);
        }

        private void FillExample_Click(object sender, RoutedEventArgs e)
        {
            ConfigureScenario(true);
        }

        private void ConfigureScenario(bool fillExample)
        {
            if (CodeInput == null) return;
            ResetCalculationDetails();
            CalculationError.IsOpen = false;
            TransferFields.Visibility = ScenarioPicker.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            ScenarioBalance.Header = "Текущий баланс, ₽";
            ScenarioTarget.Header = "Желаемый остаток, ₽";
            ResultCaption.Text = "Необходимая сумма на счёте";
            DifferenceCaption.Text = "Сколько не хватает";
            DifferenceCaption.Visibility = Visibility.Visible;
            DifferenceValue.Visibility = Visibility.Visible;
            ResultValue.Text = "— ₽";
            DifferenceValue.Text = "— ₽";
            BalanceHint.Text = "Баланс для вашего плана. Вводится вручную и не меняет счёт в шапке приложения.";
            CodeHint.Text = "Каждая строка — действие. Например, balance = balance - 20000; означает расход 20 000 ₽. Готовый образец доступен по кнопке «Заполнить пример». Значения из полей не подставляются в код автоматически.";
            if (fillExample) ScenarioBalance.Text = "1000";
            TargetHint.Text = "Сколько должно остаться после платежей. Условие в поле Post записывается вручную; значения полей не связаны автоматически.";
            switch (ScenarioPicker.SelectedIndex)
            {
                case 0:
                    ScenarioTitle.Text = "Оплатить расходы и сохранить резерв";
                    ScenarioDescription.Text = "План: аренда — 20 000 ₽, закупка — 15 000 ₽, доставка — 2 000 ₽. Задайте, сколько денег должно остаться после всех платежей.";
                    ScenarioVariables.Text = "balance — остаток на счёте; числа в коде — суммы трёх расходов в рублях.";
                    if (fillExample) ScenarioTarget.Text = "500";
                    if (fillExample) PostInput.Text = "balance >= reserve";
                    PostHint.Text = "Для резерва 5 000 ₽ цель записывается как balance >= 5000. Меняя резерв, отредактируйте Post вручную.";
                    if (fillExample) CodeInput.Text = "balance := balance - 100;\nbalance := balance - 200;\nbalance := balance - 300;";
                    break;
                case 1:
                    ScenarioTitle.Text = "Перевести деньги и сохранить остаток";
                    ScenarioDescription.Text = "Укажите перевод и тариф комиссии. Например, 1% от суммы, но не меньше 50 ₽. Для перевода 1 000 ₽ комиссия составит 50 ₽. Чтобы после списания осталось 500 ₽, до перевода нужно иметь минимум 1 550 ₽.";
                    ScenarioVariables.Text = "balance — баланс; amount — перевод; percent — процент комиссии; minFee — минимальная комиссия; fee — большая из двух сумм: процент от перевода или минимум.";
                    TargetHint.Text = "Минимальный остаток после списания перевода вместе с комиссией. Запишите это требование в Post вручную: например, balance >= 500.";
                    CodeHint.Text = "Укажите amount, percent и minFee в коде. Ветвление выбирает большую комиссию, затем из баланса списываются перевод и комиссия. Поля формы не синхронизируются с кодом; расчёт пока не подключён.";
                    if (fillExample)
                    {
                        ScenarioBalance.Text = "1000";
                        ScenarioTarget.Text = "500";
                        TransferAmount.Text = "1000";
                        CommissionPercent.Text = "1";
                        MinimumCommission.Text = "50";
                        PostInput.Text = "balance >= reserve";
                        CodeInput.Text = "var := amount * percent / 100;\nif (var >= minFee) {\n    commission := var;\n} else {\n    commission := minFee;\n};\nbalance := balance - amount;\nbalance := balance - commission;";
                    }
                    PostHint.Text = "Условие на баланс после перевода и комиссии. Например, balance >= 500 означает, что должно остаться хотя бы 500 ₽.";
                    break;
                case 2:
                    ScenarioTitle.Text = "Накопить нужную сумму за два года";
                    ScenarioDescription.Text = "Проценты начисляются раз в год и остаются на счёте. Ставка постоянная, без пополнений и снятий. Пример: 10 000 ₽ должны вырасти до 12 100 ₽ за два года. Подходящая ставка — 10% годовых.";
                    ScenarioVariables.Text = "principal — начальная сумма; target — желаемая сумма; rate — годовая ставка в долях единицы (0.1 = 10%). Уравнение principal*(1 + rate)*(1 + rate) = target является квадратным. D — дискриминант; rate = -1 — специальное значение при отсутствии действительных корней.";
                    ScenarioBalance.Header = "Начальная сумма накоплений, ₽";
                    ScenarioTarget.Header = "Желаемая сумма через два года, ₽";
                    BalanceHint.Text = "Сумма, с которой начинаются накопления. Проценты начисляются ежегодно. Укажите её также в коде как principal; поля не связаны автоматически.";
                    TargetHint.Text = "Сколько вы хотите получить ровно через два года. Укажите эту сумму в коде как target. В примере: из 10 000 ₽ получить 12 100 ₽.";
                    CodeHint.Text = "Код преобразует формулу накоплений в квадратное уравнение и рассматривает корень с неотрицательной ставкой. Калькулятор wp предназначен для проверки условий корректности этого кода. Это учебная модель без комиссий и округлений.";
                    if (fillExample) ScenarioBalance.Text = "10000";
                    if (fillExample) ScenarioTarget.Text = "12100";
                    if (fillExample) PostInput.Text = "(rate >= 0 && balance * (1 + rate) * (1 + rate) == reserve) || (D < 0 && rate == -1)";
                    PostHint.Text = "Цель описывает допустимый результат вычисления ставки. В модели с неотрицательной ставкой начальная сумма должна быть положительной, а желаемая — не меньше начальной. Значение -1 обозначает отсутствие действительных корней; wp также учитывает допустимость деления и квадратного корня.";
                    if (fillExample) CodeInput.Text = "a := balance;\nb := 2 * balance;\nc := balance - reserve;\nD := b * b - 4 * a * c;\nif (D >= 0) {\n    rate := (-b + sqrt(D)) / (2 * a);\n} else {\n    rate := -1;\n}";
                    ResultCaption.Text = "Необходимая годовая ставка";
                    ResultValue.Text = "— %";
                    DifferenceCaption.Visibility = Visibility.Collapsed;
                    DifferenceValue.Visibility = Visibility.Collapsed;
                    break;
            }
        }

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
        private void AccountHeaderLayout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (HeaderTitleGroup == null || HeaderAccountGroup == null || HeaderBalanceGroup == null)
            {
                return;
            }

            bool compact = AccountHeaderLayout.ActualWidth < 1000;
            bool narrow = AccountHeaderLayout.ActualWidth < 600;
            int rows = narrow ? 3 : compact ? 2 : 1;
            while (AccountHeaderLayout.RowDefinitions.Count < rows)
            {
                AccountHeaderLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            while (AccountHeaderLayout.RowDefinitions.Count > rows)
            {
                AccountHeaderLayout.RowDefinitions.RemoveAt(AccountHeaderLayout.RowDefinitions.Count - 1);
            }

            Grid.SetColumnSpan(HeaderTitleGroup, compact ? 3 : 1);
            Grid.SetRow(HeaderAccountGroup, compact ? 1 : 0);
            Grid.SetColumn(HeaderAccountGroup, compact ? 0 : 1);
            Grid.SetColumnSpan(HeaderAccountGroup, narrow ? 3 : compact ? 2 : 1);
            Grid.SetRow(HeaderBalanceGroup, narrow ? 2 : compact ? 1 : 0);
            Grid.SetColumn(HeaderBalanceGroup, narrow ? 0 : 2);
            Grid.SetColumnSpan(HeaderBalanceGroup, narrow ? 3 : 1);
        }

        private void PaymentLayout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PaymentNavigationColumn == null || CompactOperationPicker == null ||
                PaymentOperationPanel == null || PaymentContractPanel == null)
            {
                return;
            }

            bool compact = General.ActualWidth < 900;
            if (compact)
            {
                PaymentNavigationColumn.Width = new GridLength(0);
                BorderListBox.Visibility = Visibility.Collapsed;
                CompactOperationPicker.Visibility = Visibility.Visible;
                Grid.SetColumn(PaymentOperationPanel, 0);
                Grid.SetColumnSpan(PaymentOperationPanel, 2);
                Grid.SetRow(PaymentOperationPanel, 1);
                Grid.SetColumn(PaymentContractPanel, 0);
                Grid.SetColumnSpan(PaymentContractPanel, 2);
                Grid.SetRow(PaymentContractPanel, 2);
            }
            else
            {
                PaymentNavigationColumn.Width = new GridLength(260);
                BorderListBox.Visibility = Visibility.Visible;
                CompactOperationPicker.Visibility = Visibility.Collapsed;
                Grid.SetColumn(PaymentOperationPanel, 1);
                Grid.SetColumnSpan(PaymentOperationPanel, 1);
                Grid.SetRow(PaymentOperationPanel, 0);
                Grid.SetColumn(PaymentContractPanel, 1);
                Grid.SetColumnSpan(PaymentContractPanel, 1);
                Grid.SetRow(PaymentContractPanel, 1);
            }
        }

        private void OperationLayout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not Grid layout || layout.Children.Count != 3)
            {
                return;
            }

            // Форма операции, разделитель и вспомогательная панель.
            FrameworkElement form = (FrameworkElement)layout.Children[0];
            FrameworkElement separator = (FrameworkElement)layout.Children[1];
            FrameworkElement details = (FrameworkElement)layout.Children[2];
            bool sideBySide = layout.ActualWidth >= 860;

            layout.ColumnDefinitions[1].Width = new GridLength(sideBySide ? 1 : 0);
            layout.ColumnDefinitions[2].Width = sideBySide
                ? new GridLength(1.3, GridUnitType.Star)
                : new GridLength(0);
            layout.ColumnSpacing = sideBySide ? 20 : 0;

            Grid.SetColumnSpan(form, sideBySide ? 1 : 3);
            Grid.SetColumn(separator, sideBySide ? 1 : 0);
            Grid.SetRow(separator, sideBySide ? 0 : 1);
            Grid.SetColumnSpan(separator, sideBySide ? 1 : 3);
            Grid.SetColumn(details, sideBySide ? 2 : 0);
            Grid.SetRow(details, sideBySide ? 0 : 2);
            Grid.SetColumnSpan(details, sideBySide ? 1 : 3);

            separator.Width = sideBySide ? 1 : double.NaN;
            separator.Height = sideBySide ? double.NaN : 1;
            separator.Margin = sideBySide ? new Thickness(0) : new Thickness(0, 20, 0, 20);
        }

        private void PaymentAmounts_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not Grid amounts)
            {
                return;
            }

            int columns;
            if (amounts.ActualWidth >= 480)
            {
                columns = 3;
            }
            else if (amounts.ActualWidth >= 300)
            {
                columns = 2;
            }
            else
            {
                columns = 1;
            }

            if (amounts.ColumnDefinitions.Count == columns)
            {
                return;
            }

            amounts.ColumnDefinitions.Clear();
            for (int column = 0; column < columns; column++)
            {
                amounts.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            amounts.RowDefinitions.Clear();
            int rows = (amounts.Children.Count + columns - 1) / columns;
            for (int row = 0; row < rows; row++)
            {
                amounts.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int index = 0; index < amounts.Children.Count; index++)
            {
                Grid.SetRow((FrameworkElement)amounts.Children[index], index / columns);
                Grid.SetColumn((FrameworkElement)amounts.Children[index], index % columns);
            }
        }

        /// <summary>Переключает видимую панель операции.</summary>
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



            PaymentContractPanel.Visibility = Visibility.Collapsed;
            contractIsOpen = false;
            
            //показываем
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

            }
        }

        private void ToggleContractPanel(FrameworkElement text, FrameworkElement viewer)
        {
            if (contractIsOpen)
            {
                text.Visibility = Visibility.Collapsed;
                viewer.Visibility = Visibility.Collapsed;
                PaymentContractPanel.Visibility = Visibility.Collapsed;
                contractIsOpen = false;
            }
            else
            {
                text.Visibility = Visibility.Visible;
                viewer.Visibility = Visibility.Visible;
                PaymentContractPanel.Visibility = Visibility.Visible;
                contractIsOpen = true;
            }
        }

        private void contract1_Click(object sender, RoutedEventArgs e)
        {
            ToggleContractPanel(contract1, contractV1);
        }

        private void contract2_Click(object sender, RoutedEventArgs e)
        {
            ToggleContractPanel(contract2, contractV2);
        }

        private void contract3_Click(object sender, RoutedEventArgs e)
        {
            ToggleContractPanel(contract3, contractV3);
        }

        private void contract4_Click(object sender, RoutedEventArgs e)
        {
            ToggleContractPanel(contract4, contractV4);
        }

        private void AboutApp_Click(object sender, RoutedEventArgs e)
        {
            if (aboutAppIsOpen)
            {
                About.Visibility = Visibility.Collapsed;
                LaboratoryTabs.Visibility = Visibility.Visible;
                aboutAppIsOpen = false;
            }
            else
            {
                LaboratoryTabs.Visibility = Visibility.Collapsed;
                About.Visibility = Visibility.Visible;
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

        private void ResetCalculationDetails()
        {
            CalculationResultHint.Text = "Заполните поля слева и нажмите «Рассчитать условие».";
            CalculationSteps.Text = "Шаги появятся после расчёта.";
            CalculationPrecondition.Text = string.Empty;
            CalculationDefinedness.Text = "Ограничения появятся после расчёта.";
            CalculationHoare.Text = "Предусловие → Код → Постусловие";
            CalculationDetails.IsExpanded = false;
        }

        private void ShowCalculationDetails(WpResult result)
        {
            CalculationResultHint.Text = "Подробности — в разделе «Как получено условие».";
            CalculationSteps.Text = result.Steps;
            CalculationPrecondition.Text = result.Wp;
            CalculationDefinedness.Text = result.Defined;
            CalculationHoare.Text = result.Hoare;
        }

        private decimal ReadCalculationNumber(TextBox input)
        {
            string label = input.Header?.ToString() ?? "Число";
            if (string.IsNullOrWhiteSpace(input.Text))
            {
                throw new FormatException($"Заполните поле «{label}».");
            }

            string text = input.Text.Trim().Replace(',', '.');
            NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            if (!decimal.TryParse(text, numberStyles, CultureInfo.InvariantCulture, out decimal value) || value < 0)
            {
                throw new FormatException($"В поле «{label}» нужно неотрицательное число.");
            }

            return value;
        }

        private string ReplaceCalculationParameter(string expression, string name, decimal value)
        {
            string pattern = @"\b" + Regex.Escape(name) + @"\b";
            string number = value.ToString(CultureInfo.InvariantCulture);
            return Regex.Replace(expression, pattern, _ => number);
        }

        private void CalculateWP_Click(object sender, RoutedEventArgs e)
        {
            CalculationError.IsOpen = false;

            try
            {
                if (string.IsNullOrWhiteSpace(PostInput.Text))
                {
                    throw new FormatException("Заполните «Цель (Post)» или нажмите «Заполнить пример».");
                }
                if (string.IsNullOrWhiteSpace(CodeInput.Text))
                {
                    throw new FormatException("Введите код расчёта или нажмите «Заполнить пример».");
                }

                decimal balance = ReadCalculationNumber(ScenarioBalance);
                decimal reserve = ReadCalculationNumber(ScenarioTarget);
                string post = ReplaceCalculationParameter(PostInput.Text, "reserve", reserve);
                string code = ReplaceCalculationParameter(CodeInput.Text, "reserve", reserve);

                if (ScenarioPicker.SelectedIndex == 1)
                {
                    decimal amount = ReadCalculationNumber(TransferAmount);
                    decimal percent = ReadCalculationNumber(CommissionPercent);
                    decimal minimumFee = ReadCalculationNumber(MinimumCommission);
                    code = ReplaceCalculationParameter(code, "amount", amount);
                    code = ReplaceCalculationParameter(code, "percent", percent);
                    code = ReplaceCalculationParameter(code, "minFee", minimumFee);
                }

                if (ScenarioPicker.SelectedIndex == 2)
                {
                    if (balance == 0)
                    {
                        throw new FormatException("Начальная сумма накоплений должна быть больше нуля.");
                    }
                    post = ReplaceCalculationParameter(post, "balance", balance);
                    code = ReplaceCalculationParameter(code, "balance", balance);
                }

                WpResult result = WpEngine.Analyze(code, post);
                if (ScenarioPicker.SelectedIndex == 2)
                {
                    double rate = RateExtractor.ExtractRate(result.Wp);
                    if (!double.IsFinite(rate) || rate < 0)
                    {
                        throw new InvalidOperationException("Для этих сумм не удалось получить допустимую неотрицательную ставку.");
                    }
                    ResultValue.Text = (rate * 100).ToString("0.####", CultureInfo.CurrentCulture) + " %";
                }
                else
                {
                    decimal requiredBalance = BalanceExpressionSolver.Solve(result.Wp);
                    ResultValue.Text = requiredBalance.ToString() + "₽";
                    DifferenceValue.Text = (requiredBalance - balance).ToString() + "₽";
                }

                ShowCalculationDetails(result);
            }
            catch (Exception error) when (
                error is FormatException ||
                error is InvalidOperationException ||
                error is NotSupportedException ||
                error is ArithmeticException ||
                error is ArgumentException)
            {
                ResetCalculationDetails();
                ResultValue.Text = "—";
                DifferenceValue.Text = "—";
                CalculationResultHint.Text = "Исправьте данные слева и повторите расчёт.";
                CalculationError.Message = error.Message;
                CalculationError.IsOpen = true;
            }
        }
    }
}


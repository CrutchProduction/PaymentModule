using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace PaymentModule
{
    public sealed partial class CalculatorView : UserControl
    {
        private bool helpShown;
        private bool helpOpen;

        public CalculatorView()
        {
            InitializeComponent();
            ScenarioPicker.SelectedIndex = 0;
        }

        public async Task ShowHelpOnceAsync()
        {
            if (helpShown || XamlRoot == null) return;
            helpShown = true;
            await ShowHelpAsync();
        }

        private async void Help_Click(object sender, RoutedEventArgs e)
        {
            await ShowHelpAsync();
        }

        private async Task ShowHelpAsync()
        {
            if (helpOpen || XamlRoot == null) return;
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
                    XamlRoot = XamlRoot,
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
            TransferFields.Visibility = ScenarioPicker.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            ScenarioBalance.Header = "Текущий баланс, ₽";
            ScenarioTarget.Header = "Желаемый остаток, ₽";
            ResultCaption.Text = "Необходимая сумма на счёте";
            DifferenceCaption.Text = "Сколько не хватает";
            ResultValue.Text = "— ₽";
            DifferenceValue.Text = "— ₽";
            BalanceHint.Text = "Баланс для вашего плана. Вводится вручную и не меняет счёт в шапке приложения.";
            CodeHint.Text = "Каждая строка — действие. Например, balance = balance - 20000; означает расход 20 000 ₽. Готовый образец доступен по кнопке «Заполнить пример». Значения из полей не подставляются в код автоматически.";
            if (fillExample) ScenarioBalance.Text = "30000";
            TargetHint.Text = "Сколько должно остаться после платежей. Условие в поле Post записывается вручную; значения полей не связаны автоматически.";
            switch (ScenarioPicker.SelectedIndex)
            {
                case 0:
                    ScenarioTitle.Text = "Оплатить расходы и сохранить резерв";
                    ScenarioDescription.Text = "План: аренда — 20 000 ₽, закупка — 15 000 ₽, доставка — 2 000 ₽. Задайте, сколько денег должно остаться после всех платежей.";
                    ScenarioVariables.Text = "balance — остаток на счёте; числа в коде — суммы трёх расходов в рублях.";
                    if (fillExample) ScenarioTarget.Text = "5000";
                    if (fillExample) PostInput.Text = "balance >= 5000";
                    if (fillExample) PostDescription.Text = "После всех платежей на счёте осталось не менее 5 000 ₽.";
                    PostHint.Text = "Для резерва 5 000 ₽ цель записывается как balance >= 5000. Меняя резерв, отредактируйте Post вручную.";
                    if (fillExample) CodeInput.Text = "balance = balance - 20000;\nbalance = balance - 15000;\nbalance = balance - 2000;";
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
                        PostInput.Text = "balance >= 500";
                        PostDescription.Text = "После перевода и списания комиссии осталось не менее 500 ₽.";
                        CodeInput.Text = "amount = 1000;\npercent = 1;\nminFee = 50;\npercentFee = amount * percent / 100;\nif (percentFee >= minFee) {\n    fee = percentFee;\n} else {\n    fee = minFee;\n}\nbalance = balance - amount;\nbalance = balance - fee;";
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
                    if (fillExample) PostInput.Text = "rate == -1 || (rate >= 0 && principal*(1 + rate)*(1 + rate) == target)";
                    if (fillExample) PostDescription.Text = "Получена неотрицательная ставка, обеспечивающая желаемую сумму через два года, либо специальное значение отсутствия корней.";
                    PostHint.Text = "Цель описывает допустимый результат вычисления ставки. В модели с неотрицательной ставкой начальная сумма должна быть положительной, а желаемая — не меньше начальной. Значение -1 обозначает отсутствие действительных корней; wp также учитывает допустимость деления и квадратного корня.";
                    if (fillExample) CodeInput.Text = "principal = 10000;\ntarget = 12100;\na = principal;\nb = 2*principal;\nc = principal - target;\nD = b*b - 4*a*c;\nif (D >= 0) {\n    rate = (-b + sqrt(D)) / (2*a);\n} else {\n    rate = -1;\n}";
                    ResultCaption.Text = "Условия корректного расчёта";
                    DifferenceCaption.Text = "Годовая ставка по примеру";
                    ResultValue.Text = "—";
                    DifferenceValue.Text = "— %";
                    break;
            }
        }
    }
}

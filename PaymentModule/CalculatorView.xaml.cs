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
                        "1. Выберите сценарий. При переключении поля заменяются готовым учебным примером.\n\n" +
                        "2. Укажите баланс и желаемый остаток, а для заказа — бюджет. Это данные сценария; деньги со счёта не списываются.\n\n" +
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
            if (CodeInput == null) return;
            ScenarioTarget.Header = "Желаемый остаток после платежей, ₽";
            ResultCaption.Text = "Необходимая сумма на счёте";
            DifferenceCaption.Text = "Сколько не хватает";
            ScenarioBalance.Text = "30000";
            switch (ScenarioPicker.SelectedIndex)
            {
                case 0:
                    ScenarioTitle.Text = "Оплатить расходы и сохранить резерв";
                    ScenarioDescription.Text = "План: аренда — 20 000 ₽, закупка — 15 000 ₽, доставка — 2 000 ₽. Задайте, сколько денег должно остаться после всех платежей.";
                    ScenarioVariables.Text = "balance — остаток на счёте; числа в коде — суммы трёх расходов в рублях.";
                    ScenarioTarget.Text = "5000";
                    PostInput.Text = "balance >= 5000";
                    PostDescription.Text = "После всех платежей на счёте осталось не менее 5 000 ₽.";
                    PostHint.Text = "Для резерва 5 000 ₽ цель записывается как balance >= 5000. Меняя резерв, отредактируйте Post вручную.";
                    CodeInput.Text = "balance = balance - 20000;\nbalance = balance - 15000;\nbalance = balance - 2000;";
                    break;
                case 1:
                    ScenarioTitle.Text = "Подготовиться к большему из двух расходов";
                    ScenarioDescription.Text = "Ремонт может потребовать 8 000 ₽ или 12 000 ₽. Оплачен будет только один вариант. Нужно предусмотреть больший расход и сохранить резерв.";
                    ScenarioVariables.Text = "expense1 и expense2 — альтернативные расходы; expense — выбранный больший расход; balance — баланс.";
                    ScenarioTarget.Text = "3000";
                    PostInput.Text = "balance >= 3000";
                    PostDescription.Text = "После оплаты более дорогого варианта ремонта осталось не менее 3 000 ₽.";
                    PostHint.Text = "Если планируете оплатить оба расхода, используйте «Серия платежей»: максимум не заменяет сумму.";
                    CodeInput.Text = "expense1 = 8000;\nexpense2 = 12000;\nif (expense1 >= expense2) {\n    expense = expense1;\n} else {\n    expense = expense2;\n}\nbalance = balance - expense;";
                    break;
                case 2:
                    ScenarioTitle.Text = "Проверить расчёт заказа по нестандартному тарифу";
                    ScenarioDescription.Text = "Учебная модель: стоимость заказа зависит от объёма x по формуле a*x*x + b*x + c. Поиск объёма под заданный бюджет приводит к квадратному уравнению. Рассматриваются действительные неотрицательные объёмы.";
                    ScenarioVariables.Text = "a, b, c — коэффициенты тарифа; budget — бюджет; D — дискриминант; x = -1 — специальное значение при отсутствии действительных корней. В примере рассматривается один корень.";
                    ScenarioTarget.Header = "Бюджет заказа, ₽";
                    ScenarioTarget.Text = "10000";
                    PostInput.Text = "x == -1 || (x >= 0 && a*x*x + b*x + c == budget)";
                    PostDescription.Text = "Получен допустимый объём заказа в рамках заданной формулы либо специальное значение отсутствия корней.";
                    PostHint.Text = "Это модель нестандартного тарифа. a должно отличаться от нуля; sqrt(D) допустим при D >= 0. Бюджет в коде меняется вручную.";
                    CodeInput.Text = "budget = 10000;\nD = b*b - 4*a*(c - budget);\nif (D >= 0) {\n    x = (-b + sqrt(D)) / (2*a);\n} else {\n    x = -1;\n}";
                    ResultCaption.Text = "Требование к доступным средствам";
                    DifferenceCaption.Text = "Недостающая сумма для бюджета";
                    break;
            }
        }
    }
}

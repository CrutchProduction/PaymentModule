using PaymentClassLibrary;
using System;

namespace PaymentModule.ViewModels
{
    public class ExchangeViewModel : OperationViewModel
    {
        private int selectedCurrency;
        public int SelectedCurrency
        {
            get { return selectedCurrency; }
            set { selectedCurrency = value; Refresh(); }
        }
        public override bool AccountValid { get { return main.CurrentAccount.GetMoneyAmount() > 0; } }
        public override bool Precondition
        {
            get { return AccountValid && selectedCurrency >= 0 && selectedCurrency < 4 && selectedCurrency != main.CurrentAccount.GetMoneyTypeId(); }
        }

        /// <summary>Создаёт модель обмена валюты.</summary>
        public ExchangeViewModel(MainViewModel main) : base(main) { }

        /// <summary>Пересчитывает баланс по курсам библиотеки.</summary>
        protected override bool Apply()
        {
            Account account = main.CurrentAccount;
            // Получаем ожидаемый баланс из исходной суммы и курса.
            float expected = (float)Math.Round(account.GetMoneyAmount() * Account.GetMoneyScale(account.GetMoneyTypeId()) / Account.GetMoneyScale(selectedCurrency), 2);
            account.ConvertMoneyToAnotherType(selectedCurrency);
            return account.GetMoneyTypeId() == selectedCurrency && account.GetMoneyAmount() == expected && float.IsFinite(expected);
        }
    }
}

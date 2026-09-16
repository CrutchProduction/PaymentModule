using PaymentClassLibrary;
using System;
using System.Diagnostics;
using System.Globalization;

namespace PaymentModule.ViewModels
{
    public abstract class OperationViewModel : ViewModelBase
    {
        protected readonly MainViewModel main;
        private string amountText = "";
        public RelayCommand ExecuteCommand { get; }
        public bool Postcondition { get; private set; }
        public string AccountId { get { return main.CurrentAccount.GetAccountId().ToString(); } }
        public virtual bool AccountValid { get { return main.CurrentAccount != null; } }
        public abstract bool Precondition { get; }
        protected virtual Account RelatedAccount { get { return main.CurrentAccount; } }
        public string AmountText
        {
            get { return amountText; }
            set
            {
                amountText = value;
                Refresh();
            }
        }

        /// <summary>Подключает общую модель и команду операции.</summary>
        protected OperationViewModel(MainViewModel main)
        {
            this.main = main;
            ExecuteCommand = new RelayCommand(Execute, CanExecute);
        }

        /// <summary>Читает положительную сумму с точкой или запятой.</summary>
        protected bool TryGetAmount(out float amount)
        {
            //без бескон, дробных и отрицательных чисел, с точностью до двух знаков после запятой
            amount = 0;
            if (string.IsNullOrWhiteSpace(amountText))
            {
                return false;
            }
            decimal number;
            if (!decimal.TryParse(amountText.Replace(',', '.'), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out number))
            {
                return false;
            }
            if (number <= 0 || decimal.Round(number, 2) != number)
            {
                return false;
            }
            amount = (float)number;
            return float.IsFinite(amount);
        }

        /// <summary>Обновляет проверки сразу после изменения ввода или баланса.</summary>
        public void Refresh()
        {
            // Старый результат не относится к новым параметрам.
            Postcondition = false;
            OnPropertyChanged("");
            ExecuteCommand.Refresh();
        }

        /// <summary>Проверяет доступность выполнения.</summary>
        private bool CanExecute()
        {
            return Precondition;
        }

        /// <summary>Выполняет операцию, сохраняет счета и проверяет результат.</summary>
        private void Execute()
        {
            Account current = main.CurrentAccount;
            Account related = RelatedAccount;
            float oldBalance = current.GetMoneyAmount();
            int oldCurrency = current.GetMoneyTypeId();
            float relatedBalance = related.GetMoneyAmount();
            try
            {
                Guard.Requires(Precondition, "Предусловия операции не выполнены.");
                bool result = Apply();
                Debug.Assert(result, "Постусловие операции не выполнено.");
                Guard.Requires(result, "Постусловие операции не выполнено.");
                AccountsManager.SaveAccounts();
            }
            catch (Exception error)
            {
                // При ошибке сохранения возвращаем исходные значения.
                current.RemoveMoney(current.GetMoneyAmount());
                current.AddMoney(oldBalance);
                current.SetMoneyType(oldCurrency);
                if (related != current)
                {
                    related.RemoveMoney(related.GetMoneyAmount());
                    related.AddMoney(relatedBalance);
                }
                main.Refresh();
                main.ReportError(error.Message);
                return;
            }

            main.Refresh();
            Postcondition = true;
            OnPropertyChanged(nameof(Postcondition));
            try
            {
                Logger.Log($"{GetType().Name}: счёт {AccountId}, баланс {oldBalance} -> {current.GetMoneyAmount()} {current.GetMoneyType()}");
            }
            catch (Exception error)
            {
                // Сохранённую операцию не повторяем из-за ошибки журнала.
                main.ReportError("Операция сохранена, но журнал недоступен: " + error.Message);
            }
        }
        protected abstract bool Apply();
    }
}

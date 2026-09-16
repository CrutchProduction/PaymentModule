using PaymentClassLibrary;
using System;

namespace PaymentModule.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        //Счёт(пользователь) при запуске
        public const int CurrentAccountId = 4252;
        public Account CurrentAccount { get; }
        public TransferViewModel Transfer { get; }
        public ExchangeViewModel Exchange { get; }
        public DepositViewModel Deposit { get; }
        public WithdrawViewModel Withdraw { get; }
        public event Action<string> ErrorOccurred = delegate { };
        public string Balance { get { return CurrentAccount.GetMoneyAmount().ToString("0.00"); } }
        public string Currency { get { return CurrentAccount.GetMoneyType(); } }

        /// <summary>Подключает фиксированный счёт к четырём операциям.</summary>
        public MainViewModel(Account currentAccount)
        {
            Guard.Requires(currentAccount != null, "Счёт не найден.");
            Guard.Requires(currentAccount.GetAccountId() == CurrentAccountId, "Нельзя изменить текущий счёт.");
            CurrentAccount = currentAccount;
            Transfer = new TransferViewModel(this);
            Exchange = new ExchangeViewModel(this);
            Deposit = new DepositViewModel(this);
            Withdraw = new WithdrawViewModel(this);
        }

        /// <summary>Обновляет баланс и проверки во всех панелях.</summary>
        public void Refresh()
        {
            OnPropertyChanged("");
            Transfer.Refresh();
            Exchange.Refresh();
            Deposit.Refresh();
            Withdraw.Refresh();
        }

        /// <summary>Передаёт ошибку окну для показа сообщения.</summary>
        public void ReportError(string message)
        {
            ErrorOccurred(message);
        }
    }
}

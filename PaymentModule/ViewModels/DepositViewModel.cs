namespace PaymentModule.ViewModels
{
    public class DepositViewModel : OperationViewModel
    {
        //лимит на одно пополнение в валюте счёта
        public const float DepositLimit = 1000000;
        public bool AmountValid
        {
            get
            {
                float amount;
                if (!TryGetAmount(out amount)) { return false; }
                float balance = main.CurrentAccount.GetMoneyAmount();
                return amount < DepositLimit && float.IsFinite(balance + amount) && balance + amount > balance;
            }
        }
        public override bool Precondition { get { return AccountValid && AmountValid; } }

        /// <summary>модель пополнения</summary>
        public DepositViewModel(MainViewModel main) : base(main) { }

        /// <summary>пополняет счёт и проверяет увеличение баланса</summary>
        protected override bool Apply()
        {
            float amount;
            TryGetAmount(out amount);
            float oldBalance = main.CurrentAccount.GetMoneyAmount();
            main.CurrentAccount.AddMoney(amount);
            return main.CurrentAccount.GetMoneyAmount() == oldBalance + amount;
        }
    }
}

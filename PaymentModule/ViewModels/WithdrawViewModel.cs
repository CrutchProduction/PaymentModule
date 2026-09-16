namespace PaymentModule.ViewModels
{
    public class WithdrawViewModel : OperationViewModel
    {
        public override bool AccountValid { get { return main.CurrentAccount.GetMoneyAmount() > 0; } }
        public bool AmountValid
        {
            get
            {
                float amount;
                if (!TryGetAmount(out amount)) { return false; }
                float balance = main.CurrentAccount.GetMoneyAmount();
                return balance > amount && balance - amount < balance;
            }
        }
        public override bool Precondition { get { return AccountValid && AmountValid; } }

        /// <summary>Создаёт модель снятия.</summary>
        public WithdrawViewModel(MainViewModel main) : base(main) { }

        /// <summary>Снимает деньги и проверяет уменьшение баланса.</summary>
        protected override bool Apply()
        {
            float amount;
            TryGetAmount(out amount);
            float oldBalance = main.CurrentAccount.GetMoneyAmount();
            main.CurrentAccount.RemoveMoney(amount);
            return main.CurrentAccount.GetMoneyAmount() == oldBalance - amount;
        }
    }
}

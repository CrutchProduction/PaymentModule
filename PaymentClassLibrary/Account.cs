namespace PaymentClassLibrary
{
    public class Account
    {
        private static readonly string[] moneyTypes = ["₽", "€", "$", "¥"];
        private int accountId;
        private int moneyAmount;
        private int moneyType;

        public Account(int accountId, int moneyAmount, int moneyType)
        {
            this.accountId = accountId;
            this.moneyAmount = moneyAmount;
            this.moneyType = moneyType;
        }

        public void AddMoney(int money) { moneyAmount += money; }
        public void RemoveMoney(int money) { moneyAmount -= money; }
        public void SetMoneyType(int newMoneyType) { moneyType = newMoneyType; }

        public int GetAccountId() { return accountId; }
        public int GetMoneyAmount() { return moneyAmount; }
        public string GetMoneyType() { return moneyTypes[moneyType]; }
        public int GetMoneyTypeId() { return moneyType; }
    }
}

using System;

namespace PaymentClassLibrary
{
    public class Account
    {
        private static readonly string[] moneyTypes = ["₽", "€", "$", "¥"];
        private static readonly float[] moneyScales = [1, 97, 84.05f, 12.52f];
        private int accountId;
        private float moneyAmount;
        private int moneyType;

        public Account(int accountId, float moneyAmount, int moneyType)
        {
            this.accountId = accountId;
            this.moneyAmount = moneyAmount;
            this.moneyType = moneyType;
        }

        public void ConvertMoneyToAnotherType(int newMoneyType)
        {
            float moneyInRubble = moneyAmount * moneyScales[moneyType];
            moneyAmount = (float) Math.Round(moneyInRubble / moneyScales[newMoneyType], 2);
            moneyType = newMoneyType;
        }

        public void AddMoney(float money) { moneyAmount += money; }
        public void RemoveMoney(float money) { moneyAmount -= money; }
        public void SetMoneyType(int newMoneyType) { moneyType = newMoneyType; }

        public int GetAccountId() { return accountId; }
        public float GetMoneyAmount() { return moneyAmount; }
        public string GetMoneyType() { return moneyTypes[moneyType]; }
        public int GetMoneyTypeId() { return moneyType; }
    }
}

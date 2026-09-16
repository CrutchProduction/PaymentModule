using System;

namespace PaymentClassLibrary
{
    public class Account
    {
        private static readonly string[] moneyTypes = ["₽", "€", "$", "¥"];
        private static readonly float[] moneyScales = [1, 97, 84.05f, 12.52f];
        private readonly int accountId;
        private float moneyAmount;
        private int moneyType;

        /// <summary>Создаёт счёт с начальным балансом и валютой.</summary>
        public Account(int accountId, float moneyAmount, int moneyType)
        {
            this.accountId = accountId;
            this.moneyAmount = moneyAmount;
            this.moneyType = moneyType;
        }

        /// <summary>Переводит весь баланс в выбранную валюту.</summary>
        public void ConvertMoneyToAnotherType(int newMoneyType)
        {
            // Сначала переводим сумму в рубли, затем в новую валюту.
            float moneyInRubble = moneyAmount * moneyScales[moneyType];
            moneyAmount = (float)Math.Round(moneyInRubble / moneyScales[newMoneyType], 2);
            moneyType = newMoneyType;
        }

        /// <summary>Возвращает учебный курс валюты в рублях.</summary>
        public static float GetMoneyScale(int moneyType) { return moneyScales[moneyType]; }

        /// <summary>Добавляет указанную сумму к балансу.</summary>
        public void AddMoney(float money) { moneyAmount += money; }

        /// <summary>Вычитает указанную сумму из баланса.</summary>
        public void RemoveMoney(float money) { moneyAmount -= money; }

        /// <summary>Устанавливает валюту без пересчёта суммы.</summary>
        public void SetMoneyType(int newMoneyType) { moneyType = newMoneyType; }

        /// <summary>Возвращает неизменяемый номер счёта.</summary>
        public int GetAccountId() { return accountId; }

        /// <summary>Возвращает текущий баланс.</summary>
        public float GetMoneyAmount() { return moneyAmount; }

        /// <summary>Возвращает символ валюты.</summary>
        public string GetMoneyType() { return moneyTypes[moneyType]; }

        /// <summary>Возвращает номер валюты в списке.</summary>
        public int GetMoneyTypeId() { return moneyType; }
    }
}

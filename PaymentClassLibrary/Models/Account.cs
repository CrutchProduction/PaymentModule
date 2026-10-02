using System;

namespace PaymentClassLibrary.Models
{
    public class Account
    {
        private readonly int accountId;
        private float moneyAmount;
        private int moneyType;

        /// <summary>
        ///     Конструктор счёта
        /// </summary>
        /// <param name="accountId"> Айди аккаунта </param>
        /// <param name="moneyAmount"> Количество денег на аккаунте </param>
        /// <param name="moneyType"> Айди типа валюты аккаунта </param>
        public Account(int accountId, float moneyAmount, int moneyType)
        {
            this.accountId = accountId;
            this.moneyAmount = moneyAmount;
            this.moneyType = moneyType;
        }

        /// <summary>
        ///     Добавление денег на счёт
        /// </summary>
        /// <param name="money"> Количество денег для добавления </param>
        public void AddMoney(float money) { moneyAmount += money; }

        /// <summary>
        ///     Вычитание денег со счёта
        /// </summary>
        /// <param name="money"> Количество денег для снятия </param>
        public void RemoveMoney(float money) { moneyAmount -= money; }

        /// <summary>
        ///     Установка определённого количества денег
        /// </summary>
        /// <param name="newMoneyAmount"> Новое количество денег </param>
        public void SetMoney(float newMoneyAmount) { moneyAmount = newMoneyAmount; }

        /// <summary>
        ///     Установка определённого типа валюты
        /// </summary>
        /// <param name="newMoneyType"> Новый тип валюты </param>
        public void SetMoneyType(int newMoneyType) { moneyType = newMoneyType; }

        /// <summary>
        ///     Получение номера счёта
        /// </summary>
        /// <returns> Номер счёта </returns>
        public int GetAccountId() { return accountId; }

        /// <summary>
        ///     Получение текущего баланса
        /// </summary>
        /// <returns> Количество денег на аккаунте </returns>
        public float GetMoneyAmount() { return moneyAmount; }

        /// <summary>
        ///     Получить айди типа данных валюты
        /// </summary>
        /// <returns> Айди типа данных валюты </returns>
        public int GetMoneyTypeId() { return moneyType; }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentClassLibTest
{
    [TestClass]
    public class AccountTests
    {
        // Конструктор сохраняет начальные значения
        [TestMethod]
        public void Constructor_SetsInitialValues()
        {
            var account = new Account(1, 100f, 0);

            Assert.AreEqual(1, account.GetAccountId());
            Assert.AreEqual(100f, account.GetMoneyAmount());
            Assert.AreEqual(0, account.GetMoneyTypeId());
            Assert.AreEqual("₽", account.GetMoneyType());
        }

        // Пополнение увеличивает баланс
        [TestMethod]
        public void AddMoney_IncreasesBalance()
        {
            var account = new Account(1, 100f, 0);

            account.AddMoney(50f);

            Assert.AreEqual(150f, account.GetMoneyAmount());
        }

        // Снятие уменьшает баланс
        [TestMethod]
        public void RemoveMoney_DecreasesBalance()
        {
            var account = new Account(1, 100f, 0);

            account.RemoveMoney(30f);

            Assert.AreEqual(70f, account.GetMoneyAmount());
        }

        // Конвертация рублей в доллары (курс 84.05)
        [TestMethod]
        public void ConvertMoneyToAnotherType_RublesToDollars()
        {
            var account = new Account(1, 8405f, 0); // 8405 рублей

            account.ConvertMoneyToAnotherType(2); // 2 = доллар

            Assert.AreEqual(2, account.GetMoneyTypeId());
            Assert.AreEqual(100f, account.GetMoneyAmount(), 0.01f);
        }

        // Смена валюты без пересчёта
        [TestMethod]
        public void SetMoneyType_ChangesCurrencyWithoutRecalculation()
        {
            var account = new Account(1, 100f, 0);

            account.SetMoneyType(2);

            Assert.AreEqual(2, account.GetMoneyTypeId());
            Assert.AreEqual(100f, account.GetMoneyAmount());
        }

        // Проверка курса валют
        [TestMethod]
        public void GetMoneyScale_ReturnsCorrectScale()
        {
            Assert.AreEqual(1f, Account.GetMoneyScale(0));
            Assert.AreEqual(97f, Account.GetMoneyScale(1));
            Assert.AreEqual(84.05f, Account.GetMoneyScale(2));
            Assert.AreEqual(12.52f, Account.GetMoneyScale(3));
        }
    }
}

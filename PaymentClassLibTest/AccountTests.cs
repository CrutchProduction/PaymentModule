using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaymentClassLibrary.Managers;


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

        // Смена валюты без пересчёта
        [TestMethod]
        public void SetMoneyType_ChangesCurrencyWithoutRecalculation()
        {
            var account = new Account(1, 100f, 0);

            account.SetMoneyType(2);

            Assert.AreEqual(2, account.GetMoneyTypeId());
            Assert.AreEqual(100f, account.GetMoneyAmount());
        }
    }
}

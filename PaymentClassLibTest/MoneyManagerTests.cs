using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Managers;
using PaymentClassLibrary.Models;

namespace PaymentClassLibTest
{
    [TestClass]
    public class MoneyManagerTests
    {
        [TestInitialize]
        public void Setup()
        {
            string scalesPath = Path.Combine(AppContext.BaseDirectory, "moneyScales.dat");
            if (File.Exists(scalesPath)) File.Delete(scalesPath);
        }

        [TestMethod]
        public void LoadMoneyScales_LoadsAtLeastFourCurrencies()
        {
            MoneyManager.LoadMoneyScales();

            string[] names = MoneyManager.GetMoneyNames();

            Assert.IsNotNull(names);
            Assert.IsTrue(names.Length >= 4);
        }

        [TestMethod]
        public void GetMoneyChar_Rubles_ReturnsSymbol()
        {
            MoneyManager.LoadMoneyScales();

            string symbol = MoneyManager.GetMoneyChar(0);

            Assert.AreEqual("₽", symbol);
        }

        [TestMethod]
        public void GetMoneyScale_Rubles_IsOne()
        {
            MoneyManager.LoadMoneyScales();

            float scale = MoneyManager.GetMoneyScale(0);

            Assert.AreEqual(1f, scale);
        }

        [TestMethod]
        public void ConvertAccountMoney_RublesToDollars_Recalculates()
        {
            MoneyManager.LoadMoneyScales();
            var account = new Account(1, 1000f, 0);

            MoneyManager.ConvertAccountMoney(account, 2);

            Assert.AreEqual(2, account.GetMoneyTypeId());
            Assert.IsTrue(account.GetMoneyAmount() > 11f && account.GetMoneyAmount() < 13f);
        }

        [TestMethod]
        public void ConvertAccountMoney_SameCurrency_KeepsBalance()
        {
            MoneyManager.LoadMoneyScales();
            var account = new Account(1, 500f, 0);

            MoneyManager.ConvertAccountMoney(account, 0);

            Assert.AreEqual(0, account.GetMoneyTypeId());
            Assert.AreEqual(500f, account.GetMoneyAmount());
        }
    }
}
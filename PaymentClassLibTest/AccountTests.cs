using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Models;

namespace PaymentClassLibTest
{
    [TestClass]
    public class AccountTests
    {
        [TestMethod]
        public void Constructor_SetsInitialValues()
        {
            var account = new Account(6767, 100f, 0);

            Assert.AreEqual(6767, account.GetAccountId());
            Assert.AreEqual(100f, account.GetMoneyAmount());
            Assert.AreEqual(0, account.GetMoneyTypeId());
        }

        [TestMethod]
        public void AddMoney_IncreasesBalance()
        {
            var account = new Account(1, 100f, 0);

            account.AddMoney(50f);

            Assert.AreEqual(150f, account.GetMoneyAmount());
        }

        [TestMethod]
        public void RemoveMoney_DecreasesBalance()
        {
            var account = new Account(1, 100f, 0);

            account.RemoveMoney(30f);

            Assert.AreEqual(70f, account.GetMoneyAmount());
        }

        [TestMethod]
        public void SetMoney_ReplacesBalance()
        {
            var account = new Account(1, 100f, 0);

            account.SetMoney(999f);

            Assert.AreEqual(999f, account.GetMoneyAmount());
        }

        [TestMethod]
        public void SetMoneyType_ChangesCurrencyWithoutChangingBalance()
        {
            var account = new Account(1, 100f, 0);

            account.SetMoneyType(2);

            Assert.AreEqual(2, account.GetMoneyTypeId());
            Assert.AreEqual(100f, account.GetMoneyAmount());
        }
    }
}
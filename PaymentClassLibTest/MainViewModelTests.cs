using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Managers;
using PaymentClassLibrary.Models;
using PaymentModule.ViewModels;

namespace PaymentClassLibTest
{
    [TestClass]
    public class MainViewModelTests
    {
        [TestInitialize]
        public void Setup()
        {
            string accountsPath = Path.Combine(AppContext.BaseDirectory, "accounts.dat");
            string scalesPath = Path.Combine(AppContext.BaseDirectory, "moneyScales.dat");
            if (File.Exists(accountsPath)) File.Delete(accountsPath);
            if (File.Exists(scalesPath)) File.Delete(scalesPath);

            AccountsManager.LoadAccount();
            MoneyManager.LoadMoneyScales();
        }

        private MainViewModel CreateMain()
        {
            var account = AccountsManager.FindAccountById(MainViewModel.CurrentAccountId);
            return new MainViewModel(account);
        }

        [TestMethod]
        public void Constructor_LoadsCurrentAccount()
        {
            var main = CreateMain();

            Assert.IsNotNull(main.CurrentAccount);
            Assert.AreEqual(MainViewModel.CurrentAccountId, main.CurrentAccount.GetAccountId());
        }

        [TestMethod]
        public void Constructor_NullAccount_Throws()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new MainViewModel(null));
        }

        [TestMethod]
        public void Constructor_WrongAccountId_Throws()
        {
            var foreign = new Account(9999, 100f, 0);

            Assert.ThrowsException<InvalidOperationException>(() => new MainViewModel(foreign));
        }

        [TestMethod]
        public void Balance_ContainsInitialAmount()
        {
            var main = CreateMain();

            Assert.IsTrue(main.Balance.Contains("100"));
        }

        [TestMethod]
        public void Currency_ReturnsRubleSymbol()
        {
            var main = CreateMain();

            Assert.AreEqual("₽", main.Currency);
        }

        [TestMethod]
        public void ReportError_RaisesErrorEvent()
        {
            var main = CreateMain();
            string received = null;
            main.ErrorOccurred += msg => received = msg;

            main.ReportError("тестовая ошибка");

            Assert.AreEqual("тестовая ошибка", received);
        }

        [TestMethod]
        public void AllOperations_AreCreated()
        {
            var main = CreateMain();

            Assert.IsNotNull(main.Deposit);
            Assert.IsNotNull(main.Withdraw);
            Assert.IsNotNull(main.Transfer);
            Assert.IsNotNull(main.Exchange);
        }
    }
}
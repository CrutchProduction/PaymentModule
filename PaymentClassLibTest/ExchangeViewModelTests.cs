using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Managers;
using PaymentModule.ViewModels;

namespace PaymentClassLibTest
{
    [TestClass]
    public class ExchangeViewModelTests
    {
        private MainViewModel main;

        [TestInitialize]
        public void Setup()
        {
            string accountsPath = Path.Combine(AppContext.BaseDirectory, "accounts.dat");
            string scalesPath = Path.Combine(AppContext.BaseDirectory, "moneyScales.dat");
            if (File.Exists(accountsPath)) File.Delete(accountsPath);
            if (File.Exists(scalesPath)) File.Delete(scalesPath);

            AccountsManager.LoadAccount();
            MoneyManager.LoadMoneyScales();
            main = new MainViewModel(AccountsManager.FindAccountById(MainViewModel.CurrentAccountId));
        }

        [TestMethod]
        public void Precondition_SameCurrency_False()
        {
            main.Exchange.SelectedCurrency = main.CurrentAccount.GetMoneyTypeId();
            Assert.IsFalse(main.Exchange.Precondition);
        }

        [TestMethod]
        public void Precondition_DifferentCurrency_True()
        {
            main.Exchange.SelectedCurrency = 2; // доллар
            Assert.IsTrue(main.Exchange.Precondition);
        }

        [TestMethod]
        public void Precondition_InvalidCurrencyIndex_False()
        {
            main.Exchange.SelectedCurrency = 999;
            Assert.IsFalse(main.Exchange.Precondition);
        }

        [TestMethod]
        public void Execute_ValidExchange_ChangesCurrencyAndBalance()
        {
            float oldBalance = main.CurrentAccount.GetMoneyAmount();
            main.Exchange.SelectedCurrency = 2;

            main.Exchange.ExecuteCommand.Execute(null);

            Assert.AreEqual(2, main.CurrentAccount.GetMoneyTypeId());
            Assert.AreNotEqual(oldBalance, main.CurrentAccount.GetMoneyAmount());
            Assert.IsTrue(main.Exchange.Postcondition);
        }
    }
}
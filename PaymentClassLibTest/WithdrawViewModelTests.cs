using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Managers;
using PaymentModule.ViewModels;

namespace PaymentClassLibTest
{
    [TestClass]
    public class WithdrawViewModelTests
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
        public void Precondition_EmptyAmount_False()
        {
            main.Withdraw.AmountText = "";
            Assert.IsFalse(main.Withdraw.Precondition);
        }

        [TestMethod]
        public void Precondition_ZeroAmount_False()
        {
            main.Withdraw.AmountText = "0";
            Assert.IsFalse(main.Withdraw.Precondition);
        }

        [TestMethod]
        public void Precondition_NegativeAmount_False()
        {
            main.Withdraw.AmountText = "-50";
            Assert.IsFalse(main.Withdraw.Precondition);
        }

        [TestMethod]
        public void Precondition_AmountGreaterThanBalance_False()
        {
            main.Withdraw.AmountText = "999999";
            Assert.IsFalse(main.Withdraw.Precondition);
        }

        [TestMethod]
        public void Precondition_ValidAmount_True()
        {
            main.Withdraw.AmountText = "50";
            Assert.IsTrue(main.Withdraw.Precondition);
        }

        [TestMethod]
        public void Execute_ValidWithdraw_DecreasesBalance()
        {
            float oldBalance = main.CurrentAccount.GetMoneyAmount();
            main.Withdraw.AmountText = "50";

            main.Withdraw.ExecuteCommand.Execute(null);

            Assert.AreEqual(oldBalance - 50f, main.CurrentAccount.GetMoneyAmount());
            Assert.IsTrue(main.Withdraw.Postcondition);
        }

        [TestMethod]
        public void Precondition_ThreeDecimalPlaces_False()
        {
            main.Withdraw.AmountText = "10.123";
            Assert.IsFalse(main.Withdraw.Precondition);
        }

        [TestMethod]
        public void Precondition_CommaSeparator_True()
        {
            main.Withdraw.AmountText = "10,50";
            Assert.IsTrue(main.Withdraw.Precondition);
        }
    }
}
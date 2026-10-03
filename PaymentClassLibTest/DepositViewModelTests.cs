using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Managers;
using PaymentModule.ViewModels;

namespace PaymentClassLibTest
{
    [TestClass]
    public class DepositViewModelTests
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
            main.Deposit.AmountText = "";
            Assert.IsFalse(main.Deposit.Precondition);
        }

        [TestMethod]
        public void Precondition_ValidAmount_True()
        {
            main.Deposit.AmountText = "100";
            Assert.IsTrue(main.Deposit.Precondition);
        }

        [TestMethod]
        public void Precondition_AmountOverLimit_False()
        {
            main.Deposit.AmountText = "99999999"; // больше лимита 2 000 000
            Assert.IsFalse(main.Deposit.Precondition);
        }

        [TestMethod]
        public void Execute_ValidDeposit_IncreasesBalance()
        {
            float oldBalance = main.CurrentAccount.GetMoneyAmount();
            main.Deposit.AmountText = "100";

            main.Deposit.ExecuteCommand.Execute(null);

            Assert.AreEqual(oldBalance + 100f, main.CurrentAccount.GetMoneyAmount());
            Assert.IsTrue(main.Deposit.Postcondition);
        }
    }
}
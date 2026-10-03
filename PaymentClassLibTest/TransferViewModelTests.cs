using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Managers;
using PaymentModule.ViewModels;

namespace PaymentClassLibTest
{
    [TestClass]
    public class TransferViewModelTests
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
        public void Precondition_NoRecipient_False()
        {
            main.Transfer.RecipientText = "";
            main.Transfer.AmountText = "50";
            Assert.IsFalse(main.Transfer.Precondition);
        }

        [TestMethod]
        public void Precondition_UnknownRecipient_False()
        {
            main.Transfer.RecipientText = "9999";
            main.Transfer.AmountText = "50";
            Assert.IsFalse(main.Transfer.Precondition);
        }

        [TestMethod]
        public void Precondition_TransferToSelf_False()
        {
            main.Transfer.RecipientText = MainViewModel.CurrentAccountId.ToString();
            main.Transfer.AmountText = "50";
            Assert.IsFalse(main.Transfer.Precondition);
        }

        [TestMethod]
        public void Precondition_AmountGreaterThanBalance_False()
        {
            main.Transfer.RecipientText = "4252";
            main.Transfer.AmountText = "999999";
            Assert.IsFalse(main.Transfer.Precondition);
        }

        [TestMethod]
        public void Precondition_ValidTransfer_True()
        {
            main.Transfer.RecipientText = "4252";
            main.Transfer.AmountText = "50";
            Assert.IsTrue(main.Transfer.Precondition);
        }

        [TestMethod]
        public void Execute_ValidTransfer_UpdatesBothBalances()
        {
            float senderOld = main.CurrentAccount.GetMoneyAmount();
            float recipientOld = AccountsManager.FindAccountById(4252).GetMoneyAmount();

            main.Transfer.RecipientText = "4252";
            main.Transfer.AmountText = "50";
            main.Transfer.ExecuteCommand.Execute(null);

            Assert.AreEqual(senderOld - 50f, main.CurrentAccount.GetMoneyAmount());
            Assert.AreEqual(recipientOld + 50f, AccountsManager.FindAccountById(4252).GetMoneyAmount());
            Assert.IsTrue(main.Transfer.Postcondition);
        }
    }
}
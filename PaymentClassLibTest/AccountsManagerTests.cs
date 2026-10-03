using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Managers;
using PaymentClassLibrary.Models;

namespace PaymentClassLibTest
{
    [TestClass]
    public class AccountsManagerTests
    {
        [TestInitialize]
        public void Setup()
        {
            string accountsPath = Path.Combine(AppContext.BaseDirectory, "accounts.dat");
            string scalesPath = Path.Combine(AppContext.BaseDirectory, "moneyScales.dat");
            if (File.Exists(accountsPath)) File.Delete(accountsPath);
            if (File.Exists(scalesPath)) File.Delete(scalesPath);
        }

        [TestMethod]
        public void LoadAccount_CreatesFileWithDefaultData()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "accounts.dat");
            Assert.IsFalse(File.Exists(path));

            AccountsManager.LoadAccount();

            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        public void LoadAccount_DefaultAccount6767Has100Rubles()
        {
            AccountsManager.LoadAccount();

            Account account = AccountsManager.FindAccountById(6767);

            Assert.IsNotNull(account);
            Assert.AreEqual(100f, account.GetMoneyAmount());
            Assert.AreEqual(0, account.GetMoneyTypeId());
        }

        [TestMethod]
        public void FindAccountById_UnknownId_ReturnsNull()
        {
            AccountsManager.LoadAccount();

            Account account = AccountsManager.FindAccountById(9999);

            Assert.IsNull(account);
        }

        [TestMethod]
        public void SaveAccounts_SavesChangesToFile()
        {
            AccountsManager.LoadAccount();
            Account account = AccountsManager.FindAccountById(6767);
            account.AddMoney(500f);

            AccountsManager.SaveAccounts();
            AccountsManager.LoadAccount();
            Account reloaded = AccountsManager.FindAccountById(6767);

            Assert.AreEqual(600f, reloaded.GetMoneyAmount());
        }

        [TestMethod]
        public void LoadAccount_InvalidData_ThrowsInvalidDataException()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "accounts.dat");
            File.WriteAllText(path, "мусор мусор мусор");

            Assert.ThrowsException<InvalidDataException>(() => AccountsManager.LoadAccount());
        }

        [TestMethod]
        public void LoadAccount_DuplicateId_ThrowsInvalidDataException()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "accounts.dat");
            File.WriteAllText(path, "6767 100 0\r\n6767 200 0\r\n");

            Assert.ThrowsException<InvalidDataException>(() => AccountsManager.LoadAccount());
        }
    }
}
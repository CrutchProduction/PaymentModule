using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PaymentClassLibrary.Models;
using PaymentClassLibrary.Managers;

namespace PaymentClassLibTest
{
    [TestClass]
    public class AccountsManagerTests
    {
        // Перед каждым тестом удаляем файл, чтобы не было влияния
        [TestInitialize]
        public void Setup()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "accounts.dat");
            if (File.Exists(path)) File.Delete(path);
        }

        // LoadAccount создаёт файл и загружает счета
        [TestMethod]
        public void LoadAccount_CreatesFileAndLoadsAccounts()
        {
            AccountsManager.LoadAccount();

            Account account = AccountsManager.FindAccountById(6767);
            Assert.IsNotNull(account);
            Assert.AreEqual(100f, account.GetMoneyAmount());
        }

        // FindAccountById возвращает null для несуществующего счёта
        [TestMethod]
        public void FindAccountById_ReturnsNullForUnknownId()
        {
            AccountsManager.LoadAccount();

            Account account = AccountsManager.FindAccountById(9999);

            Assert.IsNull(account);
        }

        // SaveAccounts сохраняет изменения
        [TestMethod]
        public void SaveAccounts_SavesChanges()
        {
            AccountsManager.LoadAccount();
            Account account = AccountsManager.FindAccountById(4252);
            account.AddMoney(500f);

            AccountsManager.SaveAccounts();
            AccountsManager.LoadAccount();
            Account reloaded = AccountsManager.FindAccountById(4252);

            Assert.AreEqual(5500f, reloaded.GetMoneyAmount());
        }
    }
}

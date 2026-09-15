using System;
using System.Collections;
using System.IO;
using System.Reflection.Metadata;

namespace PaymentClassLibrary
{
    public partial class AccountsManager
    {
        private static ArrayList accounts = new ArrayList();
        // Путь до папки с данными от аккаунтов базово сохраняет в .\PaymentModule\PaymentModule\bin\x64\Debug\net8.0-windows10.0.19041.0\AppX
        private static string pathToAccounts = Path.Combine(AppContext.BaseDirectory, "accounts.dat");

        private static void CheckFileExistance()
        {
            if (!File.Exists(pathToAccounts))
            {
                File.Create(pathToAccounts).Close();
            }
        }

        /// <summary>
        ///     Считать из файла данные всех аккаунтов
        /// </summary>
        public static void LoadAccount()
        {
            CheckFileExistance();

            foreach (string line in File.ReadLines(pathToAccounts))
            {
                string[] dataSplitted = line.Split(' ');

                int accountId;
                int moneyAmount;
                int moneyType;
                
                if (!int.TryParse(dataSplitted[0], out accountId)) { break; }
                if (!int.TryParse(dataSplitted[1], out moneyAmount)) { break; }
                if (!int.TryParse(dataSplitted[2], out moneyType)) { break; }

                accounts.Add(new Account(accountId, moneyAmount, moneyType));
            }
        }

        /// <summary>
        ///     Перезаписать данные от аккаунтов
        /// </summary>
        public static void SaveAccounts()
        {
            CheckFileExistance();

            string newData = "";
            foreach (Account account in accounts)
            {
                newData += $"{account.GetAccountId()} {account.GetMoneyAmount()} {account.GetMoneyTypeId()}\n";
            }
            File.WriteAllText(pathToAccounts, newData);
        }

        /// <summary>
        ///     Найти аккаунт по его айди
        /// </summary>
        /// <param name="accountId"> Айди аккаунта </param>
        /// <returns> Класс Account, если есть, иначе null </returns>
        public static Account FindAccountById(int accountId)
        {
            foreach (Account account in accounts)
            {
                if (account.GetAccountId() == accountId)
                {
                    return account;
                }
            }
            return null;
        }
    }
}

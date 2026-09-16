using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace PaymentClassLibrary
{
    public partial class AccountsManager
    {
        private static ArrayList accounts = new ArrayList();
        private static string pathToAccounts = Path.Combine(AppContext.BaseDirectory, "accounts.dat");

        /// <summary>Создаёт файл с учебными счетами при первом запуске.</summary>
        private static void CheckFileExistance()
        {
            if (!File.Exists(pathToAccounts))
            {
                File.WriteAllText(pathToAccounts, "6767 100 0\r\n4252 5000 0\r\n5242 192 0\r\n9911 2456 0\r\n1199 1925 0\r\n");
            }
        }

        /// <summary>Считывает счета из файла без повторного добавления.</summary>
        public static void LoadAccount()
        {
            CheckFileExistance();
            ArrayList loadedAccounts = new ArrayList();
            foreach (string line in File.ReadLines(pathToAccounts))
            {
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                string[] data = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int accountId;
                float moneyAmount;
                int moneyType;
                if (data.Length != 3
                    || !int.TryParse(data[0], out accountId)
                    || !float.TryParse(data[1].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out moneyAmount)
                    || !int.TryParse(data[2], out moneyType)
                    || accountId <= 0 || !float.IsFinite(moneyAmount) || moneyAmount < 0
                    || moneyType < 0 || moneyType > 3)
                {
                    throw new InvalidDataException("Некорректные данные в accounts.dat.");
                }
                foreach (Account existing in loadedAccounts)
                {
                    if (existing.GetAccountId() == accountId)
                    {
                        throw new InvalidDataException("Повторяющийся номер счёта в accounts.dat.");
                    }
                }
                loadedAccounts.Add(new Account(accountId, moneyAmount, moneyType));
            }
            // Меняем список только после чтения всего файла.
            accounts = loadedAccounts;
        }

        /// <summary>Сохраняет балансы и валюты всех счетов.</summary>
        public static void SaveAccounts()
        {
            string newData = "";
            foreach (Account account in accounts)
            {
                newData += account.GetAccountId() + " "
                    + account.GetMoneyAmount().ToString(CultureInfo.InvariantCulture) + " "
                    + account.GetMoneyTypeId() + "\n";
            }
            // Сначала готовим новый файл, затем заменяем исходный.
            string temporaryPath = pathToAccounts + ".tmp";
            File.WriteAllText(temporaryPath, newData);
            File.Move(temporaryPath, pathToAccounts, true);
        }

        /// <summary>Находит счёт по номеру, иначе возвращает null.</summary>
        public static Account FindAccountById(int accountId)
        {
            foreach (Account account in accounts)
            {
                if (account.GetAccountId() == accountId) { return account; }
            }
            return null;
        }
    }
}

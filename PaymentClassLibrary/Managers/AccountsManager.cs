using Microsoft.UI;
using Microsoft.Windows.Storage.Pickers;
using PaymentClassLibrary.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;


namespace PaymentClassLibrary.Managers
{
    public partial class AccountsManager
    {
        private static ArrayList accounts = new ArrayList();
        private static string pathToAccounts = Path.Combine(AppContext.BaseDirectory, "accounts.dat");

        /// <summary>
        ///     Проверка на существование файла пользователей
        /// </summary>
        private static void CheckFileExistance()
        {
            if (!File.Exists(pathToAccounts))
            {
                File.WriteAllText(pathToAccounts, "6767 100 0\r\n4252 5000 0\r\n5242 192 0\r\n9911 2456 0\r\n1199 1925 0\r\n");
            }
        }

        /// <summary>
        ///     Загружает из файла все счета
        /// </summary>
        /// <exception cref="InvalidDataException"> При некорректных данных в файле </exception>
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
            accounts = loadedAccounts;
        }

        /// <summary>
        ///     Сохраняет данные всех счетов
        /// </summary>
        public static void SaveAccounts()
        {
            string newData = "";
            foreach (Account account in accounts)
            {
                newData += account.GetAccountId() + " "
                    + account.GetMoneyAmount().ToString(CultureInfo.InvariantCulture) + " "
                    + account.GetMoneyTypeId() + "\n";
            }
            string temporaryPath = pathToAccounts + ".tmp";
            File.WriteAllText(temporaryPath, newData);
            File.Move(temporaryPath, pathToAccounts, true);
        }
        
        /// <summary>
        ///     Поиск счёта по номеру
        /// </summary>
        /// <param name="accountId"> Айди счёта </param>
        /// <returns> Счёт если существует, иначе null </returns>
        public static Account FindAccountById(int accountId)
        {
            foreach (Account account in accounts)
            {
                if (account.GetAccountId() == accountId) { return account; }
            }
            return null;
        }

        /// <summary>
        ///     Экспорт данных пользователей
        /// </summary>
        public static async void ImportData()
        {
            FileOpenPicker picker = new FileOpenPicker(new WindowId(0));
            picker.FileTypeFilter.Add(".dat");
            PickFileResult result = await picker.PickSingleFileAsync();
            
            if (result != null)
            {
                string path = result.Path;
                File.Copy(path, pathToAccounts + ".tmp");
                File.Move(pathToAccounts + ".tmp", pathToAccounts, true);
                LoadAccount();
                Logger.Log($"Успешно импортированны данные из {path}");
            }
        }

        /// <summary>
        ///     Экспорт данных пользователей
        /// </summary>
        public static async void ExportData()
        {
            FileSavePicker picker = new FileSavePicker(new WindowId(0));
            picker.FileTypeChoices.Add(".dat", new List<string>() { ".dat" });
            picker.SuggestedFileName = "accounts";
            PickFileResult result = await picker.PickSaveFileAsync();

            if (result != null)
            {
                string path = result.Path;
                File.Copy(pathToAccounts, path + ".tmp");
                File.Move(path + ".tmp", path, true);
                Logger.Log($"Файл данных был экспортирован в {path}");
            }
        }
    }
}

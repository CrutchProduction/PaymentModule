using System;
using System.Collections.Generic;
using System.IO;

namespace PaymentClassLibrary
{
    public class MoneyManager
    {
        private static string pathToValues = Path.Combine(AppContext.BaseDirectory, "moneyScales.dat");
        private static string[] moneyNames;
        private static string[] moneyTypes;
        private static float[] moneyScales;

        /// <summary>
        ///     Проверка на существование файла валют
        /// </summary>
        private static void CheckFileExistance()
        {
            if (!File.Exists(pathToValues))
            {
                File.WriteAllText(pathToValues, "Российский рубль|₽|1\r\nЕвро|€|94,32\r\nДоллар США|$|83,08\r\nЮань|¥|12,39\r\nБелорусский рубль|BYN|27,62\r\nФунт стерлингов|£|110,26\r\nАвстралийский доллар|A$|57,76\r\nФранк|₣|99,51\r\nНовозеландский доллар|NZ$|46,86");
            }
        }

        /// <summary>
        ///     Конвертация валюты в другую валюту
        /// </summary>
        /// <param name="account"> Аккаунт для изменения валюты </param>
        /// <param name="newMoneyType"> Индекс новой валюты </param>
        public static void ConvertAccountMoney(Account account, int newMoneyType)
        {
            float moneyInRubble = account.GetMoneyAmount() * moneyScales[account.GetMoneyTypeId()];
            account.SetMoney((float)Math.Round(moneyInRubble / moneyScales[newMoneyType], 2));
            account.SetMoneyType(newMoneyType);
        }

        /// <summary>
        ///     Загрузка валюты из файла
        /// </summary>
        /// <exception cref="InvalidDataException"> При некорректных данных в файле </exception>
        public static void LoadMoneyScales()
        {
            CheckFileExistance();
            List<string> newMoneyNames = new List<string>();
            List<string> newMoneyTypes = new List<string>();
            List<float> newMoneyScales = new List<float>();

            foreach (string line in File.ReadLines(pathToValues))
            {
                string[] splittedLine = line.Split('|');
                float moneyScale;
                if (splittedLine.Length != 3 || 
                    !float.TryParse(splittedLine[2], out moneyScale)) { throw new InvalidDataException("Некорректные данные в moneyScales.dat"); }

                newMoneyNames.Add(splittedLine[0]);
                newMoneyTypes.Add(splittedLine[1]);
                newMoneyScales.Add(moneyScale);
            }

            moneyNames = newMoneyNames.ToArray();
            moneyTypes = newMoneyTypes.ToArray();
            moneyScales = newMoneyScales.ToArray();
        }

        /// <summary>
        ///     Получение всех названий валют
        /// </summary>
        /// <returns> Массив названий валют </returns>
        public static string[] GetMoneyNames() { return moneyNames; }

        /// <summary>
        ///     Получение символа обозначающего валюту по её индексу
        /// </summary>
        /// <param name="typeId"> Айди валюты </param>
        /// <returns> Символ обозначающий валюту </returns>
        public static string GetMoneyChar(int typeId) { return moneyTypes[typeId]; }
        
        /// <summary>
        ///     Получение множителя валюты по её индексу
        /// </summary>
        /// <param name="typeId"> Айди валюты </param>
        /// <returns> Множитель валюты </returns>
        public static float GetMoneyScale(int typeId) { return moneyScales[typeId]; }
    }
}

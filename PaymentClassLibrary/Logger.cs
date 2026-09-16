using System;
using System.IO;

namespace PaymentClassLibrary
{
    public class Logger
    {
        private static string pathToLogs = Path.Combine(AppContext.BaseDirectory, "logs");

        /// <summary>Создаёт папку журнала при необходимости.</summary>
        private static void CheckFileExistance()
        {
            if (!Directory.Exists(pathToLogs))
            {
                Directory.CreateDirectory(pathToLogs);
            }
        }

        /// <summary>Сохраняет старый журнал и начинает новый.</summary>
        public static void StartNewLog()
        {
            CheckFileExistance();
            string latest = Path.Combine(pathToLogs, "latest.log");
            if (File.Exists(latest))
            {
                // Имя файла не зависит от языка Windows и содержимого журнала.
                string archive = Path.Combine(pathToLogs, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff") + ".log");
                File.Copy(latest, archive);
            }
            File.WriteAllText(latest, DateTime.Now + "\n");
        }

        /// <summary>Добавляет выполненную операцию в журнал.</summary>
        public static void Log(string message)
        {
            CheckFileExistance();
            File.AppendAllText(Path.Combine(pathToLogs, "latest.log"), $"[{DateTime.Now}] - {message}\n");
        }
    }
}

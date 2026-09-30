using System;
using System.IO;

namespace PaymentClassLibrary
{
    public class Logger
    {
        private static string pathToLogs = Path.Combine(AppContext.BaseDirectory, "logs");

        /// <summary>
        ///     Проверка на существование папки логов
        /// </summary>
        private static void CheckFileExistance()
        {
            if (!Directory.Exists(pathToLogs))
            {
                Directory.CreateDirectory(pathToLogs);
            }
        }

        /// <summary>
        ///     Создание нового лога
        /// </summary>
        public static void StartNewLog()
        {
            CheckFileExistance();
            string latest = Path.Combine(pathToLogs, "latest.log");
            if (File.Exists(latest))
            {
                string archive = Path.Combine(pathToLogs, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff") + ".log");
                File.Copy(latest, archive);
            }
            File.WriteAllText(latest, DateTime.Now + "\n");
        }

        /// <summary>
        ///     Сохраняет сообщение в latest.log
        /// </summary>
        /// <param name="message"> Сообщение </param>
        public static void Log(string message)
        {
            CheckFileExistance();
            File.AppendAllText(Path.Combine(pathToLogs, "latest.log"), $"[{DateTime.Now}] - {message}\n");
        }
    }
}

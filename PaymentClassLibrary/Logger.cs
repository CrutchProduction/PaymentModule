using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentClassLibrary
{
    public class Logger
    {
        // Путь до папки с логами базово сохраняет в .\PaymentModule\PaymentModule\bin\x64\Debug\net8.0-windows10.0.19041.0\AppX\logs
        private static string pathToLogs = Path.Combine(AppContext.BaseDirectory, "logs");

        private static void CheckFileExistance()
        {
            if (!Directory.Exists(pathToLogs))
            {
                Directory.CreateDirectory(pathToLogs);
            }
        }

        /// <summary>
        ///     Начать записывать всё в новый файл логов
        /// </summary>
        public static void StartNewLog()
        {
            CheckFileExistance();

            if (File.Exists($"{pathToLogs}/latest.log"))
            {
                string lastLogName = File.ReadLines($"{pathToLogs}/latest.log").First().Replace(' ', '_').Replace(':', '_') + ".log";
                File.Create($"{pathToLogs}/{lastLogName}").Close();
                File.WriteAllText($"{pathToLogs}/{lastLogName}", File.ReadAllText($"{pathToLogs}/latest.log"));
                File.WriteAllText($"{pathToLogs}/latest.log", "");
            } else
            {
                File.Create($"{pathToLogs}/latest.log").Close();
            }

            File.WriteAllText($"{pathToLogs}/latest.log", DateTime.Now.ToString() + "\n");
        }

        /// <summary>
        ///     Записать сообщение в последний лог
        /// </summary>
        /// <param name="message"> Сообщение которое надо записать </param>
        public static void Log(string message)
        {
            CheckFileExistance();
            File.AppendAllText($"{pathToLogs}/latest.log", $"[{DateTime.Now}] - {message}\n");
        }
    }
}

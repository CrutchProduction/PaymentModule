using System;

namespace PaymentClassLibrary
{
    public static class Guard
    {
        /// <summary>Проверяет предусловие операции.</summary>
        public static void Requires(bool condition, string message)
        {
            // Неверные данные не должны изменять баланс.
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}

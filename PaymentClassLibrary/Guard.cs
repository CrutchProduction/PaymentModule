using System;

namespace PaymentClassLibrary
{
    public static class Guard
    {
        /// <summary>
        ///     Проверка условия
        /// </summary>
        /// <param name="condition"> Условие </param>
        /// <param name="message"> Выводимое сообщение </param>
        /// <exception cref="InvalidOperationException"> В случае condition == false </exception>
        public static void Requires(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}

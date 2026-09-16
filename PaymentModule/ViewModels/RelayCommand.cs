using System;
using System.Windows.Input;

namespace PaymentModule.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;
        public event EventHandler? CanExecuteChanged = delegate { };

        /// <summary>Сохраняет действие кнопки и проверку доступности.</summary>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>Проверяет, доступна ли операция.</summary>
        public bool CanExecute(object? parameter)
        {
            return canExecute();
        }

        /// <summary>Выполняет действие кнопки.</summary>
        public void Execute(object? parameter)
        {
            // Проверяем ввод ещё раз перед запуском.
            if (canExecute())
            {
                execute();
            }
        }

        /// <summary>Обновляет доступность кнопки.</summary>
        public void Refresh()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}


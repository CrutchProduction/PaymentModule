using System;

namespace PaymentModule.ViewModels
{
    public enum CellState
    {
        Normal,
        InPrefix,
        Changed,
        Current
    }

    public class ArrayCellViewModel : ViewModelBase
    {
        private readonly Action<int, double> valueChanged;
        private double value;
        private CellState state;
        private bool aboveThreshold;

        public ArrayCellViewModel(int index, int value, Action<int, double> valueChanged)
        {
            Index = index;
            this.value = value;
            this.valueChanged = valueChanged;
        }

        public int Index { get; }

        //номер платежа для таблицы
        public int Number
        {
            get { return Index + 1; }
        }

        //сумму платежа можно менять прямо в таблице
        public double Value
        {
            get { return value; }
            set
            {
                //пустое поле не трогаем
                if (double.IsNaN(value))
                {
                    return;
                }

                double newValue = Math.Round(value);
                if (newValue < 0)
                {
                    newValue = 0;
                }
                if (newValue > 1000000)
                {
                    newValue = 1000000;
                }
                if (this.value == newValue)
                {
                    return;
                }

                this.value = newValue;
                OnPropertyChanged();
                valueChanged(Index, newValue);
            }
        }

        public CellState State
        {
            get { return state; }
            set
            {
                if (state == value)
                {
                    return;
                }
                state = value;
                OnPropertyChanged();
            }
        }

        public bool AboveThreshold
        {
            get { return aboveThreshold; }
            set
            {
                if (aboveThreshold == value)
                {
                    return;
                }
                aboveThreshold = value;
                OnPropertyChanged();
            }
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PaymentModule.ViewModels
{
    public class LoopLabViewModel : ViewModelBase
    {
        private double n = 8;
        private double minValue = 0;
        private double maxValue = 9;
        private double threshold = 5;
        private int selectedModeIndex;
        private int[] values = new int[0];

        //состояние исполнения цикла
        private int j;
        private int res;
        private int stepCount;
        private int variantValue;
        private int variantMax = 1;
        private bool isRunning;
        private bool invBefore;
        private bool invAfter;
        private bool postHolds;
        private bool isFinished;

        private string traceText = "";
        private string variantLogText = "";
        private string summaryText = "wp-проверка подключается отдельно";

        //тексты выбранного режима
        private string codeText = "";
        private string preSpecText = "";
        private string postSpecText = "";
        private string specText = "";
        private string wpInvariant = "";
        private string wpVariant = "";
        private string defText = "";

        public LoopLabViewModel()
        {
            GenerateCommand = new RelayCommand(Generate, () => true);
            StepCommand = new RelayCommand(Step, () => true);
            RunCommand = new RelayCommand(RunClicked, () => true);
            ResetCommand = new RelayCommand(Reset, () => true);
            VerifyCommand = new RelayCommand(Verify, () => true);

            ApplyMode();
            Generate();
        }

        public ObservableCollection<ArrayCellViewModel> Cells { get; } = new ObservableCollection<ArrayCellViewModel>();
        public ObservableCollection<ProofRowViewModel> Proofs { get; } = new ObservableCollection<ProofRowViewModel>();

        public RelayCommand GenerateCommand { get; }
        public RelayCommand StepCommand { get; }
        public RelayCommand RunCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand VerifyCommand { get; }

        public double N
        {
            get { return n; }
            set
            {
                if (double.IsNaN(value))
                {
                    return;
                }
                double newValue = Math.Round(Math.Clamp(value, 1, 24));
                if (n == newValue)
                {
                    return;
                }
                n = newValue;
                OnPropertyChanged();
            }
        }

        public double MinValue
        {
            get { return minValue; }
            set
            {
                if (double.IsNaN(value))
                {
                    return;
                }
                double newValue = Math.Round(Math.Clamp(value, -50, 50));
                if (minValue == newValue)
                {
                    return;
                }
                minValue = newValue;
                OnPropertyChanged();
            }
        }

        public double MaxValue
        {
            get { return maxValue; }
            set
            {
                if (double.IsNaN(value))
                {
                    return;
                }
                double newValue = Math.Round(Math.Clamp(value, -50, 50));
                if (maxValue == newValue)
                {
                    return;
                }
                maxValue = newValue;
                OnPropertyChanged();
            }
        }

        public double Threshold
        {
            get { return threshold; }
            set
            {
                if (double.IsNaN(value))
                {
                    return;
                }
                double newValue = Math.Round(Math.Clamp(value, -50, 50));
                if (threshold == newValue)
                {
                    return;
                }
                threshold = newValue;
                OnPropertyChanged();
                Reset();
            }
        }

        public int SelectedModeIndex
        {
            get { return selectedModeIndex; }
            set
            {
                int newValue = Math.Clamp(value, 0, 2);
                if (selectedModeIndex == newValue)
                {
                    return;
                }
                selectedModeIndex = newValue;
                OnPropertyChanged();
                ApplyMode();
                Reset();
            }
        }

        public int J
        {
            get { return j; }
        }

        public int Res
        {
            get { return res; }
        }

        public int StepCount
        {
            get { return stepCount; }
        }

        public int VariantValue
        {
            get { return variantValue; }
        }

        public int VariantMax
        {
            get { return variantMax; }
        }

        public bool InvBefore
        {
            get { return invBefore; }
        }

        public bool InvAfter
        {
            get { return invAfter; }
        }

        public bool PostHolds
        {
            get { return postHolds; }
        }

        public bool IsFinished
        {
            get { return isFinished; }
        }

        public string RunButtonText
        {
            get { return isRunning ? "Пауза" : "Пуск"; }
        }

        public string VariantText
        {
            get { return "t = n − j = " + variantValue; }
        }

        public string VariantStatus
        {
            get
            {
                if (variantValue < 0)
                {
                    return "t < 0 — условие завершения нарушено";
                }
                if (stepCount == 0)
                {
                    return "t неотрицателен, шагов ещё не было";
                }
                return "t неотрицателен и строго убывает";
            }
        }

        public bool VariantOk
        {
            get { return variantValue >= 0; }
        }

        public string PostStatusText
        {
            get
            {
                if (!isFinished)
                {
                    return "Post: цикл ещё не завершён";
                }
                if (postHolds)
                {
                    return "Post выполнено: " + postSpecText;
                }
                return "Post нарушено: " + postSpecText;
            }
        }

        public string TraceText
        {
            get { return traceText; }
        }

        public string VariantLogText
        {
            get { return variantLogText; }
        }

        public string CodeText
        {
            get { return codeText; }
        }

        public string SpecText
        {
            get { return specText; }
        }

        public string PreSpecText
        {
            get { return preSpecText; }
        }

        public string PostSpecText
        {
            get { return postSpecText; }
        }

        public string WpInvariant
        {
            get { return wpInvariant; }
        }

        public string WpVariant
        {
            get { return wpVariant; }
        }

        public string DefText
        {
            get { return defText; }
        }

        public string SummaryText
        {
            get { return summaryText; }
            set
            {
                summaryText = value;
                OnPropertyChanged();
            }
        }

        public string InvariantWords { get; set; } = "";

        public string InvariantFormula { get; set; } = "";

        public string VariantFormula { get; set; } = "n - j";

        //заполняет массив случайными числами
        private void Generate()
        {
            isRunning = false;

            int count = (int)Math.Round(n);
            int low = (int)Math.Round(minValue);
            int high = (int)Math.Round(maxValue);
            if (high < low)
            {
                int temp = low;
                low = high;
                high = temp;
            }

            Random random = new Random();
            values = new int[count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = random.Next(low, high + 1);
            }

            Cells.Clear();
            for (int i = 0; i < values.Length; i++)
            {
                Cells.Add(new ArrayCellViewModel(i, values[i], OnCellValueChanged));
            }

            Reset();
        }

        //пользователь изменил ячейку таблицы
        private void OnCellValueChanged(int index, double value)
        {
            if (index < 0 || index >= values.Length)
            {
                return;
            }

            int newValue = (int)Math.Round(value);
            if (newValue < -50)
            {
                newValue = -50;
            }
            if (newValue > 50)
            {
                newValue = 50;
            }

            values[index] = newValue;
            Reset();
        }

        //возвращает цикл в начальное состояние
        private void Reset()
        {
            isRunning = false;
            stepCount = 0;
            isFinished = false;
            postHolds = false;
            traceText = "";
            variantLogText = "";

            if (values.Length == 0)
            {
                j = 0;
                res = 0;
                variantValue = 0;
                variantMax = 1;
                invBefore = true;
                invAfter = true;
                NotifyState();
                return;
            }

            if (selectedModeIndex == 2)
            {
                //для максимума префикса начинаем с первого элемента
                res = values[0];
                j = 1;
            }
            else
            {
                res = 0;
                j = 0;
            }

            variantValue = values.Length - j;
            variantMax = Math.Max(1, selectedModeIndex == 2 ? values.Length - 1 : values.Length);
            invBefore = CheckInvariant();
            invAfter = invBefore;
            isFinished = j >= values.Length;
            if (isFinished)
            {
                postHolds = CheckPost();
            }

            traceText = "старт: res = " + res + ", j = " + j;
            variantLogText = "t = n − j = " + variantValue;
            UpdateCellStates(-1);
            NotifyState();
        }

        //один шаг тела цикла
        private void Step()
        {
            if (values.Length == 0 || isFinished)
            {
                return;
            }

            invBefore = CheckInvariant();
            int oldJ = j;
            int oldRes = res;
            int oldVariant = values.Length - j;

            if (selectedModeIndex == 0)
            {
                res = res + values[j];
                j = j + 1;
            }
            else if (selectedModeIndex == 1)
            {
                if (values[j] > (int)threshold)
                {
                    res = res + 1;
                }
                j = j + 1;
            }
            else
            {
                res = Math.Max(res, values[j]);
                j = j + 1;
            }

            stepCount = stepCount + 1;
            invAfter = CheckInvariant();
            variantValue = values.Length - j;
            isFinished = j >= values.Length;
            if (isFinished)
            {
                postHolds = CheckPost();
            }

            traceText = "шаг " + stepCount + ": res " + oldRes + " → " + res + ", j " + oldJ + " → " + j + ", t " + oldVariant + " → " + variantValue + "\n" + traceText;
            variantLogText = "шаг " + stepCount + ": t = " + variantValue + (variantValue < oldVariant ? " — убывает" : " — не убывает") + "\n" + variantLogText;
            traceText = Shorten(traceText);
            variantLogText = Shorten(variantLogText);
            UpdateCellStates(isFinished ? -1 : oldJ);
            NotifyState();
        }

        //запуск и пауза прокрутки цикла
        private async void RunClicked()
        {
            if (isFinished)
            {
                return;
            }

            if (isRunning)
            {
                isRunning = false;
                OnPropertyChanged(nameof(RunButtonText));
                return;
            }

            isRunning = true;
            OnPropertyChanged(nameof(RunButtonText));
            while (isRunning && !isFinished)
            {
                Step();
                await Task.Delay(550);
            }
            isRunning = false;
            OnPropertyChanged(nameof(RunButtonText));
        }

        //раскрашивает ячейки массива
        private void UpdateCellStates(int changedIndex)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                CellState state = CellState.Normal;
                if (i == j)
                {
                    state = CellState.Current;
                }
                else if (i == changedIndex)
                {
                    state = CellState.Changed;
                }
                else if (i < j)
                {
                    state = CellState.InPrefix;
                }

                Cells[i].State = state;
                Cells[i].AboveThreshold = values[i] > (int)threshold;
            }
        }

        //проверка инварианта на текущем состоянии
        //это только подсветка для визуализации, формальные условия проверяет wp-модуль
        private bool CheckInvariant()
        {
            if (j < 0 || j > values.Length)
            {
                return false;
            }

            if (selectedModeIndex == 0)
            {
                return res == SumPrefix(j);
            }
            if (selectedModeIndex == 1)
            {
                return res == CountPrefix(j);
            }
            if (j < 1)
            {
                return false;
            }
            return res == MaxPrefix(j);
        }

        //проверка постусловия после цикла
        private bool CheckPost()
        {
            if (values.Length == 0)
            {
                return false;
            }

            if (selectedModeIndex == 0)
            {
                return res == SumPrefix(values.Length);
            }
            if (selectedModeIndex == 1)
            {
                return res == CountPrefix(values.Length);
            }
            return res == MaxPrefix(values.Length);
        }

        private int SumPrefix(int end)
        {
            int sum = 0;
            for (int i = 0; i < end; i++)
            {
                sum = sum + values[i];
            }
            return sum;
        }

        private int CountPrefix(int end)
        {
            int count = 0;
            for (int i = 0; i < end; i++)
            {
                if (values[i] > (int)threshold)
                {
                    count = count + 1;
                }
            }
            return count;
        }

        private int MaxPrefix(int end)
        {
            int max = values[0];
            for (int i = 1; i < end; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                }
            }
            return max;
        }

        //кнопка проверки условий корректности
        private void Verify()
        {
            //сами условия проверяет модуль wp, тут только заглушка для фронта
            SummaryText = "статусы условий заполнит wp-модуль (бэкенд)";
        }

        private void ApplyMode()
        {
            Proofs.Clear();
            SummaryText = "wp-проверка подключается отдельно";

            if (selectedModeIndex == 0)
            {
                ApplyPrefixSum();
            }
            else if (selectedModeIndex == 1)
            {
                ApplyCountGreater();
            }
            else
            {
                ApplyPrefixMax();
            }

            NotifySpec();
        }

        private void ApplyPrefixSum()
        {
            InvariantWords = "res равна сумме элементов a[0..j−1], и 0 ≤ j ≤ n.";
            InvariantFormula = "res == sum(a, 0, j) && 0 <= j && j <= n";
            codeText = "res := 0; j := 0;\nwhile (j < n) {\n    res := res + a[j];\n    j := j + 1;\n}";
            preSpecText = "n ≥ 0";
            postSpecText = "res = sum(a, 0, n)";
            specText = "Pre:  n ≥ 0\nInv:  res = Σ_{i<j} a[i] ∧ 0 ≤ j ≤ n\nPost: res = Σ_{i<n} a[i]\nt:    n − j";
            wpInvariant = "wp(S, Inv) = res + a[j] = Σ_{i<j+1} a[i]\n           ∧ 0 ≤ j+1 ≤ n";
            wpVariant = "wp(S, t' < t) = n − (j+1) < n − j";
            defText = "Def: n ≥ 0; 0 ≤ j < n — индекс a[j] в границах массива";

            AddProof("1. Инициализация",
                "n ≥ 0 ⇒ 0 = Σ пустого префикса ∧ 0 ≤ 0 ≤ n",
                "После res := 0 и j := 0 сумма по пустому префиксу равна нулю, поэтому инвариант истинен.");
            AddProof("2. Сохранение",
                "Inv ∧ j < n ⇒ res + a[j] = Σ_{i<j+1} a[i] ∧ 0 ≤ j+1 ≤ n",
                "Один шаг добавляет a[j] к сумме и сдвигает j вправо, инвариант остаётся истинным.");
            AddProof("3. Выход",
                "Inv ∧ j ≥ n ⇒ res = Σ_{i<n} a[i]",
                "Когда j = n, сумма по префиксу совпадает с суммой всего массива.");
            AddProof("4. Завершение",
                "Inv ∧ j < n ⇒ n − j ≥ 0 ∧ n − (j+1) < n − j",
                "Вариант n − j неотрицателен и строго убывает на каждом шаге.");
        }

        private void ApplyCountGreater()
        {
            InvariantWords = "res равно числу элементов a[0..j−1], которые больше T, и 0 ≤ j ≤ n.";
            InvariantFormula = "res == count_gt(a, 0, j, T) && 0 <= j && j <= n";
            codeText = "res := 0; j := 0;\nwhile (j < n) {\n    if (a[j] > T) { res := res + 1; }\n    j := j + 1;\n}";
            preSpecText = "n ≥ 0";
            postSpecText = "res = count_gt(a, 0, n, T)";
            specText = "Pre:  n ≥ 0\nInv:  res = |{i < j : a[i] > T}| ∧ 0 ≤ j ≤ n\nPost: res = |{i < n : a[i] > T}|\nt:    n − j";
            wpInvariant = "wp(S, Inv) = (a[j] > T ⇒ res+1 = |{i<j+1: a[i]>T}|)\n           ∧ (a[j] ≤ T ⇒ res = |{i<j+1: a[i]>T}|)\n           ∧ 0 ≤ j+1 ≤ n";
            wpVariant = "wp(S, t' < t) = n − (j+1) < n − j";
            defText = "Def: n ≥ 0; 0 ≤ j < n — индекс a[j] в границах массива; T — целое число";

            AddProof("1. Инициализация",
                "n ≥ 0 ⇒ 0 = |∅| ∧ 0 ≤ 0 ≤ n",
                "Для пустого префикса ни один элемент не больше T, поэтому res = 0.");
            AddProof("2. Сохранение",
                "Inv ∧ j < n ⇒ wp(S, Inv)",
                "Если a[j] > T, счётчик увеличивается на единицу, иначе остаётся прежним. В обоих случаях инвариант сохраняется.");
            AddProof("3. Выход",
                "Inv ∧ j ≥ n ⇒ res = |{i<n: a[i] > T}|",
                "Когда j = n, счётчик охватывает весь массив.");
            AddProof("4. Завершение",
                "Inv ∧ j < n ⇒ n − j ≥ 0 ∧ n − (j+1) < n − j",
                "Вариант n − j неотрицателен и строго убывает на каждом шаге.");
        }

        private void ApplyPrefixMax()
        {
            InvariantWords = "res — максимум элементов a[0..j−1], и 1 ≤ j ≤ n.";
            InvariantFormula = "res == max_prefix(a, 0, j) && 1 <= j && j <= n";
            codeText = "res := a[0]; j := 1;\nwhile (j < n) {\n    res := max(res, a[j]);\n    j := j + 1;\n}";
            preSpecText = "n ≥ 1";
            postSpecText = "res = max_prefix(a, 0, n)";
            specText = "Pre:  n ≥ 1\nInv:  res = max a[0..j) ∧ 1 ≤ j ≤ n\nPost: res = max a[0..n)\nt:    n − j";
            wpInvariant = "wp(S, Inv) = max(res, a[j]) = max a[0..j+1)\n           ∧ 1 ≤ j+1 ≤ n";
            wpVariant = "wp(S, t' < t) = n − (j+1) < n − j";
            defText = "Def: n ≥ 1; 1 ≤ j < n — индекс a[j] и элемент a[0] определены";

            AddProof("1. Инициализация",
                "n ≥ 1 ⇒ a[0] = max a[0..1) ∧ 1 ≤ 1 ≤ n",
                "Максимум одного элемента равен самому элементу, поэтому res := a[0] и j := 1 дают истинный инвариант.");
            AddProof("2. Сохранение",
                "Inv ∧ j < n ⇒ max(res, a[j]) = max a[0..j+1) ∧ 1 ≤ j+1 ≤ n",
                "Новый максимум префикса — это максимум из старого максимума и нового элемента a[j].");
            AddProof("3. Выход",
                "Inv ∧ j ≥ n ⇒ res = max a[0..n)",
                "Когда j = n, максимум префикса совпадает с максимумом всего массива.");
            AddProof("4. Завершение",
                "Inv ∧ j < n ⇒ n − j ≥ 0 ∧ n − (j+1) < n − j",
                "Вариант n − j неотрицателен и строго убывает на каждом шаге.");
        }

        private void AddProof(string title, string formula, string explanation)
        {
            Proofs.Add(new ProofRowViewModel
            {
                Title = title,
                Formula = formula,
                Explanation = explanation
            });
        }

        private void NotifySpec()
        {
            OnPropertyChanged(nameof(InvariantWords));
            OnPropertyChanged(nameof(InvariantFormula));
            OnPropertyChanged(nameof(VariantFormula));
            OnPropertyChanged(nameof(CodeText));
            OnPropertyChanged(nameof(SpecText));
            OnPropertyChanged(nameof(PreSpecText));
            OnPropertyChanged(nameof(PostSpecText));
            OnPropertyChanged(nameof(WpInvariant));
            OnPropertyChanged(nameof(WpVariant));
            OnPropertyChanged(nameof(DefText));
        }

        private void NotifyState()
        {
            OnPropertyChanged(nameof(J));
            OnPropertyChanged(nameof(Res));
            OnPropertyChanged(nameof(StepCount));
            OnPropertyChanged(nameof(VariantValue));
            OnPropertyChanged(nameof(VariantMax));
            OnPropertyChanged(nameof(VariantText));
            OnPropertyChanged(nameof(VariantStatus));
            OnPropertyChanged(nameof(VariantOk));
            OnPropertyChanged(nameof(InvBefore));
            OnPropertyChanged(nameof(InvAfter));
            OnPropertyChanged(nameof(IsFinished));
            OnPropertyChanged(nameof(PostHolds));
            OnPropertyChanged(nameof(PostStatusText));
            OnPropertyChanged(nameof(TraceText));
            OnPropertyChanged(nameof(VariantLogText));
            OnPropertyChanged(nameof(RunButtonText));
        }

        //оставляет в логе только последние строки
        private static string Shorten(string text)
        {
            string[] lines = text.Split('\n');
            if (lines.Length <= 12)
            {
                return text;
            }
            return string.Join("\n", lines, 0, 12);
        }
    }
}

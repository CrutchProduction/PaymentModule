using PaymentClassLibrary.Loops;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PaymentModule.ViewModels
{
    public class LoopLabViewModel : ViewModelBase
    {
        private double minAmount = 100;
        private double maxAmount = 500;
        private double threshold = 150;
        private int selectedModeIndex;

        //состояние пошагового просмотра
        private int j;
        private int res;
        private int variantValue;
        private int variantMax = 1;
        private bool isRunning;
        private int playbackVersion;
        private LoopAnalysis? currentAnalysis;
        private bool invBefore = true;
        private bool invAfter = true;
        private bool postHolds;
        private bool isFinished;

        //результат расчёта
        private string resultText = "Нажмите «Рассчитать», чтобы получить результат.";
        private bool hasResult;

        //текущий шаг для панели просмотра
        private int currentStepIndex = -1;
        private string currentStepTitle = "Нажмите «Рассчитать»";
        private string currentPaymentText = "—";
        private string currentResultText = "—";
        private string currentVariantText = "—";
        private string currentVariantStatus = "—";
        private string currentInvBeforeText = "—";
        private string currentInvAfterText = "—";

        //тексты выбранного расчёта
        private string codeText = "";
        private string specText = "";
        private string postSpecText = "";
        private string wpInvariant = "";
        private string wpVariant = "";
        private string defText = "";
        private string wpResultText = "Результат wp от бэкенда: ожидается.";

        public LoopLabViewModel()
        {
            GenerateCommand = new RelayCommand(GenerateRandom, () => true);
            CalculateCommand = new RelayCommand(Calculate, () => true);
            StepCommand = new RelayCommand(Step, () => true);
            RunCommand = new RelayCommand(RunClicked, () => true);
            ResetCommand = new RelayCommand(ResetViewer, () => true);

            //стартовый пример из задания
            Payments.Add(new ArrayCellViewModel(0, 100, OnPaymentChanged));
            Payments.Add(new ArrayCellViewModel(1, 200, OnPaymentChanged));
            Payments.Add(new ArrayCellViewModel(2, 300, OnPaymentChanged));

            ApplyMode();
            ResetViewer();
        }

        public ObservableCollection<ArrayCellViewModel> Payments { get; } = new ObservableCollection<ArrayCellViewModel>();
        public ObservableCollection<PaymentStepViewModel> Steps { get; } = new ObservableCollection<PaymentStepViewModel>();
        public ObservableCollection<ProofRowViewModel> Proofs { get; } = new ObservableCollection<ProofRowViewModel>();

        public RelayCommand GenerateCommand { get; }
        public RelayCommand CalculateCommand { get; }
        public RelayCommand StepCommand { get; }
        public RelayCommand RunCommand { get; }
        public RelayCommand ResetCommand { get; }

        //количество платежей в таблице
        public double PaymentCount
        {
            get { return Payments.Count; }
            set
            {
                if (double.IsNaN(value))
                {
                    return;
                }
                int newValue = (int)Math.Round(Math.Clamp(value, 1, 24));
                if (Payments.Count == newValue)
                {
                    return;
                }
                ResizePayments(newValue);
            }
        }

        public double MinAmount
        {
            get { return minAmount; }
            set
            {
                if (double.IsNaN(value))
                {
                    return;
                }
                minAmount = Math.Round(Math.Clamp(value, 0, 1000000));
                OnPropertyChanged();
            }
        }

        public double MaxAmount
        {
            get { return maxAmount; }
            set
            {
                if (double.IsNaN(value))
                {
                    return;
                }
                maxAmount = Math.Round(Math.Clamp(value, 0, 1000000));
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
                threshold = Math.Round(Math.Clamp(value, 0, 1000000));
                OnPropertyChanged();
                InvalidateResult();
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
                OnPropertyChanged(nameof(IsCountMode));
                ApplyMode();
                InvalidateResult();
            }
        }

        public bool IsCountMode
        {
            get { return selectedModeIndex == 1; }
        }

        public string ResultText
        {
            get { return resultText; }
            private set
            {
                resultText = value;
                OnPropertyChanged();
            }
        }

        public bool HasResult
        {
            get { return hasResult; }
            private set
            {
                hasResult = value;
                OnPropertyChanged();
            }
        }

        public string HistoryHint
        {
            get
            {
                if (Steps.Count == 0)
                {
                    return "История появится после нажатия «Рассчитать».";
                }
                return "Текущий шаг подсвечивается.";
            }
        }

        public string InvariantWords { get; set; } = "";

        private string invariantFormula = "";
        public string InvariantFormula
        {
            get => invariantFormula;
            set { if (invariantFormula == value) return; invariantFormula = value; OnPropertyChanged(); InvalidateResult(); }
        }

        private string variantFormula = "n - j";
        public string VariantFormula
        {
            get => variantFormula;
            set { if (variantFormula == value) return; variantFormula = value; OnPropertyChanged(); InvalidateResult(); }
        }

        public string CodeText
        {
            get { return codeText; }
        }

        public string SpecText
        {
            get { return specText; }
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

        public string WpResultText
        {
            get { return wpResultText; }
        }

        public int J
        {
            get { return j; }
        }

        public int Res
        {
            get { return res; }
        }

        public int VariantValue
        {
            get { return variantValue; }
        }

        public int VariantMax
        {
            get { return variantMax; }
        }

        public bool VariantOk
        {
            get { return currentStepIndex >= 0 && currentStepIndex < Steps.Count && Steps[currentStepIndex].VariantOk; }
        }

        public string VariantText
        {
            get { return "Осталось платежей: " + variantValue; }
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

        public string PostStatusText
        {
            get
            {
                if (!isFinished)
                {
                    return "Post: цикл ещё не завершён.";
                }
                if (postHolds)
                {
                    return "Post выполнено: res = " + Money(res) + " — " + postSpecText + ".";
                }
                return "Post нарушено: " + postSpecText + ".";
            }
        }

        public string RunButtonText
        {
            get { return isRunning ? "Пауза" : "Пуск"; }
        }

        public int CurrentStepIndex
        {
            get { return currentStepIndex; }
        }

        public string CurrentStepTitle
        {
            get { return currentStepTitle; }
        }

        public string CurrentPaymentText
        {
            get { return currentPaymentText; }
        }

        public string CurrentResultText
        {
            get { return currentResultText; }
        }

        public string CurrentVariantText
        {
            get { return currentVariantText; }
        }

        public string CurrentVariantStatus
        {
            get { return currentVariantStatus; }
        }

        public string CurrentInvBeforeText
        {
            get { return currentInvBeforeText; }
        }

        public string CurrentInvAfterText
        {
            get { return currentInvAfterText; }
        }

        //сумма конкретного платежа
        private int Amount(int index)
        {
            return (int)Math.Round(Payments[index].Value);
        }

        private int InitialJ()
        {
            //для максимума первый платёж уже взят как начальный
            return selectedModeIndex == 2 ? 1 : 0;
        }

        private int InitialResult()
        {
            return selectedModeIndex == 2 ? Amount(0) : 0;
        }

        private string Money(int value)
        {
            return value + " ₽";
        }

        private void ResizePayments(int count)
        {
            while (Payments.Count > count)
            {
                Payments.RemoveAt(Payments.Count - 1);
            }
            while (Payments.Count < count)
            {
                Payments.Add(new ArrayCellViewModel(Payments.Count, 0, OnPaymentChanged));
            }
            OnPropertyChanged(nameof(PaymentCount));
            InvalidateResult();
        }

        //случайные суммы для всех платежей
        private void GenerateRandom()
        {
            int count = Payments.Count;
            int low = (int)Math.Round(minAmount);
            int high = (int)Math.Round(maxAmount);
            if (high < low)
            {
                int temp = low;
                low = high;
                high = temp;
            }

            Random random = new Random();
            Payments.Clear();
            for (int i = 0; i < count; i++)
            {
                Payments.Add(new ArrayCellViewModel(i, random.Next(low, high + 1), OnPaymentChanged));
            }

            OnPropertyChanged(nameof(PaymentCount));
            InvalidateResult();
        }

        //пользователь изменил сумму платежа
        private void OnPaymentChanged(int index, double value)
        {
            InvalidateResult();
        }

        //сбрасывает результат и историю, если данные поменялись
        private void InvalidateResult()
        {
            currentAnalysis = null;
            HasResult = false;
            wpInvariant = "wp(S, Inv): ещё не вычислено.";
            wpVariant = "wp варианта: ещё не вычислено.";
            defText = "Определённость проверяется при расчёте.";
            wpResultText = "После изменения данных или формул выполните расчёт заново.";
            foreach (var proof in Proofs)
            {
                proof.StatusText = "Не проверено";
                proof.StatusBrush = new SolidColorBrush(Colors.Gray);
            }
            NotifySpec();
            ResultText = "Нажмите «Рассчитать», чтобы получить результат.";
            Steps.Clear();
            ResetViewer();
            OnPropertyChanged(nameof(HistoryHint));
        }

        // Библиотека возвращает результат, историю и вычисленные wp.
        private void Calculate()
        {
            isRunning = false;
            playbackVersion++;
            try
            {
                LoopAnalysis analysis = PaymentLoop.Analyze(Payments.Select((_, i) => Amount(i)).ToArray(),
                    selectedModeIndex, (int)threshold, InvariantFormula, VariantFormula);
                Steps.Clear();
                for (int i = 0; i < analysis.Steps.Count; i++)
                {
                    LoopStep step = analysis.Steps[i];
                    Steps.Add(new PaymentStepViewModel {
                        IsStart = i == 0,
                        Title = i == 0 ? "Старт" : "Шаг " + i,
                        PaymentText = i == 0 ? (selectedModeIndex == 2 ? Money(Amount(0)) : "—") : Money(Amount(step.Index - 1)),
                        Result = step.Result,
                        ResultText = selectedModeIndex == 1 ? step.Result.ToString() : Money(step.Result),
                        InvBeforeOk = step.InvariantBefore, InvAfterOk = step.InvariantAfter,
                        VariantBefore = step.VariantBefore, VariantAfter = step.VariantAfter,
                        VariantOk = step.VariantOk
                    });
                }
                Proofs[0].Formula = "n >= 1 => " + analysis.InitializationWp.Wp;
                Proofs[1].Formula = "(" + InvariantFormula + ") && j < n => " + analysis.InvariantWp.Wp;
                Proofs[2].Formula = "(" + InvariantFormula + ") && j >= n => " + postSpecText;
                Proofs[3].Formula = "(" + InvariantFormula + ") && j < n => (" + VariantFormula +
                    ") >= 0 && " + analysis.VariantWp.Wp;
                wpInvariant = "wp(S, Inv) = " + analysis.InvariantWp.Wp;
                wpVariant = "wp(S, t >= 0 && t < old_t) = " + analysis.VariantWp.Wp;
                defText = "Индексы и границы префиксов проверены при выполнении.\n" +
                    analysis.InvariantWp.Defined + "\n" + analysis.VariantWp.Defined;
                wpResultText = "Проверки выполнены на введённых платежах. Это не доказательство для всех массивов.\n" +
                    "old_t — значение варианта перед шагом.\n" + analysis.InvariantWp.Steps;
                bool[] checks = { analysis.Initialization, analysis.Preservation, analysis.Exit, analysis.Termination };
                for (int i = 0; i < Proofs.Count; i++)
                {
                    Proofs[i].StatusText = checks[i] ? "Выполнено на этих данных" : "Нарушено на этих данных";
                    Proofs[i].StatusBrush = new SolidColorBrush(checks[i] ? Colors.Green : Colors.Red);
                }
                currentAnalysis = analysis;
                ResultText = BuildResult(analysis.Result);
                HasResult = true;
                ResetViewer();
                NotifySpec();
                OnPropertyChanged(nameof(HistoryHint));
            }
            catch (Exception error) when (error is FormatException || error is ArgumentException ||
                                          error is ArithmeticException || error is InvalidOperationException)
            {
                InvalidateResult();
                ResultText = "Не удалось выполнить проверку: " + error.Message;
                wpResultText = ResultText;
                NotifySpec();
            }
        }
        private string BuildResult(int value)
        {
            if (selectedModeIndex == 0)
            {
                return "На все платежи потребуется " + value + " ₽.";
            }
            if (selectedModeIndex == 1)
            {
                return "Платежей больше " + (int)threshold + " ₽: " + value + ".";
            }
            return "Самый крупный платёж: " + value + " ₽.";
        }

        // Просмотр готовой истории без повторного выполнения расчёта во фронте.
        private void Step()
        {
            if (!HasResult || isFinished || currentStepIndex < 0 ||
                currentStepIndex + 1 >= Steps.Count) return;
            int next = currentStepIndex + 1;
            j++;
            LoopStepView(next);
            UpdatePaymentStates(j - 1);
            NotifyState();
        }

        private void LoopStepView(int index)
        {
            res = Steps[index].Result;
            SetCurrentStep(index);
            variantValue = Steps[index].VariantAfter;
            isFinished = index == Steps.Count - 1;
            postHolds = isFinished && currentAnalysis != null && currentAnalysis.Exit;
        }
        //пуск и пауза пошагового просмотра
        private async void RunClicked()
        {
            if (Steps.Count == 0 || isFinished)
            {
                return;
            }

            if (isRunning)
            {
                isRunning = false;
                playbackVersion++;
                OnPropertyChanged(nameof(RunButtonText));
                return;
            }

            int version = ++playbackVersion;
            isRunning = true;
            OnPropertyChanged(nameof(RunButtonText));
            while (isRunning && !isFinished && version == playbackVersion)
            {
                Step();
                await Task.Delay(700);
            }
            if (version == playbackVersion)
            {
                isRunning = false;
                OnPropertyChanged(nameof(RunButtonText));
            }
        }

        //возвращает просмотр в начало
        private void ResetViewer()
        {
            isRunning = false;
            playbackVersion++;
            j = InitialJ();
            res = InitialResult();
            variantValue = Payments.Count - j;
            variantMax = Math.Max(1, Payments.Count);
            invBefore = InvariantAt(res, j);
            invAfter = invBefore;
            isFinished = j >= Payments.Count;
            postHolds = isFinished && currentAnalysis != null && currentAnalysis.Exit;

            UpdatePaymentStates(-1);
            SetCurrentStep(Steps.Count > 0 ? 0 : -1);
            if (Steps.Count > 0)
            {
                variantValue = Steps[0].VariantAfter;
                variantMax = Math.Max(1, Steps.Max(step => Math.Max(step.VariantBefore, step.VariantAfter)));
            }
            NotifyState();
        }

        //подсветка текущего шага и заполнение панели просмотра
        private void SetCurrentStep(int index)
        {
            currentStepIndex = index;

            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].IsCurrent = i == index;
            }

            if (index >= 0 && index < Steps.Count)
            {
                PaymentStepViewModel step = Steps[index];
                currentStepTitle = step.IsStart ? "Старт" : "Шаг " + index + " из " + (Payments.Count - InitialJ());
                currentPaymentText = step.PaymentText;
                currentResultText = step.ResultText;
                currentVariantText = step.VariantText;
                currentVariantStatus = step.IsStart ? "сколько платежей осталось" : (step.VariantOk ? "уменьшился и остался ≥ 0" : "нарушено");
                currentInvBeforeText = step.InvBeforeText;
                currentInvAfterText = step.InvAfterText;
                invBefore = step.InvBeforeOk;
                invAfter = step.InvAfterOk;
            }
            else
            {
                currentStepTitle = "Нажмите «Рассчитать»";
                currentPaymentText = "—";
                currentResultText = "—";
                currentVariantText = "—";
                currentVariantStatus = "—";
                currentInvBeforeText = "—";
                currentInvAfterText = "—";
            }

            OnPropertyChanged(nameof(CurrentStepIndex));
            OnPropertyChanged(nameof(CurrentStepTitle));
            OnPropertyChanged(nameof(CurrentPaymentText));
            OnPropertyChanged(nameof(CurrentResultText));
            OnPropertyChanged(nameof(CurrentVariantText));
            OnPropertyChanged(nameof(CurrentVariantStatus));
            OnPropertyChanged(nameof(CurrentInvBeforeText));
            OnPropertyChanged(nameof(CurrentInvAfterText));
            OnPropertyChanged(nameof(InvBefore));
            OnPropertyChanged(nameof(InvAfter));
        }

        //раскрашивает таблицу платежей
        private void UpdatePaymentStates(int changedIndex)
        {
            for (int i = 0; i < Payments.Count; i++)
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

                Payments[i].State = state;
                Payments[i].AboveThreshold = selectedModeIndex == 1 && Amount(i) > (int)threshold;
            }
        }

        private bool InvariantAt(int value, int index)
        {
            try
            {
                return PaymentLoop.CheckInvariant(InvariantFormula,
                    Payments.Select((_, i) => Amount(i)).ToArray(), (int)threshold, index, value);
            }
            catch (Exception error) when (error is FormatException || error is ArithmeticException)
            {
                return false;
            }
        }
        private void ApplyMode()
        {
            Proofs.Clear();
            VariantFormula = "n - j";

            if (selectedModeIndex == 0)
            {
                ApplySum();
            }
            else if (selectedModeIndex == 1)
            {
                ApplyCount();
            }
            else
            {
                ApplyMax();
            }

            NotifySpec();
        }

        private void ApplySum()
        {
            InvariantWords = "res — сумма уже обработанных платежей, и 0 ≤ j ≤ n.";
            InvariantFormula = "res == sum(a, 0, j) && 0 <= j && j <= n";
            codeText = "res := 0; j := 0;\nwhile (j < n) {\n    res := res + a[j];\n    j := j + 1;\n}";
            specText = "Pre:  n ≥ 1\nInv:  res = Σ_{i<j} a[i] ∧ 0 ≤ j ≤ n\nPost: res = Σ_{i<n} a[i]\nt:    n − j";
            postSpecText = "итог равен сумме всех платежей";
            wpInvariant = "wp(S, Inv) = res + a[j] = Σ_{i<j+1} a[i]\n           ∧ 0 ≤ j+1 ≤ n";
            wpVariant = "wp(S, t' < t) = n − (j+1) < n − j";
            defText = "Def: n ≥ 1; 0 ≤ j < n — индекс a[j] в границах массива";

            AddProof("1. Инициализация",
                "n ≥ 1 ⇒ 0 = Σ пустого префикса ∧ 0 ≤ 0 ≤ n",
                "До цикла ни один платёж не обработан, поэтому итог равен нулю.");
            AddProof("2. Сохранение",
                "Inv ∧ j < n ⇒ res + a[j] = Σ_{i<j+1} a[i] ∧ 0 ≤ j+1 ≤ n",
                "Один шаг добавляет сумму текущего платежа к итогу и сдвигает j вправо.");
            AddProof("3. Выход",
                "Inv ∧ j ≥ n ⇒ res = Σ_{i<n} a[i]",
                "Когда все платежи обработаны, итог равен сумме всего массива.");
            AddProof("4. Завершение",
                "Inv ∧ j < n ⇒ n − j ≥ 0 ∧ n − (j+1) < n − j",
                "Вариант n − j неотрицателен и строго убывает на каждом шаге.");
        }

        private void ApplyCount()
        {
            InvariantWords = "res — число уже обработанных платежей больше T, и 0 ≤ j ≤ n.";
            InvariantFormula = "res == count_gt(a, 0, j, T) && 0 <= j && j <= n";
            codeText = "res := 0; j := 0;\nwhile (j < n) {\n    if (a[j] > T) { res := res + 1; }\n    j := j + 1;\n}";
            specText = "Pre:  n ≥ 1\nInv:  res = |{i < j : a[i] > T}| ∧ 0 ≤ j ≤ n\nPost: res = |{i < n : a[i] > T}|\nt:    n − j";
            postSpecText = "счётчик равен числу платежей больше T";
            wpInvariant = "wp(S, Inv) = (a[j] > T ⇒ res+1 = |{i<j+1: a[i]>T}|)\n           ∧ (a[j] ≤ T ⇒ res = |{i<j+1: a[i]>T}|)\n           ∧ 0 ≤ j+1 ≤ n";
            wpVariant = "wp(S, t' < t) = n − (j+1) < n − j";
            defText = "Def: n ≥ 1; 0 ≤ j < n — индекс a[j] в границах массива; T — целое число";

            AddProof("1. Инициализация",
                "n ≥ 1 ⇒ 0 = |∅| ∧ 0 ≤ 0 ≤ n",
                "До цикла ни один платёж не обработан, счётчик равен нулю.");
            AddProof("2. Сохранение",
                "Inv ∧ j < n ⇒ wp(S, Inv)",
                "Если текущий платёж больше T, счётчик увеличивается на единицу, иначе не меняется.");
            AddProof("3. Выход",
                "Inv ∧ j ≥ n ⇒ res = |{i<n: a[i] > T}|",
                "Когда все платежи обработаны, счётчик охватывает весь массив.");
            AddProof("4. Завершение",
                "Inv ∧ j < n ⇒ n − j ≥ 0 ∧ n − (j+1) < n − j",
                "Вариант n − j неотрицателен и строго убывает на каждом шаге.");
        }

        private void ApplyMax()
        {
            InvariantWords = "res — максимум уже обработанных платежей, и 1 ≤ j ≤ n.";
            InvariantFormula = "res == max_prefix(a, 0, j) && 1 <= j && j <= n";
            codeText = "res := a[0]; j := 1;\nwhile (j < n) {\n    res := max(res, a[j]);\n    j := j + 1;\n}";
            specText = "Pre:  n ≥ 1\nInv:  res = max a[0..j) ∧ 1 ≤ j ≤ n\nPost: res = max a[0..n)\nt:    n − j";
            postSpecText = "итог равен максимуму всех платежей";
            wpInvariant = "wp(S, Inv) = max(res, a[j]) = max a[0..j+1)\n           ∧ 1 ≤ j+1 ≤ n";
            wpVariant = "wp(S, t' < t) = n − (j+1) < n − j";
            defText = "Def: n ≥ 1; 1 ≤ j < n — a[0] и a[j] определены";

            AddProof("1. Инициализация",
                "n ≥ 1 ⇒ a[0] = max a[0..1) ∧ 1 ≤ 1 ≤ n",
                "Первый платёж сразу становится начальным максимумом.");
            AddProof("2. Сохранение",
                "Inv ∧ j < n ⇒ max(res, a[j]) = max a[0..j+1) ∧ 1 ≤ j+1 ≤ n",
                "Новый максимум — это максимум из старого максимума и текущего платежа.");
            AddProof("3. Выход",
                "Inv ∧ j ≥ n ⇒ res = max a[0..n)",
                "Когда все платежи обработаны, максимум префикса равен максимуму всего массива.");
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
            OnPropertyChanged(nameof(PostSpecText));
            OnPropertyChanged(nameof(WpInvariant));
            OnPropertyChanged(nameof(WpVariant));
            OnPropertyChanged(nameof(DefText));
            OnPropertyChanged(nameof(WpResultText));
        }

        private void NotifyState()
        {
            OnPropertyChanged(nameof(J));
            OnPropertyChanged(nameof(Res));
            OnPropertyChanged(nameof(VariantValue));
            OnPropertyChanged(nameof(VariantMax));
            OnPropertyChanged(nameof(VariantOk));
            OnPropertyChanged(nameof(VariantText));
            OnPropertyChanged(nameof(InvBefore));
            OnPropertyChanged(nameof(InvAfter));
            OnPropertyChanged(nameof(PostHolds));
            OnPropertyChanged(nameof(PostStatusText));
            OnPropertyChanged(nameof(IsFinished));
            OnPropertyChanged(nameof(RunButtonText));
        }
    }
}

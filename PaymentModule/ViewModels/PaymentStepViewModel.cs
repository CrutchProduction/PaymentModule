namespace PaymentModule.ViewModels
{
    public class PaymentStepViewModel : ViewModelBase
    {
        private bool isCurrent;

        public bool IsStart { get; set; }
        public string Title { get; set; } = "";
        public string PaymentText { get; set; } = "";
        public string ResultText { get; set; } = "";

        public bool InvBeforeOk { get; set; }
        public bool InvAfterOk { get; set; }

        public string InvBeforeText
        {
            get { return InvBeforeOk ? "верен" : "нарушен"; }
        }

        public string InvAfterText
        {
            get { return InvAfterOk ? "верен" : "нарушен"; }
        }

        public int VariantBefore { get; set; }
        public int VariantAfter { get; set; }

        public bool VariantOk { get; set; }

        public string VariantText
        {
            get
            {
                if (VariantBefore == VariantAfter)
                {
                    return VariantBefore.ToString();
                }
                return VariantBefore + " → " + VariantAfter;
            }
        }

        public string VariantCheckText
        {
            get
            {
                if (IsStart)
                {
                    return "—";
                }
                return VariantOk ? "✓" : "✗";
            }
        }

        public bool IsCurrent
        {
            get { return isCurrent; }
            set
            {
                if (isCurrent == value)
                {
                    return;
                }
                isCurrent = value;
                OnPropertyChanged();
            }
        }
    }
}

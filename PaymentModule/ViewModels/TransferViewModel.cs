using PaymentClassLibrary;
using System;

namespace PaymentModule.ViewModels
{
    public class TransferViewModel : OperationViewModel
    {
        private string recipientText = "";
        public string RecipientText
        {
            get { return recipientText; }
            set { recipientText = value; Refresh(); }
        }
        public bool RecipientValid
        {
            get
            {
                Account? recipient = FindRecipient();
                return recipient != null && recipient != main.CurrentAccount && recipient.GetMoneyTypeId() == main.CurrentAccount.GetMoneyTypeId();
            }
        }
        public bool AmountValid
        {
            get
            {
                float amount;
                if (!TryGetAmount(out amount)) { return false; }
                return main.CurrentAccount.GetMoneyAmount() > amount && main.CurrentAccount.GetMoneyAmount() - amount < main.CurrentAccount.GetMoneyAmount();
            }
        }
        public override bool Precondition
        {
            get
            {
                if (!AccountValid || !RecipientValid || !AmountValid) { return false; }
                float amount;
                TryGetAmount(out amount);
                float balance = FindRecipient()!.GetMoneyAmount();
                return float.IsFinite(balance + amount) && balance + amount > balance;
            }
        }
        protected override Account RelatedAccount
        {
            get
            {
                Account? recipient = FindRecipient();
                if (recipient == null) { return main.CurrentAccount; }
                return recipient;
            }
        }

        /// <summary>Создаёт модель перевода.</summary>
        public TransferViewModel(MainViewModel main) : base(main) { }

        /// <summary>Ищет получателя по введённому номеру.</summary>
        private Account? FindRecipient()
        {
            int id;
            if (!int.TryParse(recipientText, out id)) { return null; }
            return AccountsManager.FindAccountById(id);
        }

        /// <summary>Переводит деньги и проверяет оба баланса.</summary>
        protected override bool Apply()
        {
            float amount;
            TryGetAmount(out amount);
            Account sender = main.CurrentAccount;
            Account? recipient = FindRecipient();
            float oldSender = sender.GetMoneyAmount();
            float oldRecipient = recipient!.GetMoneyAmount();
            sender.RemoveMoney(amount);
            recipient.AddMoney(amount);
            // Оба счёта изменяются на одну и ту же сумму.
            return sender.GetMoneyAmount() == oldSender - amount
                && recipient.GetMoneyAmount() == oldRecipient + amount
                && Math.Abs((double)sender.GetMoneyAmount() + recipient.GetMoneyAmount() - oldSender - oldRecipient) < 0.01;
        }
    }
}


using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentModule.ViewModels;

namespace PaymentClassLibTest
{
    [TestClass]
    public class RelayCommandTests
    {
        [TestMethod]
        public void CanExecute_TrueCondition_ReturnsTrue()
        {
            var cmd = new RelayCommand(() => { }, () => true);

            Assert.IsTrue(cmd.CanExecute(null));
        }

        [TestMethod]
        public void CanExecute_FalseCondition_ReturnsFalse()
        {
            var cmd = new RelayCommand(() => { }, () => false);

            Assert.IsFalse(cmd.CanExecute(null));
        }

        [TestMethod]
        public void Execute_WhenAllowed_InvokesAction()
        {
            int counter = 0;
            var cmd = new RelayCommand(() => counter++, () => true);

            cmd.Execute(null);

            Assert.AreEqual(1, counter);
        }

        [TestMethod]
        public void Execute_WhenBlocked_DoesNotInvokeAction()
        {
            int counter = 0;
            var cmd = new RelayCommand(() => counter++, () => false);

            cmd.Execute(null);

            Assert.AreEqual(0, counter);
        }
    }
}
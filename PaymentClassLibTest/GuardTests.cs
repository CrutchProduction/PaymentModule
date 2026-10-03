using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Validations;

namespace PaymentClassLibTest
{
    [TestClass]
    public class GuardTests
    {
        [TestMethod]
        public void Requires_TrueCondition_DoesNotThrow()
        {
            Guard.Requires(true, "не должно бросать");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Requires_FalseCondition_Throws()
        {
            Guard.Requires(false, "тестовое сообщение");
        }

        [TestMethod]
        public void Requires_FalseCondition_MessageMatches()
        {
            try
            {
                Guard.Requires(false, "текст ошибки");
                Assert.Fail("Ожидалось исключение.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("текст ошибки", ex.Message);
            }
        }
    }
}
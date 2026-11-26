using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentClassLibrary.Loops;

namespace PaymentClassLibTest
{
    [TestClass]
    public class PaymentLoopTests
    {
        private static readonly string[] Invariants = {
            "res == sum(a, 0, j) && 0 <= j && j <= n",
            "res == count_gt(a, 0, j, T) && 0 <= j && j <= n",
            "res == max_prefix(a, 0, j) && 1 <= j && j <= n"
        };

        [TestMethod]
        public void AllModesMatchExpectedResultsOnSmallArrays()
        {
            for (int length = 1; length <= 4; length++)
            for (int mask = 0; mask < Math.Pow(3, length); mask++)
            {
                int encoded = mask;
                int[] a = new int[length];
                for (int i = 0; i < length; i++) { a[i] = encoded % 3; encoded /= 3; }
                for (int mode = 0; mode < 3; mode++)
                {
                    var r = PaymentLoop.Analyze(a, mode, 1, Invariants[mode], "n - j");
                    int expected = mode == 0 ? a.Sum() : mode == 1 ? a.Count(x => x > 1) : a.Max();
                    Assert.AreEqual(expected, r.Result);
                    Assert.IsTrue(r.Initialization && r.Preservation && r.Exit && r.Termination);
                    Assert.AreEqual(0, r.Steps.Last().VariantAfter);
                    Assert.IsTrue(r.Steps.Skip(1).All(x => x.VariantAfter < x.VariantBefore));
                }
            }
        }

        [TestMethod]
        public void WrongInvariantAndConstantVariantAreReported()
        {
            var wrong = PaymentLoop.Analyze(new[] { 100, 200 }, 0, 0, "res == 0", "n-j");
            Assert.IsFalse(wrong.Preservation);
            Assert.IsFalse(wrong.Exit);
            var constant = PaymentLoop.Analyze(new[] { 100, 200 }, 0, 0, Invariants[0], "n");
            Assert.IsFalse(constant.Termination);
            Assert.IsTrue(constant.Steps.Skip(1).All(x => !x.VariantOk));
        }

        [TestMethod]
        public void FormulaErrorsAreRejected()
        {
            foreach (string formula in new[] { "", "unknown == 1", "a[n] == 0", "res == 1 / 0", "sum(a, -1, j) == 0" })
                Assert.ThrowsException<FormatException>(() =>
                    PaymentLoop.Analyze(new[] { 100 }, 0, 0, formula, "n-j"));
            Assert.ThrowsException<FormatException>(() =>
                PaymentLoop.Analyze(new[] { 100, 200 }, 0, 0, Invariants[0], "0.5"));
        }

        [TestMethod]
        public void WpSupportsArraySubstitutionAndMultipleArguments()
        {
            var r = WpEngine.Analyze("j := j + 1;", "res == sum(a, 0, j) && a[j] >= 0");
            StringAssert.Contains(r.Wp, "sum(a, 0, (j + 1))");
            StringAssert.Contains(r.Wp, "a[(j + 1)]");
        }
    }
}
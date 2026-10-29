using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PaymentClassLibrary.Models;
using PaymentClassLibrary.Managers;
using PaymentClassLibrary.Validations;

namespace PaymentClassLibTest
{
    [TestClass]
    public class WpEngineTests
    {
        [TestMethod]
        public void Wp_GuaranteesPost_ForSequence()
        {
            var code = "if (x1 >= x2) { max := x1; } else { max := x2; }";
            var post = "max >= x1 && max >= x2";

            var result = WpEngine.Analyze(code, post);

            Assert.AreEqual(result.Hoare, "{ (((x1 >= x2) && ((x1 >= x1) && (x1 >= x2))) || (!((x1 >= x2)) && ((x2 >= x1) && (x2 >= x2)))) }  if (x1 >= x2) { max := x1; } else { max := x2; }  { max >= x1 && max >= x2 }");
        }

    }
}

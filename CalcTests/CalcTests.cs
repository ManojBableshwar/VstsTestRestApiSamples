//Simple umit tests for the CalcApp

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CalcApp;

namespace CalcTest
{
    [TestClass]
    public class CalcTests
    {
        [TestMethod]
        public void TestAdd()
        {
            Calc cal1 = new Calc();
            Assert.AreEqual(cal1.Add(3, 4), 7, "3 + 4 must be 7");
        }

        [TestMethod]
        public void TestSub()
        {
            Calc cal1 = new Calc();
            Assert.AreEqual(cal1.Substract(10, 4), 6, "10 - 4 must be 6");
        }

        [TestMethod]
        public void TestMul()
        {
            Calc cal1 = new Calc();
            Assert.AreEqual(cal1.Multiply(3, 4), 12, "3 * 4 must be 12");
        }

        [TestMethod]
        public void TestDiv()
        {
            Calc cal1 = new Calc();

            //doing something dumb to get an exception...
            Assert.AreEqual(cal1.Divide(8, 0), "infinity", "8 / 0 must be infinity");
        }
    }
}

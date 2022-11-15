using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S;

namespace BackendTest.ServerCommon
{
    [TestClass]
    public class MadIdTest
    {
        [TestMethod]
        public void IntBoundTest()
        {
            var min = new MadId((uint) MadId.MinValue);
            var max = new MadId((uint) MadId.MaxValue);

            Assert.AreEqual("000-000", min);
            Assert.AreEqual("ZZZ-ZZZ", max);
        }

        [TestMethod]
        public void AlphTest()
        {
            for (var i = 0; i < MadId._alph.Length; ++i)
            {
                var fromInt = new MadId((uint)i);
                var fromChar = new MadId("000-00" + MadId._alph[i]);

                Assert.AreEqual((string)fromInt, fromChar);
                Assert.AreEqual((uint)fromInt, fromChar);
            }
        }

        [TestMethod]
        public void RandomConvTest()
        {
            var rnd = new Random();
            for (var i = 0; i < 100000; ++i)
            {
                var num = (uint) (rnd.NextDouble() * MadId.MaxValue);
                var madId = new MadId(num);

                Assert.AreEqual(num, madId);
            }
        }
    }
}

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace Tests
{
    [TestClass]
    public class MathTests
    {
        private static float ExpectedLerp(float value1, float value2, float weight)
        {
            return value1 + (value2 - value1) * weight;
        }

        [TestMethod]
        public void LerpTest()
        {
            float expected, actual;
            actual = ExpectedLerp(100, 10, .5f);
            expected = MathHelper.Lerp(100, 10, .5f);
            Assert.AreEqual(expected, actual);
        }
    }
}

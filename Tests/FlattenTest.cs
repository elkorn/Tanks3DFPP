using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class FlattenTest
    {
        [TestMethod]
        public void OutputsAnArrayOfCorrectLength()
        {
            int[,] input = new int[11, 17];
            int[] output = Tanks3DFPP.Utilities.Utilities.Flatten(input);
            Assert.AreEqual<int>(input.GetLength(0) * input.GetLength(1), output.GetLength(0));
        }
    }
}

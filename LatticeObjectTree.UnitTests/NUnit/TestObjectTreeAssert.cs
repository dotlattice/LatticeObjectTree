using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LatticeObjectTree.NUnit.Comparers;

namespace LatticeObjectTree.UnitTests.NUnit
{
    public class TestObjectTreeAssert
    {
        [Test]
        public void AreEqual_SameObjectTwice()
        {
            var obj = new object();
            ObjectTreeAssert.AreEqual(obj, obj);
        }

        [Test]
        public void AreEqualFail_DifferentStrings()
        {
            var expected = "hello";
            var actual = "world";
            var expectedException = Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreEqual(expected, actual));

            Console.WriteLine(expectedException.Message);

            var expectedMessage = "  Expected: <LatticeObjectTree.ObjectTree> with root \"hello\"" + Environment.NewLine
                + "  But was:  <LatticeObjectTree.ObjectTree> with root \"world\"" + Environment.NewLine
                + "  1 Differences:    < <<root>: expected value \"hello\" but was \"world\".> >";
            Assert.AreEqual(expectedMessage, expectedException.Message);
        }

        [Test]
        public void AreEqualFail_DifferentStrings_CustomMessage()
        {
            var expected = "hello";
            var actual = "world";
            var expectedException = Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreEqual(expected, actual, message: "This is a test message"));

            Console.WriteLine(expectedException.Message);

            var expectedMessage = "  This is a test message" + Environment.NewLine
                + "  Expected: <LatticeObjectTree.ObjectTree> with root \"hello\"" + Environment.NewLine
                + "  But was:  <LatticeObjectTree.ObjectTree> with root \"world\"" + Environment.NewLine
                + "  1 Differences:    < <<root>: expected value \"hello\" but was \"world\".> >";
            Assert.AreEqual(expectedMessage, expectedException.Message);
        }

        [Test]
        public void AreEqualFail_VeryDifferentLists()
        {
            var expected = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            var actual = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
            var expectedException = Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreEqual(expected, actual));

            Console.WriteLine(expectedException.Message);

            var expectedMessage = "  Expected: <LatticeObjectTree.ObjectTree> with root < 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 >" + Environment.NewLine
                + "  But was:  <LatticeObjectTree.ObjectTree> with root < 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 >" + Environment.NewLine
                + "  26 Differences:    < <<root>[0]: expected value \"0\" but was \"1\".>, <<root>[1]: expected value \"1\" but was \"2\".>, <<root>[2]: expected value \"2\" but was \"3\".>, <<root>[3]: expected value \"3\" but was \"4\".>, <<root>[4]: expected value \"4\" but was \"5\".>, <<root>[5]: expected value \"5\" but was \"6\".>, <<root>[6]: expected value \"6\" but was \"7\".>, <<root>[7]: expected value \"7\" but was \"8\".>, <<root>[8]: expected value \"8\" but was \"9\".>, <<root>[9]: expected value \"9\" but was \"10\".>, <<root>[10]: expected value \"10\" but was \"11\".>, <<root>[11]: expected value \"11\" but was \"12\".>, <<root>[12]: expected value \"12\" but was \"13\".>, <<root>[13]: expected value \"13\" but was \"14\".>, <<root>[14]: expected value \"14\" but was \"15\".>, <<root>[15]: expected value \"15\" but was \"16\".>, <<root>[16]: expected value \"16\" but was \"17\".>, <<root>[17]: expected value \"17\" but was \"18\".>, <<root>[18]: expected value \"18\" but was \"19\".>, <<root>[19]: expected value \"19\" but was \"20\".>... >";
            Assert.AreEqual(expectedMessage, expectedException.Message);
        }
    }
}

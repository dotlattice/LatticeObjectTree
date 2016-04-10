using LatticeObjectTree.Comparers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.Comparers
{
    public class ObjectTreeValueEqualityComparerTest
    {
        private IEqualityComparer<object> objectComparer;

        [SetUp]
        public void SetUp()
        {
            objectComparer = ObjectTreeValueEqualityComparer.Instance;
        }

        [TestCase(2, 2, true)]
        [TestCase(2, 1, false)]
        [TestCase(2, 2f, false)]
        [TestCase("Hello world!", "Hello world!", true)]
        [TestCase("hello", "world", false)]
        [TestCase(2, null, false)]
        [TestCase("test", null, false)]
        [TestCase(null, null, true)]
        public void AreValuesEqual_SimpleValues(object a, object b, bool expected)
        {
            var actual = objectComparer.Equals(a, b);
            Assert.AreEqual(expected, actual);

            var actualReversed = objectComparer.Equals(b, a);
            Assert.AreEqual(expected, actualReversed);
        }

        [Test]
        public void AreValuesEqual_ByteArray_Self()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var actual = objectComparer.Equals(a, a);
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void AreValuesEqual_ByteArray_DifferentEqual()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var b = new byte[] { 1, 171, 128, 3 };
            var actual = objectComparer.Equals(a, b);
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void AreValuesEqual_ByteArray_DifferentNotEqual()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var b = new byte[] { 1, 171, 128, 2 };
            var actual = objectComparer.Equals(a, b);
            Assert.AreEqual(false, actual);
        }
    }
}

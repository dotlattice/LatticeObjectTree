using LatticeObjectTree.Comparison;
using NUnit.Framework;
using TestAttribute = Xunit.FactAttribute;
using TestCaseAttribute = Xunit.InlineDataAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.Comparison
{
    public class ObjectTreeValueEqualityComparerTest
    {
        [Xunit.Theory]
        [TestCase(2, 2, true)]
        [TestCase(2, 1, false)]
        [TestCase(2, 2f, false)]
        [TestCase(0f, float.Epsilon, false)]
        [TestCase(0d, double.Epsilon, false)]
        [TestCase("Hello world!", "Hello world!", true)]
        [TestCase("hello", "world", false)]
        [TestCase(2, null, false)]
        [TestCase("test", null, false)]
        [TestCase(null, null, true)]
        public void AreValuesEqual_SimpleValues(object a, object b, bool expected)
        {
            var actual = ObjectTreeValueEqualityComparer.Instance.Equals(a, b);
            Assert.AreEqual(expected, actual);

            var actualReversed = ObjectTreeValueEqualityComparer.Instance.Equals(b, a);
            Assert.AreEqual(expected, actualReversed);

            if (expected)
            {
                Assert.AreEqual(ObjectTreeValueEqualityComparer.Instance.GetHashCode(a), ObjectTreeValueEqualityComparer.Instance.GetHashCode(b));
            }
        }

        [Test]
        public void AreValuesEqual_ByteArray_Self()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var actual = ObjectTreeValueEqualityComparer.Instance.Equals(a, a);
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void AreValuesEqual_ByteArray_DifferentEqual()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var b = new byte[] { 1, 171, 128, 3 };
            var actual = ObjectTreeValueEqualityComparer.Instance.Equals(a, b);
            Assert.AreEqual(true, actual);
            Assert.AreEqual(ObjectTreeValueEqualityComparer.Instance.GetHashCode(a), ObjectTreeValueEqualityComparer.Instance.GetHashCode(b));
        }

        [Test]
        public void AreValuesEqual_ByteArray_DifferentNotEqual()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var b = new byte[] { 1, 171, 128, 2 };
            var actual = ObjectTreeValueEqualityComparer.Instance.Equals(a, b);
            Assert.AreEqual(false, actual);
        }

        [Test]
        public void AreValuesEqual_FloatsAlmostEqual()
        {
            var a = 0f;
            var b = float.Epsilon / 2;

            var actual = ObjectTreeValueEqualityComparer.Instance.Equals(a, b);
            Assert.AreEqual(true, actual);
            Assert.AreEqual(ObjectTreeValueEqualityComparer.Instance.GetHashCode(a), ObjectTreeValueEqualityComparer.Instance.GetHashCode(b));
        }

        [Test]
        public void AreValuesEqual_DoublesAlmostEqual()
        {
            var a = 0d;
            var b = double.Epsilon / 2;

            var actual = ObjectTreeValueEqualityComparer.Instance.Equals(a, b);
            Assert.AreEqual(true, actual);
            Assert.AreEqual(ObjectTreeValueEqualityComparer.Instance.GetHashCode(a), ObjectTreeValueEqualityComparer.Instance.GetHashCode(b));
        }
    }
}

using LatticeObjectTree.Comparison;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.Comparison
{
    public class ObjectTreeValueFormatterTest
    {
        private ObjectTreeValueFormatter objectFormatter;

        [SetUp]
        public void SetUp()
        {
            objectFormatter = ObjectTreeValueFormatter.Instance;
        }

        [TestCase(2, "2")]
        [TestCase(2f, "2f")]
        [TestCase(2.0f, "2f")]
        [TestCase(2.123f, "2.123f")]
        [TestCase(2d, "2d")]
        [TestCase(2.0d, "2d")]
        [TestCase(2.123d, "2.123d")]
        [TestCase("", "\"\"")]
        [TestCase("2", "\"2\"")]
        [TestCase("Hello world!", "\"Hello world!\"")]
        [TestCase("\"hi\"", "@\"\"\"hi\"\"\"")]
        [TestCase(@"domain\user", @"""domain\\user""")]
        [TestCase(null, "null")]
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void FormatValue_SimpleValue(object value, string expected)
        {
            var actual = objectFormatter.Format(value);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FormatValue_Decimal()
        {
            decimal value = 2.123m;
            var actual = objectFormatter.Format(value);
            var expected = "2.123m";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FormatValue_Enum()
        {
            var value = StringComparison.OrdinalIgnoreCase;
            var actual = objectFormatter.Format(value);
            var expected = "StringComparison.OrdinalIgnoreCase";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FormatValue_ByteArray()
        {
            var value = new byte[] { 1, 171, 128, 3 };
            var actual = objectFormatter.Format(value);
            var expected = "0x01AB8003";
            Assert.AreEqual(expected, actual);
        }
    }
}

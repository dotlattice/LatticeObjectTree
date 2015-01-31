using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.UnitTests.Core
{
    public class TestObjectTreeEdge
    {
        [Test]
        public void Constructor_NegativeIndex()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new DefaultObjectTreeEdge(-1));
        }

        #region Equality

        [Test]
        public void Equals_EmptyNode()
        {
            AssertEquality(new DefaultObjectTreeEdge(), new DefaultObjectTreeEdge(), expected: true);
        }

        [Test]
        public void Equals_EmptyNodeVsNull()
        {
            AssertEquality(new DefaultObjectTreeEdge(), null, expected: false);
        }

        [Test]
        public void Equals_SameProperty()
        {
            var property = typeof(string).GetProperty("Length");

            var a = new DefaultObjectTreeEdge(property);
            var b = new DefaultObjectTreeEdge(property);
            AssertEquality(a, b, expected: true);
        }

        [Test]
        public void Equals_DifferentProperty_SameName()
        {
            var propertyA = typeof(string).GetProperty("Length");
            var propertyB = typeof(int[]).GetProperty("Length");
            var a = new DefaultObjectTreeEdge(propertyA);
            var b = new DefaultObjectTreeEdge(propertyB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_SamePropertyWithDifferentGenericType()
        {
            var propertyA = typeof(ICollection<int>).GetProperty("Count");
            var propertyB = typeof(ICollection<short>).GetProperty("Count");
            var a = new DefaultObjectTreeEdge(propertyA);
            var b = new DefaultObjectTreeEdge(propertyB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_SameIndex()
        {
            var a = new DefaultObjectTreeEdge(2);
            var b = new DefaultObjectTreeEdge(2);
            AssertEquality(a, b, expected: true);
        }

        [Test]
        public void Equals_DifferentIndex()
        {
            var a = new DefaultObjectTreeEdge(2);
            var b = new DefaultObjectTreeEdge(0);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_DifferentPropertySameCaseInsensitiveName()
        {
            var propertyA = typeof(TestClass).GetProperty("Hi");
            var propertyB = typeof(TestClass).GetProperty("HI");
            Assert.AreNotEqual(propertyA, propertyB);

            var a = new DefaultObjectTreeEdge(propertyA);
            var b = new DefaultObjectTreeEdge(propertyB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_SameField()
        {
            var field = typeof(TestClass).GetField("world");
            var a = new DefaultObjectTreeEdge(field);
            var b = new DefaultObjectTreeEdge(field);
            AssertEquality(a, b, expected: true);
        }

        [Test]
        public void Equals_DifferentField()
        {
            var fieldA = typeof(TestClass).GetField("hello");
            var fieldB = typeof(TestClass).GetField("world");

            var a = new DefaultObjectTreeEdge(fieldA);
            var b = new DefaultObjectTreeEdge(fieldB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_PropertyVsField()
        {
            var property = typeof(TestClass).GetProperty("Hi");
            var field = typeof(TestClass).GetField("hi");

            var a = new DefaultObjectTreeEdge(property);
            var b = new DefaultObjectTreeEdge(field);
            AssertEquality(a, b, expected: false);
        }

        private class TestClass
        {
            public string Hi { get; set; }
            public string HI { get; set; }

            public string hi = null;
            public int hello = 0;
            public int Hello = 0;
            public DateTime world = default(DateTime);
        }

        private static void AssertEquality(DefaultObjectTreeEdge a, DefaultObjectTreeEdge b, bool expected)
        {
            Assert.AreNotSame(a, b);
            Assert.AreEqual(expected, Equals(a, b));
            Assert.AreEqual(expected, Equals(b, a));
            if (a != null)
            {
                Assert.AreEqual(expected, a.Equals(b));
            }
            if (b != null)
            {
                Assert.AreEqual(expected, b.Equals(a));
            }
            Assert.AreEqual(expected, a == b);
            Assert.AreEqual(!expected, a != b);
            if (expected && a != null && b != null)
            {
                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            }
        }

        #endregion

        #region ToString

        [Test]
        public void ToString_RootNode()
        {
            var rootNode = new DefaultObjectTreeEdge();
            Assert.AreEqual("", rootNode.ToString());
        }

        [Test]
        public void ToString_PropertyNode()
        {
            var property = typeof(string).GetProperty("Length");
            var node = new DefaultObjectTreeEdge(property);
            Assert.AreEqual(".Length", node.ToString());
        }

        [Test]
        public void ToString_IndexNode()
        {
            var node = new DefaultObjectTreeEdge(index: 2);
            Assert.AreEqual("[2]", node.ToString());
        }

        #endregion
    }
}

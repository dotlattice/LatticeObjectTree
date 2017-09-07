using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree
{
    public class ObjectTreeEdgeTest
    {
        [Test]
        public void Constructor_NegativeIndex()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectTreeEdge(-1));
        }

        #region TryResolve


        [Test]
        public void TryResolve_NoMemberOrIndex()
        {
            var edge = new ObjectTreeEdge();
            var rootObject = new Object();

            object result;
            bool isResolved = edge.TryResolve(rootObject, out result);

            Assert.IsTrue(isResolved);
            Assert.AreSame(rootObject, result);
        }

        [Test]
        public void TryResolve_NoMemberOrIndex_NullRoot()
        {
            var edge = new ObjectTreeEdge();

            object result;
            bool isResolved = edge.TryResolve(null, out result);

            Assert.IsTrue(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_StringLengthProperty()
        {
            var edge = new ObjectTreeEdge(typeof(string).GetProperty("Length"));
            var str = "test";

            object result;
            bool isResolved = edge.TryResolve(str, out result);

            Assert.IsTrue(isResolved);
            Assert.AreEqual(str.Length, result);
        }

        [Test]
        public void TryResolve_StringLengthProperty_NullString()
        {
            var edge = new ObjectTreeEdge(typeof(string).GetProperty("Length"));

            object result;
            bool isResolved = edge.TryResolve(null, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_ListIndex()
        {
            var edge = new ObjectTreeEdge(index: 0); ;
            var list = new[] { "hello", "world" };

            object result;
            bool isResolved = edge.TryResolve(list, out result);

            Assert.IsTrue(isResolved);
            Assert.AreEqual(list[0], result);
        }

        [Test]
        public void TryResolve_ListIndex_OutOfRange()
        {
            var edge = new ObjectTreeEdge(index: 2); ;
            var list = new[] { "hello", "world" };

            object result;
            bool isResolved = edge.TryResolve(list, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_ListIndex_NullList()
        {
            var edge = new ObjectTreeEdge(index: 0);

            object result;
            bool isResolved = edge.TryResolve(null, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_IndexOnNonEnumerable()
        {
            var edge = new ObjectTreeEdge(index: 0);
            var rootObject = new Object();

            object result;
            bool isResolved = edge.TryResolve(rootObject, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_StringLengthProperty_NonString()
        {
            var edge = new ObjectTreeEdge(typeof(string).GetProperty("Length"));

            object result;
            bool isResolved = edge.TryResolve(new Object(), out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        #endregion

        #region Equality

        [Test]
        public void Equals_EmptyNode()
        {
            AssertEquality(new ObjectTreeEdge(), new ObjectTreeEdge(), expected: true);
        }

        [Test]
        public void Equals_EmptyNodeVsNull()
        {
            AssertEquality(new ObjectTreeEdge(), null, expected: false);
        }

        [Test]
        public void Equals_SameProperty()
        {
            var property = typeof(string).GetProperty("Length");

            var a = new ObjectTreeEdge(property);
            var b = new ObjectTreeEdge(property);
            AssertEquality(a, b, expected: true);
        }

        [Test]
        public void Equals_DifferentProperty_SameName()
        {
            var propertyA = typeof(string).GetProperty("Length");
            var propertyB = typeof(int[]).GetProperty("Length");
            var a = new ObjectTreeEdge(propertyA);
            var b = new ObjectTreeEdge(propertyB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_SamePropertyWithDifferentGenericType()
        {
            var propertyA = typeof(ICollection<int>).GetProperty("Count");
            var propertyB = typeof(ICollection<short>).GetProperty("Count");
            var a = new ObjectTreeEdge(propertyA);
            var b = new ObjectTreeEdge(propertyB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_SameIndex()
        {
            var a = new ObjectTreeEdge(2);
            var b = new ObjectTreeEdge(2);
            AssertEquality(a, b, expected: true);
        }

        [Test]
        public void Equals_DifferentIndex()
        {
            var a = new ObjectTreeEdge(2);
            var b = new ObjectTreeEdge(0);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_DifferentPropertySameCaseInsensitiveName()
        {
            var propertyA = typeof(TestClass).GetProperty("Hi");
            var propertyB = typeof(TestClass).GetProperty("HI");
            Assert.AreNotEqual(propertyA, propertyB);

            var a = new ObjectTreeEdge(propertyA);
            var b = new ObjectTreeEdge(propertyB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_SameField()
        {
            var field = typeof(TestClass).GetField("world");
            var a = new ObjectTreeEdge(field);
            var b = new ObjectTreeEdge(field);
            AssertEquality(a, b, expected: true);
        }

        [Test]
        public void Equals_DifferentField()
        {
            var fieldA = typeof(TestClass).GetField("hello");
            var fieldB = typeof(TestClass).GetField("world");

            var a = new ObjectTreeEdge(fieldA);
            var b = new ObjectTreeEdge(fieldB);
            AssertEquality(a, b, expected: false);
        }

        [Test]
        public void Equals_PropertyVsField()
        {
            var property = typeof(TestClass).GetProperty("Hi");
            var field = typeof(TestClass).GetField("hi");

            var a = new ObjectTreeEdge(property);
            var b = new ObjectTreeEdge(field);
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

        private static void AssertEquality(ObjectTreeEdge a, ObjectTreeEdge b, bool expected)
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
            var rootNode = new ObjectTreeEdge();
            Assert.AreEqual("", rootNode.ToString());
        }

        [Test]
        public void ToString_PropertyNode()
        {
            var property = typeof(string).GetProperty("Length");
            var node = new ObjectTreeEdge(property);
            Assert.AreEqual(".Length", node.ToString());
        }

        [Test]
        public void ToString_IndexNode()
        {
            var node = new ObjectTreeEdge(index: 2);
            Assert.AreEqual("[2]", node.ToString());
        }

        #endregion
    }
}

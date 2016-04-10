using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree
{
    public class ObjectTreeEdgePathTest
    {
        [Test]
        public void Constructor_Null()
        {
            var expectedException = Assert.Throws<ArgumentNullException>(() => new ObjectTreeEdgePath(null));
            StringAssert.Contains("edges", expectedException.Message);
        }

        [Test]
        public void Constructor_EmptyNodeList()
        {
            var path = new ObjectTreeEdgePath(new ObjectTreeEdge[0]);
            Assert.AreEqual(0, path.Edges.Count);
        }

        [Test]
        public void Constructor_RootNodeOnly()
        {
            var node = new ObjectTreeEdge();
            var path = new ObjectTreeEdgePath(new[] { node });

            Assert.AreEqual(1, path.Edges.Count);
            Assert.AreSame(node, path.Edges.Single());
        }

        [Test]
        public void Nodes_CannotAdd()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge() });
            Assert.Throws<NotSupportedException>(() => path.Edges.Add(new ObjectTreeEdge()));
        }

        [Test]
        public void Nodes_CannotRemove()
        {
            var node = new ObjectTreeEdge();
            var path = new ObjectTreeEdgePath(new[] { node });
            Assert.Throws<NotSupportedException>(() => path.Edges.Remove(node));
        }

        #region ToString

        [Test]
        public void ToString_Empty()
        {
            var path = new ObjectTreeEdgePath(new ObjectTreeEdge[0]);
            Assert.AreEqual("<root>", path.ToString());
        }

        [Test]
        public void ToString_RootNodeOnly()
        {
            var path = new ObjectTreeEdgePath(new [] { new ObjectTreeEdge() });
            Assert.AreEqual("<root>", path.ToString());
        }

        [Test]
        public void ToString_PropertyNodeOnly()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(typeof(string).GetProperty("Length")) });
            Assert.AreEqual("<root>.Length", path.ToString());
        }

        [Test]
        public void ToString_RootToPropertyNode()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(typeof(string).GetProperty("Length")) });
            Assert.AreEqual("<root>.Length", path.ToString());
        }

        [Test]
        public void ToString_IndexNodeOnly()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(1) });
            Assert.AreEqual("<root>[1]", path.ToString());
        }

        [Test]
        public void ToString_RootToIndexNode()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(1) });
            Assert.AreEqual("<root>[1]", path.ToString());
        }

        [Test]
        public void ToString_RootToPropertyNodeToIndexNode()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(typeof(string).GetProperty("Length")), new ObjectTreeEdge(2) });
            Assert.AreEqual("<root>.Length[2]", path.ToString());
        }

        #endregion

        #region TryResolve

        [Test]
        public void TryResolve_Empty()
        {
            var path = new ObjectTreeEdgePath(new ObjectTreeEdge[0]);
            var rootObject = new Object();

            object result;
            bool isResolved = path.TryResolve(rootObject, out result);

            Assert.IsTrue(isResolved);
            Assert.AreSame(rootObject, result);
        }

        [Test]
        public void TryResolve_RootOnly()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge() });
            var rootObject = new Object();

            object result;
            bool isResolved = path.TryResolve(rootObject, out result);

            Assert.IsTrue(isResolved);
            Assert.AreSame(rootObject, result);
        }

        [Test]
        public void TryResolve_RootOnly_Null()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge() });

            object result;
            bool isResolved = path.TryResolve(null, out result);

            Assert.IsTrue(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_StringLengthProperty()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(typeof(string).GetProperty("Length")) });
            var str = "test";

            object result;
            bool isResolved = path.TryResolve(str, out result);

            Assert.IsTrue(isResolved);
            Assert.AreEqual(str.Length, result);
        }

        [Test]
        public void TryResolve_RootToStringLengthProperty()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(typeof(string).GetProperty("Length")) });
            var str = "test";

            object result;
            bool isResolved = path.TryResolve(str, out result);

            Assert.IsTrue(isResolved);
            Assert.AreEqual(str.Length, result);
        }

        [Test]
        public void TryResolve_RootToStringLengthProperty_NullString()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(typeof(string).GetProperty("Length")) });

            object result;
            bool isResolved = path.TryResolve(null, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_ListIndex()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(index: 0) });
            var list = new[] { "hello", "world" };

            object result;
            bool isResolved = path.TryResolve(list, out result);

            Assert.IsTrue(isResolved);
            Assert.AreEqual(list[0], result);
        }


        [Test]
        public void TryResolve_ListIndex_OutOfRange()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(index: 2) });
            var list = new[] { "hello", "world" };

            object result;
            bool isResolved = path.TryResolve(list, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_RootToListIndex()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(index: 1) });
            var list = new[] { "hello", "world" };

            object result;
            bool isResolved = path.TryResolve(list, out result);

            Assert.IsTrue(isResolved);
            Assert.AreEqual(list[1], result);
        }

        [Test]
        public void TryResolve_RootToListIndex_NullList()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(index: 1) });

            object result;
            bool isResolved = path.TryResolve(null, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_RootToListIndexToStringLengthProperty()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(index: 1), new ObjectTreeEdge(typeof(string).GetProperty("Length")) });
            var list = new[] { "hello", "world" };

            object result;
            bool isResolved = path.TryResolve(list, out result);

            Assert.IsTrue(isResolved);
            Assert.AreEqual(list[1].Length, result);
        }

        [Test]
        public void TryResolve_RootToListIndexToStringLengthProperty_NullList()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(index: 1), new ObjectTreeEdge(typeof(string).GetProperty("Length")) });

            object result;
            bool isResolved = path.TryResolve(null, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        [Test]
        public void TryResolve_RootToListIndexToStringLengthProperty_NullString()
        {
            var path = new ObjectTreeEdgePath(new[] { new ObjectTreeEdge(), new ObjectTreeEdge(index: 1), new ObjectTreeEdge(typeof(string).GetProperty("Length")) });
            var list = new[] { "hello", null };

            object result;
            bool isResolved = path.TryResolve(list, out result);

            Assert.IsFalse(isResolved);
            Assert.IsNull(result);
        }

        #endregion
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.UnitTests.Core
{
    public class TestObjectTreeNode
    {
        [Test]
        public void ToEdgePath_RootNode()
        {
            var node = new ObjectTreeNode(null);
            Assert.AreEqual(0, node.ToEdgePath().Edges.Count);
        }

        [Test]
        public void ToEdgePath_OneParentWithIndexEdge()
        {
            var node = new ObjectTreeNode(null, new ObjectTreeNode(null), new DefaultObjectTreeEdge(0));
            Assert.AreEqual(1, node.ToEdgePath().Edges.Count);
            Assert.AreEqual(0, node.ToEdgePath().Edges.Single().Index);
        }

        [Test]
        public void ToEdgePath_TwoParentWithIndexEdges()
        {
            var node = new ObjectTreeNode(null, 
                new ObjectTreeNode(null, 
                    new ObjectTreeNode(null), 
                    new DefaultObjectTreeEdge(0)
                ), 
                new DefaultObjectTreeEdge(1)
            );
            Assert.AreEqual(2, node.ToEdgePath().Edges.Count);
            Assert.AreEqual(0, node.ToEdgePath().Edges.ElementAt(0).Index);
            Assert.AreEqual(1, node.ToEdgePath().Edges.ElementAt(1).Index);
        }
    }
}

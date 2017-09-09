using NUnit.Framework;
using TestAttribute = Xunit.FactAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree
{
    public class ObjectTreeNodeTest
    {
        [Test]
        public void ToEdgePath_RootNode()
        {
            var node = new ObjectTreeNode(null, ObjectTreeNodeType.Object);
            Assert.AreEqual(0, node.ToEdgePath().Edges.Count);
        }

        [Test]
        public void ToEdgePath_OneParentWithIndexEdge()
        {
            var node = new ObjectTreeNode(null, ObjectTreeNodeType.Object, new ObjectTreeNode(null, ObjectTreeNodeType.Object), new ObjectTreeEdge(0));
            Assert.AreEqual(1, node.ToEdgePath().Edges.Count);
            Assert.AreEqual(0, node.ToEdgePath().Edges.Single().Index);
        }

        [Test]
        public void ToEdgePath_TwoParentWithIndexEdges()
        {
            var node = new ObjectTreeNode(null,
                ObjectTreeNodeType.Object,
                new ObjectTreeNode(null,
                    ObjectTreeNodeType.Object,
                    new ObjectTreeNode(null, ObjectTreeNodeType.Object), 
                    new ObjectTreeEdge(0)
                ), 
                new ObjectTreeEdge(1)
            );
            Assert.AreEqual(2, node.ToEdgePath().Edges.Count);
            Assert.AreEqual(0, node.ToEdgePath().Edges.ElementAt(0).Index);
            Assert.AreEqual(1, node.ToEdgePath().Edges.ElementAt(1).Index);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree
{
    /// <summary>
    /// A node in an object tree.  
    /// </summary>
    public class ObjectTreeNode
    {
        /// <summary>
        /// Constructs a root node with the specified value.
        /// </summary>
        /// <param name="value">the object that this node represents</param>
        /// <param name="nodeType">the type of this node</param>
        /// <param name="spawnStrategy">the strategy to use for generating descendant nodes, or null to use a default <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/> strategy</param>
        public ObjectTreeNode(object value, ObjectTreeNodeType nodeType, IObjectTreeSpawnStrategy spawnStrategy = null)
            : this(value, nodeType: nodeType, parentNode: null, edgeFromParent: null, spawnStrategy: spawnStrategy) { }

        /// <summary>
        /// Constructs a node with the specified value.
        /// </summary>
        /// <param name="value">the object that this node represents</param>
        /// <param name="nodeType">the type of this node</param>
        /// <param name="parentNode">the parent of this node, or null for a root node</param>
        /// <param name="edgeFromParent">the edge that leads from the parent to this node, or null for a root node</param>
        /// <param name="spawnStrategy">the strategy to use for generating descendant nodes, or null to use a default <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/> strategy</param>
        /// <exception cref="ArgumentException">if one of <c>parentNode</c> and <c>edgeFromParent</c> is null and the other is not null</exception>
        public ObjectTreeNode(object value, ObjectTreeNodeType nodeType, ObjectTreeNode parentNode, ObjectTreeEdge edgeFromParent, IObjectTreeSpawnStrategy spawnStrategy = null)
        {
            if ((parentNode == null) != (edgeFromParent == null))
            {
                throw new ArgumentException("The parent and edge must both be null or neither can be null (you can't have one without the other)");
            }

            Value = value;
            NodeType = nodeType;
            ParentNode = parentNode;
            EdgeFromParent = edgeFromParent;
            SpawnStrategy = spawnStrategy ?? new DuplicateCheckingObjectTreeSpawnStrategy();
        }

        /// <summary>
        /// The object represented by this node.  This can be null.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// The type of this node.
        /// </summary>
        public ObjectTreeNodeType NodeType { get; }

        /// <summary>
        /// The parent of this node, or null if this is a root node.
        /// </summary>
        public ObjectTreeNode ParentNode { get; }

        /// <summary>
        /// The edge that connects the parent node to this node, or null if this is a root node.
        /// </summary>
        public ObjectTreeEdge EdgeFromParent { get; }

        /// <summary>
        /// Creates and returns a path of the edges used to access this node from the root node.
        /// </summary>
        /// <returns>the edge path for this node</returns>
        /// <exception cref="InvalidOperationException">if a cycle is detected that prevents the creation of an edge path</exception>
        public ObjectTreeEdgePath ToEdgePath()
        {
            return new ObjectTreeEdgePath(GetEdgesFromCurrentToRoot().Reverse());
        }

        private IEnumerable<ObjectTreeEdge> GetEdgesFromCurrentToRoot()
        {
            // This set is to make sure we don't visit a node more than once while heading up the tree
            // (because that would mean we'd have an infinite loop)
            var cycleDetectionSet = new HashSet<ObjectTreeNode>();

            var currentNode = this;
            while (currentNode != null)
            {
                if (!cycleDetectionSet.Add(currentNode))
                {
                    throw new InvalidOperationException("There is a parent node cycle");
                }
                yield return currentNode.EdgeFromParent;
                currentNode = currentNode.ParentNode;
            }
        }

        /// <summary>
        /// The children of this node (if any).  This will never be null, but it may be empty.
        /// </summary>
        public IEnumerable<ObjectTreeNode> ChildNodes => SpawnStrategy.CreateChildNodes(this);

        /// <summary>
        /// The strategy that this node will use to spawn child nodes.  This will never be null.
        /// </summary>
        public IObjectTreeSpawnStrategy SpawnStrategy { get; }

        /// <summary>
        /// For a value that has already been represented in the tree, returns the original node that had this value.
        /// If duplicate detection is not being used then this will always be null.
        /// </summary>
        /// <remarks>
        /// This implementation always returns null, but it can be overridden in a subclass (like <see cref="DuplicateObjectTreeNode"/>) to return a value.
        /// </remarks>
        public virtual ObjectTreeNode OriginalNode => null;
    }

    /// <summary>
    /// A node whose value already has a previous node representing it in the object tree.
    /// </summary>
    public class DuplicateObjectTreeNode : ObjectTreeNode
    {
        /// <summary>
        /// Constructs a node that is already represented in the tree by the specified <c>originalNode</c>.
        /// </summary>
        /// <param name="originalNode">the original node that represents this value</param>
        /// <param name="parentNode">the parent of this version of the node</param>
        /// <param name="edgeFromParent">the edge from the parent node to this node</param>
        /// <exception cref="ArgumentNullException">if <c>originalNode</c>, <c>parentNode</c>, or <c>edgeFromParent</c> is null</exception>
        public DuplicateObjectTreeNode(ObjectTreeNode originalNode, ObjectTreeNode parentNode, ObjectTreeEdge edgeFromParent)
             : base(originalNode?.Value, originalNode?.NodeType ?? ObjectTreeNodeType.Unknown, parentNode, edgeFromParent, new EmptyObjectTreeSpawnStrategy())
        {
            if (originalNode == null) throw new ArgumentNullException(nameof(originalNode));
            if (parentNode == null) throw new ArgumentNullException(nameof(parentNode));
            if (edgeFromParent == null) throw new ArgumentNullException(nameof(edgeFromParent));
            OriginalNode = originalNode;
        }

        /// <summary>
        /// The original node that contains the same value as this node.  This will never be null.
        /// </summary>
        public override ObjectTreeNode OriginalNode { get; }
    }
}

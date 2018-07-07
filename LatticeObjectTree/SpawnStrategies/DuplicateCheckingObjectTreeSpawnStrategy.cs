using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree
{
    /// <summary>
    /// A spawn strategy that adds checking for duplicate objects to a backing spawn strategy.
    /// </summary>
    public class DuplicateCheckingObjectTreeSpawnStrategy : IObjectTreeSpawnStrategy
    {
        private readonly IDictionary<object, ObjectTreeNode> visitedValueToNodeDictionary;

        /// <summary>
        /// Constructs a default duplicate checking spawn strategy based on <see cref="BasicObjectTreeSpawnStrategy"/>.
        /// </summary>
        public DuplicateCheckingObjectTreeSpawnStrategy() 
            : this(new BasicObjectTreeSpawnStrategy()) { }

        /// <summary>
        /// Constructs a duplicate checking spawn strategy that uses the specified options.
        /// </summary>
        /// <param name="options">(optional) options that control how the tree is built</param>
        public DuplicateCheckingObjectTreeSpawnStrategy(IObjectTreeOptions options)
            : this(new BasicObjectTreeSpawnStrategy(options)) { }

        /// <summary>
        /// Constructs a spawn strategy that adds duplicate checking to the specified strategy.
        /// </summary>
        /// <param name="backingSpawnStrategy">the backing spawn strategy, or null to use a default <see cref="BasicObjectTreeSpawnStrategy"/></param>
        /// <exception cref="ArgumentException">if the backing spawn strategy is an instance of <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/></exception>
        public DuplicateCheckingObjectTreeSpawnStrategy(IObjectTreeSpawnStrategy backingSpawnStrategy)
        {
            if (backingSpawnStrategy is DuplicateCheckingObjectTreeSpawnStrategy)
            {
                throw new ArgumentException("Cannot add duplicate checking to a spawn strategy that already has it.");
            }
            BackingSpawnStrategy = backingSpawnStrategy ?? new BasicObjectTreeSpawnStrategy();
            visitedValueToNodeDictionary = new Dictionary<object, ObjectTreeNode>(ObjectIdentityEqualityComparer.Instance);
        }

        /// <summary>
        /// The backing strategy that this spawn strategy adds duplicate checking to.
        /// </summary>
        public IObjectTreeSpawnStrategy BackingSpawnStrategy { get; }

        /// <inheritdoc />
        public ObjectTreeNode CreateRootNode(object value, IObjectTreeSpawnStrategy spawnStrategyOverride = null)
        {
            return BackingSpawnStrategy.CreateRootNode(value, spawnStrategyOverride ?? this);
        }

        /// <inheritdoc />
        public IEnumerable<ObjectTreeNode> CreateChildNodes(ObjectTreeNode node, IObjectTreeSpawnStrategy spawnStrategyOverride)
        {
            var childSpawnStrategyOverride = spawnStrategyOverride ?? this;

            // Need to do this first to be able to get the root node.
            StoreNodeIfNecessary(node);

            var childNodes = BackingSpawnStrategy.CreateChildNodes(node, spawnStrategyOverride: childSpawnStrategyOverride);
            foreach (var childNode in childNodes)
            {
                ObjectTreeNode originalChildNode;
                if (childNode.NodeType == ObjectTreeNodeType.Primitive)
                {
                    yield return childNode;
                }
                else if (StoreNodeIfNecessary(childNode, out originalChildNode) || originalChildNode == null)
                {
                    yield return childNode;
                }
                else
                {
                    // The parent reference it not set yet on the childNode, so we can't use its ToEdgePath method.
                    //var childMemberPath = childNode.ToEdgePath();
                    var childPath = new ObjectTreeEdgePath(node.ToEdgePath().Edges.Concat(new[] { childNode.EdgeFromParent }));
                    var originalChildMemberPath = originalChildNode.ToEdgePath();
                    if (Equals(originalChildMemberPath, childPath))
                    {
                        yield return originalChildNode;
                    }
                    else
                    {
                        yield return new DuplicateObjectTreeNode(originalChildNode, childNode.ParentNode, childNode.EdgeFromParent);
                    }
                }
            }
        }

        /// <summary>
        /// Stores an entry in the dictionary for the specified node if necessary.
        /// </summary>
        /// <param name="node">the node to store</param>
        /// <returns>true if the node was stored or false if the node's value was already stored</returns>
        private bool StoreNodeIfNecessary(ObjectTreeNode node)
        {
            ObjectTreeNode originalNode;
            return StoreNodeIfNecessary(node, out originalNode);
        }

        /// <summary>
        /// Stores an entry in the dictionary for the specified node if necessary.
        /// </summary>
        /// <param name="node">the node to store</param>
        /// <param name="originalNode">will be set to the original node if there is a duplicate, or null if there is not</param>
        /// <returns>true if the node was stored or false if the node's value was already stored</returns>
        private bool StoreNodeIfNecessary(ObjectTreeNode node, out ObjectTreeNode originalNode)
        {
            if (node.NodeType == ObjectTreeNodeType.Primitive)
            {
                originalNode = null;
                return false;
            }

            var nodeValue = node.Value;
            if (nodeValue == null)
            {
                originalNode = null;
                return false;
            }

            //var childValueType = nodeValue.GetType();
            //if (childValueType == typeof(string))
            //{
            //    originalNode = null;
            //    return false;
            //}

            if (!visitedValueToNodeDictionary.TryGetValue(nodeValue, out originalNode))
            {
                visitedValueToNodeDictionary[nodeValue] = node;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two objects for equality by identity, even for classes that override the Equals/GetHashCode methods.
        /// </summary>
        private class ObjectIdentityEqualityComparer : IEqualityComparer<object>
        {
            public static ObjectIdentityEqualityComparer Instance { get; } = new ObjectIdentityEqualityComparer();

            private ObjectIdentityEqualityComparer() { }

            public new bool Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}

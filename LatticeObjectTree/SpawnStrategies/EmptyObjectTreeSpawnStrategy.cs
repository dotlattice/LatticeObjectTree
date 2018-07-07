using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree
{
    /// <summary>
    /// A spawn strategy that never spawns any child nodes.
    /// </summary>
    public class EmptyObjectTreeSpawnStrategy : IObjectTreeSpawnStrategy
    {
        /// <summary>
        /// Always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">always</exception>
        public ObjectTreeNode CreateRootNode(object value, IObjectTreeSpawnStrategy spawnStrategyOverride = null)
        {
            throw new NotSupportedException("Cannot create a root node from an empty spawn strategy");
        }

        /// <summary>
        /// Always returns an empty enumerable.
        /// </summary>
        /// <param name="node">not used</param>
        /// <param name="spawnStrategyOverride">not used</param>
        /// <returns>an empty enumerable</returns>
        public IEnumerable<ObjectTreeNode> CreateChildNodes(ObjectTreeNode node, IObjectTreeSpawnStrategy spawnStrategyOverride = null)
        {
            return Enumerable.Empty<ObjectTreeNode>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LatticeObjectTree
{
    /// <summary>
    /// The strategy to use for spawning child nodes from a specified node
    /// </summary>
    public interface IObjectTreeSpawnStrategy
    {
        /// <summary>
        /// Returns a root node with the specified value.
        /// </summary>
        /// <param name="value">the root node's value</param>
        /// <param name="spawnStrategyOverride">the spawn strategy to pass on to any created child nodes, or null to use a default strategy</param>
        /// <returns>the root node with the specified value</returns>
        ObjectTreeNode CreateRootNode(object value, IObjectTreeSpawnStrategy spawnStrategyOverride = null);

        /// <summary>
        /// Creates child nodes for the specified node.
        /// </summary>
        /// <param name="node">the node for which to spawn child nodes</param>
        /// <param name="spawnStrategyOverride">the spawn strategy to pass on to any created child nodes, or null to use a default strategy</param>
        /// <returns>the child nodes; this can never be null</returns>
        IEnumerable<ObjectTreeNode> CreateChildNodes(ObjectTreeNode node, IObjectTreeSpawnStrategy spawnStrategyOverride = null);
    }
}

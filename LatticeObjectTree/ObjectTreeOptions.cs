using LatticeObjectTree.Comparison;
using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeObjectTree
{
    /// <summary>
    /// Options that control how an object tree is built.
    /// </summary>
    public interface IObjectTreeOptions
    {
        /// <summary>
        /// A filter on the nodes that are included in the object tree.
        /// </summary>
        IObjectTreeNodeFilter NodeFilter { get; }

        /// <summary>
        /// True if dictionaries should be evaluated as collections (instead of as objects).
        /// </summary>
        bool? EvaluateDictionariesAsCollections { get; }

        /// <summary>
        /// The default spawn strategy to use for creating nodes, or null if no specific spawn strategy is requested.
        /// </summary>
        IObjectTreeSpawnStrategy DefaultSpawnStrategy { get; }
    }

    /// <summary>
    /// A default implementation of <see cref="IObjectTreeOptions"/>.
    /// </summary>
    public class ObjectTreeOptions : IObjectTreeOptions
    {
        /// <inheritdoc />
        public IObjectTreeNodeFilter NodeFilter { get; set; }

        /// <inheritdoc />
        public bool? EvaluateDictionariesAsCollections { get; set; }

        /// <inheritdoc />
        public IObjectTreeSpawnStrategy DefaultSpawnStrategy { get; set; }
    }
}

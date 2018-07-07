using System;
using System.Collections.Generic;

namespace LatticeObjectTree
{
    /// <summary>
    /// A spawn strategy that applies a filter to the output of another spawn strategy.
    /// </summary>
    [Obsolete("Use " + nameof(BasicObjectTreeSpawnStrategy) + " with a " + nameof(IObjectTreeOptions.NodeFilter))]
    public class FilteredObjectTreeSpawnStrategy : IObjectTreeSpawnStrategy
    {
        /// <summary>
        /// Constructs a spawn strategy using the specified filter on the output of a default <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/>.
        /// </summary>
        /// <param name="filter">the filter to apply</param>
        /// <exception cref="ArgumentNullException">if the filter is null</exception>
        public FilteredObjectTreeSpawnStrategy(IObjectTreeNodeFilter filter)
            : this(filter, new DuplicateCheckingObjectTreeSpawnStrategy()) { }

        /// <summary>
        /// Constructs a spawn strategy using the specified filter  on the output of a <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/> that uses the specified options.
        /// </summary>
        /// <param name="filter">the filter to apply</param>
        /// <param name="options">the options to apply</param>
        /// <exception cref="ArgumentNullException">if the filter is null</exception>
        public FilteredObjectTreeSpawnStrategy(IObjectTreeNodeFilter filter, IObjectTreeOptions options)
            : this(filter, new DuplicateCheckingObjectTreeSpawnStrategy(options)) { }

        /// <summary>
        /// Constructs a spawn strategy using the specified filter on the output of the specified spawn strategy.
        /// </summary>
        /// <param name="filter">the filter to apply</param>
        /// <param name="backingSpawnStrategy">the spawn strategy on which to apply the filter, or null to use a default <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/> spawn strategy</param>
        /// <exception cref="ArgumentNullException">if the filter is null</exception>
        public FilteredObjectTreeSpawnStrategy(IObjectTreeNodeFilter filter, IObjectTreeSpawnStrategy backingSpawnStrategy)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            Filter = filter;
            BackingSpawnStrategy = backingSpawnStrategy ?? new DuplicateCheckingObjectTreeSpawnStrategy();
        }

        /// <summary>
        /// The filter used by this spawn strategy (never null).
        /// </summary>
        public IObjectTreeNodeFilter Filter { get; }

        /// <summary>
        /// The backing strategy that this spawn strategy applies its filter to (never null).
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
            var childNodes = BackingSpawnStrategy.CreateChildNodes(node, spawnStrategyOverride: childSpawnStrategyOverride);
            var filteredChildNodes = Filter.Apply(childNodes);
            return filteredChildNodes;
        }
    }
}

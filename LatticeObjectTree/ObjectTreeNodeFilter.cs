using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LatticeObjectTree
{
    /// <summary>
    /// A filter for <see cref="ObjectTreeNode"/> values.
    /// </summary>
    public interface IObjectTreeNodeFilter
    {
        /// <summary>
        /// Applies the filter to the nodes.
        /// </summary>
        /// <param name="nodes">the nodes to filter; this list will not be modified by the filter</param>
        /// <returns>the nodes remaining after the filter is applied</returns>
        IEnumerable<ObjectTreeNode> Apply(IEnumerable<ObjectTreeNode> nodes);
    }

    /// <summary>
    /// A default filter that uses blacklists to exclude nodes.
    /// </summary>
    public class ObjectTreeNodeFilter : IObjectTreeNodeFilter
    {
        /// <summary>
        /// The case-insensitive names of properties to exclude.
        /// </summary>
        public ICollection<string> ExcludedPropertyNames { get; set; } = new string[0];

        /// <summary>
        /// Properties to exclude.
        /// </summary>
        public ICollection<PropertyInfo> ExcludedProperties { get; set; } = new PropertyInfo[0];

        /// <summary>
        /// Predicates that determine whether a property is excluded.
        /// </summary>
        public ICollection<Func<PropertyInfo, bool>> ExcludedPropertyPredicates { get; set; } = new Func<PropertyInfo, bool>[0];

        /// <summary>
        /// Predicates that determine whether a node is excluded.
        /// </summary>
        public ICollection<Func<ObjectTreeNode, bool>> ExcludedNodePredicates { get; set; } = new Func<ObjectTreeNode, bool>[0];

        /// <summary>
        /// Applies the filter to the nodes.
        /// </summary>
        /// <param name="nodes">the nodes to filter</param>
        /// <returns>the nodes remaining after the filter is applied</returns>
        public IEnumerable<ObjectTreeNode> Apply(IEnumerable<ObjectTreeNode> nodes)
        {
            var predicates = CreateAllExcludedNodePredicates().ToList();
            if (!predicates.Any())
            {
                return nodes;
            }
            return nodes.Where(node => !predicates.Any(predicate => predicate(node)));
        }

        private IEnumerable<Func<ObjectTreeNode, bool>> CreateAllExcludedNodePredicates()
        {
            if (ExcludedPropertyNames != null)
            {
                foreach (var propertyName in ExcludedPropertyNames)
                {
                    if (propertyName == null) continue;
                    yield return new Func<ObjectTreeNode, bool>(x => 
                        x.EdgeFromParent != null
                        && x.EdgeFromParent.Member is PropertyInfo
                        && string.Equals(x.EdgeFromParent.Member.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                    );
                }
            }

            if (ExcludedProperties != null)
            {
                foreach (var property in ExcludedProperties)
                {
                    if (property == null) continue;
                    yield return new Func<ObjectTreeNode, bool>(x =>
                        x.EdgeFromParent != null
                        && x.EdgeFromParent.Member is PropertyInfo
                        && string.Equals(x.EdgeFromParent.Member.Name, property.Name)
                    );
                }
            }

            if (ExcludedPropertyPredicates != null)
            {
                foreach (var predicate in ExcludedPropertyPredicates)
                {
                    if (predicate == null) continue;
                    yield return new Func<ObjectTreeNode, bool>(x =>
                        x.EdgeFromParent != null
                        && x.EdgeFromParent.Member is PropertyInfo
                        && predicate(x.EdgeFromParent.Member as PropertyInfo)
                    );
                }
            }

            if (ExcludedNodePredicates != null)
            {
                foreach (var predicate in ExcludedNodePredicates)
                {
                    if (predicate == null) continue;
                    yield return predicate;
                }
            }
        }
    }
}

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

    /// <summary>
    /// A spawn strategy that applies a filter to the output of another spawn strategy.
    /// </summary>
    public class FilteredObjectTreeSpawnStrategy : IObjectTreeSpawnStrategy
    {
        /// <summary>
        /// Constructs a spawn strategy using the specified filter on the output of the specified spawn strategy.
        /// </summary>
        /// <param name="filter">the filter to apply</param>
        /// <param name="backingSpawnStrategy">the spawn strategy on which to apply the filter, or null to use a default <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/> spawn strategy</param>
        /// <exception cref="ArgumentNullException">if the filter is null</exception>
        public FilteredObjectTreeSpawnStrategy(IObjectTreeNodeFilter filter, IObjectTreeSpawnStrategy backingSpawnStrategy = null)
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

    /// <summary>
    /// A spawn strategy that adds checking for duplicate objects to a backing spawn strategy.
    /// </summary>
    public class DuplicateCheckingObjectTreeSpawnStrategy : IObjectTreeSpawnStrategy
    {
        private readonly IDictionary<object, ObjectTreeNode> visitedValueToNodeDictionary;

        /// <summary>
        /// Constructs a spawn strategy that adds duplicate checking to the specified strategy.
        /// </summary>
        /// <param name="backingSpawnStrategy">the backing spawn strategy or null to use a default <see cref="BasicObjectTreeSpawnStrategy"/></param>
        /// <exception cref="ArgumentException">if the backing spawn strategy is an instance of <see cref="DuplicateCheckingObjectTreeSpawnStrategy"/></exception>
        public DuplicateCheckingObjectTreeSpawnStrategy(IObjectTreeSpawnStrategy backingSpawnStrategy = null)
        {
            if (backingSpawnStrategy is DuplicateCheckingObjectTreeSpawnStrategy) throw new ArgumentException("Cannot add duplicate checking to a spawn strategy that already has it.");

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

    /// <summary>
    /// A spawn strategy that generates child nodes from properties, fields, and enumerables.  
    /// This does not include any duplicate object or cycle checking.
    /// </summary>
    public class BasicObjectTreeSpawnStrategy : IObjectTreeSpawnStrategy
    {
        /// <inheritdoc />
        public ObjectTreeNode CreateRootNode(object value, IObjectTreeSpawnStrategy spawnStrategyOverride = null)
        {
            var nodeType = DetermineNodeType(value, parentNode: null, edgeFromParent: null);
            return new ObjectTreeNode(value, nodeType, spawnStrategyOverride ?? this);
        }

        /// <summary>
        /// Determines the node type of a node with the specified value and parent.
        /// </summary>
        /// <param name="value">the value of the node</param>
        /// <param name="parentNode">the parent of the node</param>
        /// <param name="edgeFromParent">the edge that would connect the parent node to this node</param>
        /// <returns>the type of the node</returns>
        protected virtual ObjectTreeNodeType DetermineNodeType(object value, ObjectTreeNode parentNode, ObjectTreeEdge edgeFromParent)
        {
            var valueType = value?.GetType() ?? edgeFromParent?.MemberType;
            return valueType != null ? DetermineNodeType(valueType) : ObjectTreeNodeType.Unknown;
        }

        private static ObjectTreeNodeType DetermineNodeType(Type valueType)
        {
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            ObjectTreeNodeType nodeType;
            if (IsPrimitiveType(valueType))
            {
                nodeType = ObjectTreeNodeType.Primitive;
            }
            else if (TypeUtils.IsAssignableFrom(typeof(System.Collections.IEnumerable), valueType))
            {
                nodeType = ObjectTreeNodeType.Collection;
            }
            else if (!IsIgnoredSystemType(valueType))
            {
                nodeType = ObjectTreeNodeType.Object;
            }
            else
            {
                nodeType = ObjectTreeNodeType.Unknown;
            }
            return nodeType;
        }

        private static bool IsPrimitiveType(Type valueType)
        {
            bool isPrimitive = TypeUtils.IsPrimitive(valueType)
                || valueType == typeof(string)
                || valueType == typeof(decimal)
                || valueType == typeof(Guid)
                || valueType == typeof(DateTime)
                || valueType == typeof(DateTimeOffset)
                || valueType == typeof(TimeSpan)
                || valueType == typeof(byte[])
                || TypeUtils.IsEnum(valueType);
            return isPrimitive;
        }

        private static bool IsIgnoredSystemType(Type valueType)
        {
            if (valueType.Name.Contains("AnonymousType") || valueType == typeof(string))
            {
                return false;
            }

            if (valueType.Namespace == null)
            {
                return true;
            }

            return (valueType.Namespace == "System" || valueType.Namespace == "System.Reflection" || valueType.Namespace == "System.Threading" || valueType.Namespace == "System.Threading.Tasks")
                && (!TypeUtils.IsValueType(valueType) || valueType.Name == "CancellationToken")
                && !valueType.IsArray;
        }

        /// <inheritdoc />
        public IEnumerable<ObjectTreeNode> CreateChildNodes(ObjectTreeNode node, IObjectTreeSpawnStrategy spawnStrategyOverride)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var childSpawnStrategyOverride = spawnStrategyOverride ?? this;

            if (node.OriginalNode != null)
            {
                return Enumerable.Empty<ObjectTreeNode>();
            }

            var value = node.Value;
            var valueType = value?.GetType()
                ?? node.EdgeFromParent?.MemberType
                ?? typeof(object);
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            IEnumerable<ObjectTreeNode> childNodeEnumerable;
            if (node.NodeType == ObjectTreeNodeType.Primitive)
            {
                childNodeEnumerable = Enumerable.Empty<ObjectTreeNode>();
            }
            else if (node.NodeType == ObjectTreeNodeType.Object)
            {
                childNodeEnumerable = CreateObjectChildNodes(value, node, childSpawnStrategyOverride);
            }
            else if (node.NodeType == ObjectTreeNodeType.Collection && TypeUtils.IsAssignableFrom(typeof(System.Collections.IEnumerable), valueType))
            {
                var enumerable = (System.Collections.IEnumerable)value;
                childNodeEnumerable = CreateEnumerableChildNodes(enumerable, node, childSpawnStrategyOverride);
            }
            else
            {
                childNodeEnumerable = Enumerable.Empty<ObjectTreeNode>();
            }
            return childNodeEnumerable;
        }

        private IEnumerable<ObjectTreeNode> CreateEnumerableChildNodes(System.Collections.IEnumerable enumerable, ObjectTreeNode parentNode, IObjectTreeSpawnStrategy childSpawnStrategy)
        {
            if (enumerable == null) yield break;

            int indexCounter = 0;
            foreach (var element in enumerable)
            {
                var edgeFromParent = new ObjectTreeEdge(indexCounter);
                var nodeType = DetermineNodeType(element, parentNode, edgeFromParent);
                yield return new ObjectTreeNode(element, nodeType, parentNode, edgeFromParent, spawnStrategy: childSpawnStrategy);
                indexCounter++;
            }
        }

        private IEnumerable<ObjectTreeNode> CreateObjectChildNodes(object value, ObjectTreeNode parentNode, IObjectTreeSpawnStrategy childSpawnStrategy)
        {
            if (value == null) return Enumerable.Empty<ObjectTreeNode>();

            var parentValueType = value.GetType();
            var propertyChildNodes = (
                from property in TypeUtils.GetProperties(parentValueType)
                where property.CanRead && !property.GetIndexParameters().Any()
                let childValue = GetPropertyValue(property, value)
                let edgeFromParent = new ObjectTreeEdge(property)
                let nodeType = DetermineNodeType(childValue, parentNode, edgeFromParent)
                select new ObjectTreeNode(childValue, nodeType, parentNode, edgeFromParent, spawnStrategy: childSpawnStrategy)
            );
            var fieldChildNodes = (
                from field in TypeUtils.GetFields(parentValueType)
                where !MemberInfoUtils.IsConstantField(field)
                let childValue = GetFieldValue(field, value)
                let edgeFromParent = new ObjectTreeEdge(field)
                let nodeType = DetermineNodeType(childValue, parentNode, edgeFromParent)
                select new ObjectTreeNode(childValue, nodeType, parentNode, new ObjectTreeEdge(field), spawnStrategy: childSpawnStrategy)
            );
            return propertyChildNodes.Concat(fieldChildNodes);
        }

        private static object GetPropertyValue(PropertyInfo property, object value)
        {
            try
            {
                return property.GetValue(value, index: null);
            }
            catch (TargetInvocationException ex)
            {
                throw new TargetInvocationException($"Property \"{property.Name}\" with declaring type \"{property.DeclaringType?.FullName}\"", ex);
            }
        }

        private static object GetFieldValue(FieldInfo field, object value)
        {
            try
            {
                return field.GetValue(value);
            }
            catch (TargetInvocationException ex)
            {
                throw new TargetInvocationException($"Field \"{field.Name}\" with declaring type \"{field.DeclaringType?.FullName}\"", ex);
            }
        }
    }

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

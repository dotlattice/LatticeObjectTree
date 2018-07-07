using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LatticeObjectTree
{
    /// <summary>
    /// A spawn strategy that generates child nodes from properties, fields, and enumerables.  
    /// This does not include any duplicate object or cycle checking.
    /// </summary>
    public class BasicObjectTreeSpawnStrategy : IObjectTreeSpawnStrategy
    {
        /// <summary />
        public BasicObjectTreeSpawnStrategy() : this(options: null) { }

        /// <summary>
        /// Constructs with the given options.
        /// </summary>
        /// <param name="options">(optional) options that control how the tree is built</param>
        public BasicObjectTreeSpawnStrategy(IObjectTreeOptions options)
        {
            this.Options = options;
        }

        /// <summary>
        /// The options used by this spawn strategy, or null if no options were specified.
        /// </summary>
        public IObjectTreeOptions Options { get; }

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

        private ObjectTreeNodeType DetermineNodeType(Type valueType)
        {
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            ObjectTreeNodeType nodeType;
            if (IsPrimitiveType(valueType))
            {
                nodeType = ObjectTreeNodeType.Primitive;
            }
            else if (TypeUtils.IsCollection(valueType))
            {
                nodeType = ObjectTreeNodeType.Collection;
            }
            else if (TypeUtils.IsDictionary(valueType))
            {
                //nodeType = Options?.ProcessDictionariesAsCollections == true 
                //    ? ObjectTreeNodeType.Collection 
                //    : ObjectTreeNodeType.Object;
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

            if (Options?.NodeFilter != null)
            {
                childNodeEnumerable = Options.NodeFilter.Apply(childNodeEnumerable);
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
            if (TypeUtils.IsDictionary(parentValueType))
            {
                if (value is System.Collections.IDictionary dictionaryValue)
                {
                    var dictionaryNodes = new List<ObjectTreeNode>(dictionaryValue.Count);
                    var dictionaryEnumerator = dictionaryValue.GetEnumerator();
                    while (dictionaryEnumerator.MoveNext())
                    {
                        var entryKey = dictionaryEnumerator.Key;
                        var childValue = dictionaryEnumerator.Value;
                        var edgeFromParent = new ObjectTreeEdge(entryKey);
                        var nodeType = DetermineNodeType(childValue, parentNode, edgeFromParent);
                        dictionaryNodes.Add(new ObjectTreeNode(childValue, nodeType, parentNode, edgeFromParent, spawnStrategy: childSpawnStrategy));
                    }
                    return dictionaryNodes;
                }
            }

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
}

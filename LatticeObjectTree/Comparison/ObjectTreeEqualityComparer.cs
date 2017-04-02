using LatticeObjectTree.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree.Comparison
{
    /// <summary>
    /// Compares two objects recursively based on their <see cref="ObjectTree"/> representations.
    /// </summary>
    public class ObjectTreeEqualityComparer : IEqualityComparer<ObjectTree>, IEqualityComparer<object>
    {
        private readonly IEqualityComparer<object> valueEqualityComparer;
        private readonly ICustomFormatter valueFormatter;

        /// <summary>
        /// Constructs a default equality comparer.
        /// </summary>
        public ObjectTreeEqualityComparer()
            : this(valueEqualityComparer: null, valueFormatter: null)
        {
        }

        /// <summary>
        /// Constructs an equality comparer using the specified value comparer and formatter.
        /// </summary>
        /// <param name="valueEqualityComparer">the equality comparer for values within the tree, or null to use a default comparer</param>
        /// <param name="valueFormatter">the formatter for values within the tree, or null to use a default formatter</param>
        /// <exception cref="ArgumentException">if the <paramref name="valueEqualityComparer"/> is another <see cref="ObjectTreeEqualityComparer"/></exception>
        public ObjectTreeEqualityComparer(IEqualityComparer<object> valueEqualityComparer, ICustomFormatter valueFormatter)
        {
            if (valueEqualityComparer is ObjectTreeEqualityComparer) throw new ArgumentException($"Cannot use an {nameof(ObjectTreeEqualityComparer)} as a value equality comparer");

            this.valueEqualityComparer = valueEqualityComparer ?? ObjectTreeValueEqualityComparer.Instance;
            this.valueFormatter = valueFormatter ?? ObjectTreeValueFormatter.Instance;
        }

        #region EqualityComparer

        /// <summary>
        /// Returns true if there are no differences between the trees of the two objects.
        /// </summary>
        /// <param name="x">one of the objects to compare</param>
        /// <param name="y">one of the objects to compare</param>
        /// <returns>true if the object tree representation of the two objects are equal</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in the <see cref="ObjectTree"/></exception>
        public new bool Equals(object x, object y)
        {
            return !FindDifferences(x, y).Any();
        }

        /// <summary>
        /// Returns true if there are no differences between the filtered trees of the two objects.
        /// </summary>
        /// <param name="x">one of the objects to compare</param>
        /// <param name="y">one of the objects to compare</param>
        /// <param name="nodeFilter">a filter to control how the objects are compared</param>
        /// <returns>true if the filtered object tree representation of the two objects are equal</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in the <see cref="ObjectTree"/></exception>
        public bool Equals(object x, object y, IObjectTreeNodeFilter nodeFilter)
        {
            return !FindDifferences(x, y, nodeFilter).Any();
        }

        /// <summary>
        /// Returns true if there are no differences between the two object trees.
        /// </summary>
        /// <param name="x">one of the object trees to compare</param>
        /// <param name="y">one of the object trees to compare</param>
        /// <returns>true if the two object trees are equal</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in the <see cref="ObjectTree"/></exception>
        public bool Equals(ObjectTree x, ObjectTree y)
        {
            return !FindDifferences(x, y).Any();
        }

        /// <summary>
        /// Returns a hash code for the object based on the hashcode values from each node in its object tree representation.
        /// </summary>
        /// <param name="obj">the object or object tree</param>
        /// <returns>the hashcode of the object</returns>
        public int GetHashCode(object obj)
        {
            return GetHashCode(ObjectTree.Create(obj));
        }

        /// <summary>
        /// Returns a hash code for the object based on the hashcode values from each node in its object tree representation.
        /// </summary>
        /// <param name="obj">the object or object tree</param>
        /// <param name="filter">the filter to apply to the object tree</param>
        /// <returns>the hashcode of the object</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in the <see cref="ObjectTree"/></exception>
        public int GetHashCode(object obj, IObjectTreeNodeFilter filter)
        {
            return GetHashCode(ObjectTree.Create(obj, filter));
        }

        /// <summary>
        /// Returns a hash code for the object tree based on the hashcode values from each of its nodes.
        /// </summary>
        /// <param name="objTree">the object tree</param>
        /// <returns>the hashcode of the object represented by the tree</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in the <see cref="ObjectTree"/></exception>
        public int GetHashCode(ObjectTree objTree)
        {
            return GetHashCodeRecursive(new[] { objTree.RootNode }, level: 0);
        }

        private int GetHashCodeRecursive(IEnumerable<ObjectTreeNode> nodes, int level)
        {
            // Protection from infinite recursion
            const int maxLevel = 1000;
            if (level > maxLevel)
            {
                throw new ObjectTreeCircularReferenceException($"Exceeded max nested object level of {maxLevel}");
            }

            int hashCode = 7;
            foreach (var node in nodes)
            {
                if (node.EdgeFromParent != null)
                {
                    hashCode = 31 * hashCode + node.EdgeFromParent.GetHashCode();
                }

                var childNodes = node.ChildNodes.ToList();
                hashCode = 31 * hashCode + childNodes.Count;
                if (childNodes.Any())
                {
                    hashCode = 31 * GetHashCodeRecursive(childNodes, level: level + 1);
                }
                else
                {
                    var valueHashCode = node.Value != null ? valueEqualityComparer.GetHashCode(node.Value) : 0;
                    hashCode = 31 * hashCode + valueHashCode;
                }
            }
            return hashCode;
        }

        #endregion

        #region Differences

        /// <summary>
        /// Compares the two objects recursively based on their object tree representations.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <returns>any differences detected between the two objects</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in an <see cref="ObjectTree"/></exception>
        public IEnumerable<ObjectTreeNodeDifference> FindDifferences(object expected, object actual)
        {
            return FindDifferences(ObjectTree.Create(expected), ObjectTree.Create(actual));
        }

        /// <summary>
        /// Compares the two objects recursively based on their filtered object tree representations.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="nodeFilter">a filter that controls how the objects are compared</param>
        /// <returns>any differences detected between the two objects</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in an <see cref="ObjectTree"/></exception>
        public IEnumerable<ObjectTreeNodeDifference> FindDifferences(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            return FindDifferences(ObjectTree.Create(expected, nodeFilter), ObjectTree.Create(actual, nodeFilter));
        }

        /// <summary>
        /// Compares the two object trees recursively.
        /// </summary>
        /// <param name="expected">the tree of the expected object</param>
        /// <param name="actual">the tree of the actual object</param>
        /// <returns>any differences detected between the two object trees</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in an <see cref="ObjectTree"/></exception>
        public IEnumerable<ObjectTreeNodeDifference> FindDifferences(ObjectTree expected, ObjectTree actual)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (actual == null) throw new ArgumentNullException(nameof(actual));

            return FindDifferences(expected.RootNode, actual.RootNode);
        }

        /// <summary>
        /// Compares the two object tree nodes recursively.
        /// </summary>
        /// <param name="expected">the node of the expected object</param>
        /// <param name="actual">the node of the actual object</param>
        /// <returns>any differences detected between the two nodes</returns>
        /// <exception cref="ObjectTreeCircularReferenceException">if there's a cycle in an <see cref="ObjectTree"/></exception>
        public IEnumerable<ObjectTreeNodeDifference> FindDifferences(ObjectTreeNode expected, ObjectTreeNode actual)
        {
            return FindDifferencesRecursive(expected, actual, level: 0);
        }

        private IEnumerable<ObjectTreeNodeDifference> FindDifferencesRecursive(ObjectTreeNode expectedNode, ObjectTreeNode actualNode, int level)
        {
            // Protection from inifinite recursion
            const int maxLevel = 1000;
            if (level > maxLevel)
            {
                throw new ObjectTreeCircularReferenceException($"Exceeded max nested object level of {maxLevel}");
            }

            if (expectedNode == null) throw new ArgumentNullException(nameof(expectedNode));
            if (actualNode == null) throw new ArgumentNullException(nameof(actualNode));

            var expectedPath = expectedNode.ToEdgePath();
            var actualPath = actualNode.ToEdgePath();
            if (expectedPath != actualPath)
            {
                var message = $"Expected path \"{expectedPath}\" but was \"{actualPath}\"";
                yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
                yield break;
            }

            // If either node is a duplicate or another node in its tree, then we need to do some special handling.
            var expectedOriginalNode = expectedNode.OriginalNode;
            var actualOriginalNode = actualNode.OriginalNode;
            if (expectedOriginalNode != null || actualOriginalNode != null)
            {
                // Basically, our strategy here will be to take the path to the original from one of the trees and apply it to the other.
                // Then we'll check to see if the resolved original is the same as the current value, and if not we'll consider that a difference?
                // It's probably not a perfect strategy, but it should be good enough for now for these obscure situations.
                var isOriginalNodePathDifferent = (expectedOriginalNode != null && actualOriginalNode != null && expectedOriginalNode.ToEdgePath() != actualOriginalNode.ToEdgePath());
                if (expectedOriginalNode == null || isOriginalNodePathDifferent)
                {
                    var actualOriginalNodePath = actualOriginalNode.ToEdgePath();
                    var expectedRoot = GetRootNode(expectedNode);
                    object resolvedExpectedOriginalValue;
                    if (actualOriginalNodePath.TryResolve(expectedRoot, out resolvedExpectedOriginalValue) && !AreValuesEqual(resolvedExpectedOriginalValue, actualNode.Value))
                    {
                        var messageFormat = $"{EscapeMessageFormat(expectedPath)}: expected value {{0}} but was {{1}}.";
                        yield return new ObjectTreeNodeDifference(expectedNode, FormatValue(expectedNode.Value), actualNode, FormatValue(actualNode.Value), messageFormat: messageFormat);
                        yield break;
                    }
                }
                else if (actualOriginalNode == null || isOriginalNodePathDifferent)
                {
                    var expectedOriginalNodePath = expectedOriginalNode.ToEdgePath();
                    var actualRoot = GetRootNode(actualNode);
                    object resolvedActualOriginalValue;
                    if (expectedOriginalNodePath.TryResolve(actualRoot, out resolvedActualOriginalValue) && !AreValuesEqual(resolvedActualOriginalValue, actualNode.Value))
                    {
                        var messageFormat = $"{EscapeMessageFormat(expectedPath)}: expected value {{0}} but was {{1}}.";
                        yield return new ObjectTreeNodeDifference(expectedNode, FormatValue(expectedNode.Value), actualNode, FormatValue(actualNode.Value), messageFormat: messageFormat);
                        yield break;
                    }
                }
                
                yield break;
            }

            if (expectedNode.NodeType != actualNode.NodeType)
            {
                var message = $"{expectedPath}: expected node type {Enum.GetName(typeof(ObjectTreeNodeType), expectedNode.NodeType)} but was {Enum.GetName(typeof(ObjectTreeNodeType), actualNode.NodeType)}.";
                yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
                yield break;
            }

            var expectedChildren = expectedNode.ChildNodes.ToList();
            var actualChildren = actualNode.ChildNodes.ToList();
            if (expectedChildren.Count != actualChildren.Count)
            {
                var message = $"{expectedPath}: expected {expectedChildren.Count} children but had {actualChildren.Count} children";
                yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
            }

            if (expectedChildren.Any())
            {
                foreach (var expectedChild in expectedChildren)
                {
                    var actualChild = actualChildren.FirstOrDefault(a => Object.Equals(a.EdgeFromParent, expectedChild.EdgeFromParent));
                    if (actualChild == null)
                    {
                        var message = $"{expectedPath}: expected a child at \"{expectedChild.EdgeFromParent}\" but did not find one.";
                        yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
                        continue;
                    }

                    var descendantDifferences = FindDifferencesRecursive(expectedChild, actualChild, level + 1);
                    foreach (var descendantDifference in descendantDifferences)
                    {
                        yield return descendantDifference;
                    }
                }
            }
            else if (!actualChildren.Any() && expectedNode.NodeType != ObjectTreeNodeType.Collection)
            {
                var expectedValue = expectedNode.Value;
                var actualValue = actualNode.Value;
                if (!AreValuesEqual(expectedValue, actualValue))
                {
                    var messageFormat = $"{EscapeMessageFormat(expectedPath)}: expected value {{0}} but was {{1}}.";
                    yield return new ObjectTreeNodeDifference(expectedNode, FormatValue(expectedValue), actualNode, FormatValue(actualValue), messageFormat: messageFormat);
                }
            }
        }

        private static ObjectTreeNode GetRootNode(ObjectTreeNode node)
        {
            var ancestorNode = node;
            while (ancestorNode != null)
            {
                ancestorNode = ancestorNode.ParentNode;
            }
            return ancestorNode;
        }

        private bool AreValuesEqual(object expected, object actual)
        {
            return valueEqualityComparer.Equals(expected, actual);
        }

        private string FormatValue(object value)
        {
            return valueFormatter.Format(format: null, arg: value, formatProvider: null);
        }

        private static string EscapeMessageFormat(object message)
        {
            if (message == null) return null;
            return message.ToString().Replace("{", "{{").Replace("}", "}}");
        }

        #endregion
    }
}

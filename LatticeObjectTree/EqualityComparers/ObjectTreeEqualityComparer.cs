using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.Comparers
{
    /// <summary>
    /// A comparer for comparing two objects recursively based on their object tree representations.
    /// </summary>
    public class ObjectTreeEqualityComparer : IEqualityComparer<ObjectTree>
    {
        #region EqualityComparer

        /// <summary>
        /// Returns true if there are no differences between the two object trees.
        /// </summary>
        /// <param name="x">one of the object trees to compare</param>
        /// <param name="y">one of the object trees to compare</param>
        /// <returns>true if the object trees are equal</returns>
        public bool Equals(ObjectTree x, ObjectTree y)
        {
            var differences = FindDifferences(x, y);
            return !differences.Any();
        }

        /// <summary>
        /// Returns a hash code for the object tree based on the hashcode values from each node in the tree.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(ObjectTree obj)
        {
            return GetHashCodeRecursive(new[] { obj.RootNode }, level: 0);
        }

        private int GetHashCodeRecursive(IEnumerable<ObjectTreeNode> nodes, int level)
        {
            // Protection from infinite recursion
            const int maxLevel = 1000;
            if (level > maxLevel)
            {
                throw new InvalidOperationException(string.Format("Exceeded max nested object level of {0}", maxLevel));
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
                    hashCode = 31 * hashCode + GetValueHashCode(node.Value);
                }
            }
            return hashCode;
        }

        /// <summary>
        /// Returns a hash code for a value from a leaf node of an object tree.
        /// </summary>
        protected virtual int GetValueHashCode(object value)
        {
            return value != null ? value.GetHashCode() : 0;
        }

        #endregion

        #region Differences

        /// <summary>
        /// Compares the two objects recursively.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <returns>any differences detected between the two objects</returns>
        public IEnumerable<ObjectTreeNodeDifference> FindDifferences(object expected, object actual)
        {
            return FindDifferences(ObjectTree.Create(expected), ObjectTree.Create(actual));
        }

        /// <summary>
        /// Compares the two object trees recursively.
        /// </summary>
        /// <param name="expected">the tree of the expected object</param>
        /// <param name="actual">the tree of the actual object</param>
        /// <returns>any differences detected between the two trees</returns>
        public IEnumerable<ObjectTreeNodeDifference> FindDifferences(ObjectTree expected, ObjectTree actual)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (actual == null) throw new ArgumentNullException("actual");

            return FindDifferences(expected.RootNode, actual.RootNode);
        }

        /// <summary>
        /// Compares the two object tree nodes recursively.
        /// </summary>
        /// <param name="expected">the node of the expected object</param>
        /// <param name="actual">the node of the actual object</param>
        /// <returns>any differences detected between the two nodes</returns>
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
                throw new InvalidOperationException(string.Format("Exceeded max nested object level of {0}", maxLevel));
            }

            if (expectedNode == null) throw new ArgumentNullException("expected");
            if (actualNode == null) throw new ArgumentNullException("actual");

            var expectedPath = expectedNode.ToEdgePath();
            var actualPath = actualNode.ToEdgePath();
            if (expectedPath != actualPath)
            {
                var message = string.Format("Expected path \"{0}\" but was \"{1}\"", expectedPath, actualPath);
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
                        var message = string.Format("{0}: expected value {1} but was {2}.", expectedPath.ToString(), FormatValue(expectedNode.Value), FormatValue(actualNode.Value));
                        yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
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
                        var message = string.Format("{0}: expected value {1} but was {2}.", expectedPath.ToString(), FormatValue(expectedNode.Value), FormatValue(actualNode.Value));
                        yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
                        yield break;
                    }
                }
                
                yield break;
            }

            var expectedChildren = expectedNode.ChildNodes.ToList();
            var actualChildren = actualNode.ChildNodes.ToList();
            if (expectedChildren.Count != actualChildren.Count)
            {
                var message = string.Format("{0}: expected {1} children but had {2} children", expectedPath.ToString(), expectedChildren.Count, actualChildren.Count);
                yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
            }

            if (expectedChildren.Any())
            {
                foreach (var expectedChild in expectedChildren)
                {
                    var actualChild = actualChildren.FirstOrDefault(a => Equals(a.EdgeFromParent, expectedChild.EdgeFromParent));
                    if (actualChild == null)
                    {
                        var message = string.Format("{0}: expected a child at \"{1}\" but did not find one.", expectedPath.ToString(), expectedChild.EdgeFromParent.ToString());
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
            else if (!actualChildren.Any())
            {
                var expectedValue = expectedNode.Value;
                var actualValue = actualNode.Value;
                if (!AreValuesEqual(expectedValue, actualValue))
                {
                    var message = string.Format("{0}: expected value {1} but was {2}.", expectedPath.ToString(), FormatValue(expectedValue), FormatValue(actualValue));
                    yield return new ObjectTreeNodeDifference(expectedNode, actualNode, message);
                }
            }
        }

        /// <summary>
        /// Compares two values from a leaf node in an object tree.
        /// </summary>
        protected virtual bool AreValuesEqual(object expected, object actual)
        {
            if (expected == null || actual == null)
            {
                return Object.Equals(expected, actual);
            }

            var expectedType = expected.GetType();
            var actualType = actual.GetType();
            if (expectedType != actualType)
            {
                return Object.Equals(expected, actual);
            }

            // Special handling for a few types
            var type = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
            if (type == typeof(float))
            {
                return Math.Abs((float)expected - (float)actual) < float.Epsilon;
            }
            else if (type == typeof(double))
            {
                return Math.Abs((double)expected - (double)actual) < double.Epsilon;
            }
            else
            {
                return Object.Equals(expected, actual);
            }
        }

        /// <summary>
        /// Formats a value for use in a the message in a <c>ObjectTreeNodeDifference</c>.
        /// </summary>
        protected virtual string FormatValue(object value)
        {
            if (value == null) return "null";

            var valueType = value.GetType();
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            string valueString;
            if (valueType == typeof(float))
            {
                valueString = ((float)value).ToString("R") + 'f';
            }
            else if (valueType == typeof(double))
            {
                valueString = ((double)value).ToString("R") + 'd';
            }
            else
            {
                valueString = value.ToString();
                if (valueType == typeof(decimal))
                {
                    valueString += 'm';
                }
                else if (valueType == typeof(bool))
                {
                    valueString = valueString.ToLower();
                }
                else if (valueType.IsEnum)
                {
                    valueString = valueType.Name + "." + valueString;
                }
            }

            // If the string contains any "special" characters, then we'll use the verbatim string literal syntax.
            char[] specialCharacters = new[] { '\'', '"', '\n', '\r', '\t', '\0', '\a', '\b', '\f', '\v' };
            if (valueString.Any(specialCharacters.Contains))
            {
                valueString = "@\"" + valueString.Replace("\"", "\"\"") + "\"";
            }
            else
            {
                valueString = "\"" + valueString + "\"";
            }

            return valueString;
        }

        /// <summary>
        /// Returns the root of the tree that the specified node is in.
        /// </summary>
        private static ObjectTreeNode GetRootNode(ObjectTreeNode node)
        {
            var ancestorNode = node;
            while (ancestorNode != null)
            {
                ancestorNode = ancestorNode.ParentNode;
            }
            return ancestorNode;
        }

        #endregion
    }
}

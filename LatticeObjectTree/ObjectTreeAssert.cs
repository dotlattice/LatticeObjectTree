using LatticeObjectTree.Comparison;
using LatticeObjectTree.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LatticeObjectTree
{
    /// <summary>
    /// Provides methods for comparing objects based on their object tree representations.
    /// </summary>
    public static class ObjectTreeAssert
    {
        /// <summary>
        /// Verifies that two objects are equal based on their object tree representations. 
        /// If they are not, an <see cref="ObjectTreeEqualException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <exception cref="ObjectTreeEqualException">if the two objects are not equal</exception>
        public static void AreEqual(object expected, object actual)
        {
            AreEqual(expected, actual, nodeFilter: null);
        }

        /// <summary>
        /// Verifies that two objects are equal based on their filtered object tree representations. 
        /// If they are not, an <see cref="ObjectTreeEqualException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="nodeFilter">a filter on the comparison</param>
        /// <exception cref="ObjectTreeEqualException">if the two objects are not equal</exception>
        public static void AreEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            var expectedTree = ObjectTree.Create(expected, nodeFilter);
            var actualTree = ObjectTree.Create(actual, nodeFilter);
            var differences = new ObjectTreeEqualityComparer().FindDifferences(expectedTree, actualTree);
            if (differences.Any())
            {
                throw new ObjectTreeEqualException(expectedTree, actualTree, differences);
            }
        }

        /// <summary>
        /// Verifies that two objects are not equal based on their object tree representations. 
        /// If they are equal, an <see cref="ObjectTreeNotEqualException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <exception cref="ObjectTreeNotEqualException">if the two objects are equal</exception>
        public static void AreNotEqual(object expected, object actual)
        {
            AreNotEqual(expected, actual, nodeFilter: null);
        }

        /// <summary>
        /// Verifies that two objects are not equal based on their filtered object tree representations. 
        /// If they are equal, an <see cref="ObjectTreeNotEqualException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="nodeFilter">a filter on the nodes of the object trees to include in the comparison</param>
        /// <exception cref="ObjectTreeNotEqualException">if the two objects are equal</exception>
        public static void AreNotEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            var expectedTree = ObjectTree.Create(expected, nodeFilter);
            var actualTree = ObjectTree.Create(actual, nodeFilter);
            var differences = new ObjectTreeEqualityComparer().FindDifferences(expectedTree, actualTree);
            if (!differences.Any())
            {
                throw new ObjectTreeNotEqualException(expectedTree, actualTree);
            }
        }

        #region Invalid

        /// <summary>
        /// Always throws an <see cref="InvalidOperationException" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new bool Equals(object a, object b)
        {
            throw new InvalidOperationException($"{nameof(ObjectTreeAssert)}.{nameof(Equals)} should not be used for assertions");
        }

        /// <summary>
        /// Always throws an <see cref="InvalidOperationException" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new void ReferenceEquals(object a, object b)
        {
            throw new InvalidOperationException($"{ nameof(ObjectTreeAssert) }.{ nameof(ReferenceEquals)} should not be used for assertions");
        }

        #endregion
    }
}

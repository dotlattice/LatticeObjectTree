using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using LatticeObjectTree.Comparers;
using NUnit.Framework;
using LatticeObjectTree.NUnit.Constraints;

namespace LatticeObjectTree.NUnit.Comparers
{
    /// <summary>
    /// Provides methods for examining objects based on their object tree representations.
    /// </summary>
    public static class ObjectTreeAssert
    {
        /// <summary>
        /// Verifies that two objects are equal based on their object trees. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        public static void AreEqual(object expected, object actual)
        {
            AreEqual(actual, expected, message: default(string), args: default(object[]));
        }

        /// <summary>
        /// Verifies that two objects are equal based on their object trees. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="message">the message to display in case of failure</param>
        public static void AreEqual(object expected, object actual, string message)
        {
            AreEqual(actual, expected, message, args: default(object[]));
        }

        /// <summary>
        /// Verifies that two objects are equal based on their object trees. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="message">the message to display in case of failure</param>
        /// <param name="args">objects to be used in formatting the message</param>
        public static void AreEqual(object expected, object actual, string message, params object[] args)
        {
            AreEqual(ObjectTree.Create(actual), ObjectTree.Create(expected), message, args);
        }

        /// <summary>
        /// Verifies that two objects are equal based on their filtered object trees. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="nodeFilter">a filter on the nodes of the object trees to include in the comparison</param>
        public static void AreEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            AreEqual(actual, expected, nodeFilter, message: default(string), args: default(object[]));
        }

        /// <summary>
        /// Verifies that two objects are equal based on their filtered object trees. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="nodeFilter">a filter on the nodes of the object trees to include in the comparison</param>
        /// <param name="message">the message to display in case of failure</param>
        public static void AreEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter, string message)
        {
            AreEqual(actual, expected, nodeFilter, message, args: default(object[]));
        }

        /// <summary>
        /// Verifies that two objects are equal based on their filtered object trees. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="nodeFilter">a filter on the nodes of the object trees to include in the comparison</param>
        /// <param name="message">the message to display in case of failure</param>
        /// <param name="args">objects to be used in formatting the message</param>
        public static void AreEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter, string message, params object[] args)
        {
            AreEqual(new ObjectTree(actual, nodeFilter), new ObjectTree(expected, nodeFilter), message, args);
        }

        /// <summary>
        /// Verifies that two object trees are equal. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        public static void AreEqual(ObjectTree expected, ObjectTree actual)
        {
            AreEqual(actual, expected, message: default(string), args: default(object[]));
        }

        /// <summary>
        /// Verifies that two object trees are equal. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="message">the message to display in case of failure</param>
        public static void AreEqual(ObjectTree expected, ObjectTree actual, string message)
        {
            AreEqual(actual, expected, message, args: default(object[]));
        }

        /// <summary>
        /// Verifies that two object trees are equal. 
        /// If they are not, an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">the expected object</param>
        /// <param name="actual">the actual object</param>
        /// <param name="message">the message to display in case of failure</param>
        /// <param name="args">objects to be used in formatting the message</param>
        public static void AreEqual(ObjectTree expected, ObjectTree actual, string message, params object[] args)
        {
            Assert.That(actual, new ObjectTreeEqualConstraint(expected), message, args);
        }

        #region Equals and ReferenceEquals

        /// <summary>
        /// Always throws an <c>AssertionException</c>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new bool Equals(object a, object b)
        {
            throw new AssertionException("ObjectTreeAssert.Equals should not be used for assertions");
        }

        /// <summary>
        /// Always throws an <c>AssertionException</c>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new void ReferenceEquals(object a, object b)
        {
            throw new AssertionException("ObjectTreeAssert.ReferenceEquals should not be used for assertions");
        }

        #endregion
    }
}

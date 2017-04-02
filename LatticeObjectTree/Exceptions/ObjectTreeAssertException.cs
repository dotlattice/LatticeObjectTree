using LatticeObjectTree.Comparison;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree.Exceptions
{
    /// <summary>
    /// The base class for <see cref="ObjectTree"/> assertion exceptions.
    /// </summary>
    public class ObjectTreeAssertException : Exception
    {
        /// <summary>
        /// Constructs an exception for the expected and actual ObjectTree values.
        /// </summary>
        /// <param name="expectedTree">the expected tree</param>
        /// <param name="actualTree">the actual tree</param>
        /// <param name="message">the base message describing the exception</param>
        public ObjectTreeAssertException(ObjectTree expectedTree, ObjectTree actualTree, string message)
            : base(message)
        {
            ExpectedTree = expectedTree;
            ActualTree = actualTree;
        }

        /// <summary>
        /// The expected <see cref="ObjectTree"/> value.
        /// </summary>
        public ObjectTree ExpectedTree { get; }

        /// <summary>
        /// The actual <see cref="ObjectTree"/> value.
        /// </summary>
        public ObjectTree ActualTree { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree.Asserts
{
    /// <summary>
    /// Exception thrown when two <see cref="ObjectTree"/> values are unexpectedly equal.
    /// </summary>
    public class ObjectTreeNotEqualException : ObjectTreeAssertException
    {
        /// <summary>
        /// Constructs an exception for the specified expected and actual object trees.
        /// </summary>
        /// <param name="expectedTree">the expected ObjectTree</param>
        /// <param name="actualTree">the actual ObjectTree</param>
        public ObjectTreeNotEqualException(ObjectTree expectedTree, ObjectTree actualTree)
            : base(expectedTree, actualTree, $"{nameof(ObjectTreeAssert)}.{nameof(ObjectTreeAssert.AreNotEqual)}() Failure")
        { }
    }
}
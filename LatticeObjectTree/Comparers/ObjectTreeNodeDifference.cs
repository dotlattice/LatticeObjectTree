using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.Comparers
{
    /// <summary>
    /// A difference between two nodes in an object tree.
    /// </summary>
    public class ObjectTreeNodeDifference
    {
        /// <summary>
        /// Constructs a difference for the specified expected and actual nodes.
        /// </summary>
        /// <param name="expected">the node that was expected</param>
        /// <param name="actual">the node that was actually found</param>
        /// <param name="message">describes the difference between the nodes</param>
        public ObjectTreeNodeDifference(ObjectTreeNode expected, ObjectTreeNode actual, string message)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (actual == null) throw new ArgumentNullException(nameof(actual));

            Expected = expected;
            Actual = actual;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// The node that was expected.
        /// </summary>
        public ObjectTreeNode Expected { get; }

        /// <summary>
        /// The node that was actually in the object tree.
        /// </summary>
        public ObjectTreeNode Actual { get; }

        /// <summary>
        /// A message describing the difference between the nodes.  This may be blank, but it will never be null.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Returns a description of the difference betwen the expected and actual nodes.
        /// </summary>
        /// <returns>the string representation of this difference</returns>
        public override string ToString()
        {
            return Message;
        }
    }
}

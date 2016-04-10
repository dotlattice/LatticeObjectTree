using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree
{
    /// <summary>
    /// The type of an <see cref="ObjectTreeNode"/>.
    /// </summary>
    public enum ObjectTreeNodeType
    {
        /// <summary>
        /// The node's type is unknown or unspecified.
        /// </summary>
        Unknown,

        /// <summary>
        /// The node is for a primitive value (like an integer, decimal, character, string, etc.)
        /// </summary>
        Primitive,

        /// <summary>
        /// The node is for an object value that may have child nodes.
        /// </summary>
        Object,

        /// <summary>
        /// The node is for a collection or other enumerable that may have child nodes.
        /// </summary>
        Collection,
    }
}

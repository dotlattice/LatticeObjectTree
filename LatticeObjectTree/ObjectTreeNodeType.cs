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
        /// For primitive value nodes that won't have child nodes (like an integer, decimal, character, or string).
        /// </summary>
        Primitive,

        /// <summary>
        /// For an object value node that may have child nodes.
        /// </summary>
        Object,

        /// <summary>
        /// For a collection or other enumerable value node that may have child nodes.
        /// </summary>
        Collection,
    }
}

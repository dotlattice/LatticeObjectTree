using LatticeObjectTree.NUnit.Constraints;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.NUnit
{
    /// <summary>
    /// An equivalent of the NUnit <c>Is</c> class that use ObjectTrees.
    /// </summary>
    public static class IsObjectTree
    {
        /// <summary>
        /// Returns a constraint that tests two items for object tree equality.
        /// </summary>
        public static ObjectTreeEqualConstraint EqualTo(object expected)
        {
            return new ObjectTreeEqualConstraint(expected);
        }

        /// <summary>
        /// Returns a constraint that tests two items for object tree inequality.
        /// </summary>
        public static ObjectTreeEqualConstraint NotEqualTo(object expected)
        {
            return Is.Not.ObjectTreeEqualTo(expected);
        }
    }
}

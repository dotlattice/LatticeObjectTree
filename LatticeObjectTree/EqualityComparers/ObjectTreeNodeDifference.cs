using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.Comparers
{
    public class ObjectTreeNodeDifference
    {
        private readonly ObjectTreeNode expected;
        private readonly ObjectTreeNode actual;
        private readonly string message;

        public ObjectTreeNodeDifference(ObjectTreeNode expected, ObjectTreeNode actual, string message)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (actual == null) throw new ArgumentNullException("actual");

            this.expected = expected;
            this.actual = actual;
            this.message = message ?? string.Empty;
        }

        public ObjectTreeNode Expected { get { return expected; } }
        public ObjectTreeNode Actual { get { return actual; } }
        public string Message { get { return message; } }

        public override string ToString()
        {
            return message;
        }
    }
}

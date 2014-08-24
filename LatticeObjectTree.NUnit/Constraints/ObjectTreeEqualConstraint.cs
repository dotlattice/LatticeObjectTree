using LatticeObjectTree.Comparers;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.NUnit.Constraints
{
    /// <summary>
    /// A constraint for object equality that uses ObjectTrees to compare objects.
    /// </summary>
    public class ObjectTreeEqualConstraint : Constraint
    {
        /// <summary>
        /// The expected tree for this constraint.
        /// </summary>
        protected readonly ObjectTree expectedTree;

        /// <summary>
        /// The differences from the last attempt at matching an actual value.
        /// </summary>
        protected ICollection<ObjectTreeNodeDifference> differences;

        /// <summary>
        /// Constructs a constraint with the specified expected value.
        /// </summary>
        /// <param name="expected">the expected value</param>
        public ObjectTreeEqualConstraint(object expected)
            : this(new ObjectTree(expected)) { }

        /// <summary>
        /// Constructs a constraint with the specified object tree for the expected value.
        /// </summary>
        /// <param name="expectedTree">the tree of the expected value</param>
        public ObjectTreeEqualConstraint(ObjectTree expectedTree)
            : base(expectedTree)
        {
            this.expectedTree = expectedTree;
            this.DisplayName = "equal";
        }

        public override bool Matches(object actual)
        {
            return Matches(ObjectTree.Create(actual));
        }

        protected virtual bool Matches(ObjectTree actualTree)
        {
            this.actual = actualTree;
            this.differences = new ObjectTreeEqualityComparer().FindDifferences(expectedTree, actualTree).ToList();
            return !this.differences.Any();
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            writer.DisplayDifferences(this);
            if (differences != null && differences.Any())
            {
                writer.Write("  {0} Differences:    ", differences.Count);
                writer.WriteCollectionElements(differences, 0, 20);
            }
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.Write("<{0}> with root ", expectedTree);
            writer.WriteExpectedValue(expectedTree.RootNode.Value);
        }

        public override void WriteActualValueTo(MessageWriter writer)
        {
            var actualTree = actual as ObjectTree;
            if (actualTree == null)
            {
                base.WriteActualValueTo(writer);
                return;
            }

            writer.Write("<{0}> with root ", actualTree);
            writer.WriteActualValue(actualTree.RootNode.Value);
        }
    }
}

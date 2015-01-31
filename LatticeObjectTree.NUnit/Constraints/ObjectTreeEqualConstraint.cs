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
        private readonly object expected;

        private ObjectTree expectedTree;
        private IObjectTreeNodeFilter nodeFilter;

        #region Matches State

        private ObjectTree actualTree;
        private ICollection<ObjectTreeNodeDifference> differences;

        #endregion

        /// <summary>
        /// Constructs a constraint with the specified expected value.
        /// </summary>
        /// <param name="expected">the expected value</param>
        public ObjectTreeEqualConstraint(object expected)
            : this(expected, nodeFilter: (IObjectTreeNodeFilter)null) { }

        /// <summary>
        /// Constructs a constraint with the specified expected value and node filter.
        /// </summary>
        /// <param name="expected">the expected value</param>
        /// <param name="nodeFilter">the filter to use, or null if no filter should be used</param>
        public ObjectTreeEqualConstraint(object expected, IObjectTreeNodeFilter nodeFilter)
        {
            this.expected = expected;
            this.nodeFilter = nodeFilter;
            this.expectedTree = ObjectTree.Create(expected, nodeFilter);
            this.DisplayName = "equal";
        }

        /// <inheritdoc />
        public override bool Matches(object actual)
        {
            this.actual = actual;
            this.actualTree = ObjectTree.Create(actual, nodeFilter);
            
            this.differences = new ObjectTreeEqualityComparer().FindDifferences(expectedTree, actualTree).ToList();
            return !this.differences.Any();
        }

        /// <inheritdoc />
        public override void WriteMessageTo(MessageWriter writer)
        {
            writer.DisplayDifferences(this);
            if (differences != null && differences.Any())
            {
                writer.Write("  {0} Differences:    ", differences.Count);
                writer.WriteCollectionElements(differences, 0, 20);
            }
        }

        /// <inheritdoc />
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.Write("<{0}> with root ", expectedTree);
            writer.WriteExpectedValue(expectedTree.RootNode.Value);
        }

        /// <inheritdoc />
        public override void WriteActualValueTo(MessageWriter writer)
        {
            if (actualTree == null)
            {
                base.WriteActualValueTo(writer);
                return;
            }

            writer.Write("<{0}> with root ", actualTree);
            writer.WriteActualValue(actualTree.RootNode.Value);
        }

        #region Constraint Modifiers

        /// <summary>
        /// Adds the specified node filter to this constraint.
        /// </summary>
        /// <param name="nodeFilter">the filter to use, or null if no filter should be used</param>
        /// <returns>self</returns>
        public ObjectTreeEqualConstraint WithFilter(IObjectTreeNodeFilter nodeFilter)
        {
            if (nodeFilter == null) throw new ArgumentNullException("nodeFilter");
            this.nodeFilter = nodeFilter;
            this.expectedTree = ObjectTree.Create(expected, nodeFilter);
            return this;
        }

        #endregion
    }
}

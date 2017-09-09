using LatticeObjectTree.Comparison;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree.Exceptions
{
    /// <summary>
    /// Exception thrown when two <see cref="ObjectTree"/> values are unexpectedly not equal.
    /// </summary>
    public class ObjectTreeEqualException : ObjectTreeAssertException
    {
        /// <summary>
        /// Constructs an exception for the specified expected and actual object trees with the specified differences between them.
        /// </summary>
        /// <param name="expectedTree">the expected tree</param>
        /// <param name="actualTree">the actual tree</param>
        /// <param name="differences">the differences between the two objects</param>
        /// <exception cref="ArgumentNullException">if <c>differences</c> is null</exception>
        /// <exception cref="ArgumentException">if <c>differences</c> is empty</exception>
        public ObjectTreeEqualException(ObjectTree expectedTree, ObjectTree actualTree, IEnumerable<ObjectTreeNodeDifference> differences)
            : base(expectedTree, actualTree, $"{nameof(ObjectTreeAssert)}.{nameof(ObjectTreeAssert.AreEqual)}() Failure")
        {
            if (differences == null) throw new ArgumentNullException(nameof(differences));
            if (!differences.Any()) throw new ArgumentException("Must have at least one difference if the expected and actual objects are not equal");

            Differences = differences.Take(100).ToList().AsReadOnly();
        }

        /// <summary>
        /// The differences detected between the expected and actual objects.
        /// </summary>
        public IEnumerable<ObjectTreeNodeDifference> Differences { get; }

        /// <summary>
        /// A message that describes the exception, include the differences between the expected and actual objects.
        /// </summary>
        public override string Message => GenerateMessage();
        private string GenerateMessage()
        {
            var differenceCollection = Differences as ICollection<ObjectTreeNodeDifference>;

            var differenceCount = differenceCollection.Count;
            var countString = (differenceCount >= 100) ? "99+" : differenceCount.ToString();
            var differenceTitle = $"{countString} Difference{(differenceCount != 1 ? "s" : "")}:";

            var differenceLineEnumerable = differenceCollection.Take(99).Select(diff => {
                var expectedDisplayValue = diff.ExpectedDisplayValue;
                var actualDisplayValue = diff.ActualDisplayValue;

                const int maxDisplayValueLength = 140;
                if (expectedDisplayValue?.Length > maxDisplayValueLength)
                {
                    expectedDisplayValue = expectedDisplayValue.Substring(0, maxDisplayValueLength).TrimEnd() + "…";
                    if (expectedDisplayValue.StartsWith("\""))
                    {
                        expectedDisplayValue += '"';
                    }
                }
                if (actualDisplayValue?.Length > maxDisplayValueLength)
                {
                    actualDisplayValue = actualDisplayValue.Substring(0, maxDisplayValueLength).TrimEnd() + "…";
                    if (actualDisplayValue.StartsWith("\""))
                    {
                        actualDisplayValue += '"';
                    }
                }

                var diffString = diff.GenerateMessage(expectedDisplayValue, actualDisplayValue).TrimEnd();
                const int maxDiffStringLength = 512;
                if (diffString.Length > maxDiffStringLength)
                {
                    diffString = diffString.Substring(0, maxDiffStringLength - 1).TrimEnd() + "…";
                }
                return '\t' + diffString;
            });
            var differenceLinesString = string.Join(Environment.NewLine, differenceLineEnumerable.ToArray());

            return $"{$"{nameof(ObjectTreeAssert)}.{nameof(ObjectTreeAssert.AreEqual)}() Failure"}{Environment.NewLine}{differenceTitle}{Environment.NewLine}{differenceLinesString}";
        }
    }
}
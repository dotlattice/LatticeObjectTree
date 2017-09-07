using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.Comparison
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
            : this(expected: expected, expectedDisplayValue: null, actual: actual, actualDisplayValue: null, messageFormat: EscapeMessageFormat(message)) { }

        private static string EscapeMessageFormat(string message)
        {
            if (message == null) return null;
            return message.Replace("{", "{{").Replace("}", "}}");
        }

        /// <summary>
        /// Constructs a difference for the specified expected and actual nodes with a message format.
        /// </summary>
        /// <param name="expected">the node that was expected</param>
        /// <param name="expectedDisplayValue">the display value of <paramref name="expected"/></param>
        /// <param name="actual">the node that was actually found</param>
        /// <param name="actualDisplayValue">the display value of <paramref name="actual"/></param>
        /// <param name="messageFormat">describes the difference between the nodes, optionally with placeholder "{0}" and "{1}" values for the expected and actual values</param>
        public ObjectTreeNodeDifference(ObjectTreeNode expected, string expectedDisplayValue, ObjectTreeNode actual, string actualDisplayValue, string messageFormat)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            if (actual == null) throw new ArgumentNullException(nameof(actual));

            ExpectedNode = expected;
            ExpectedDisplayValue = expectedDisplayValue;
            ActualNode = actual;
            ActualDisplayValue = actualDisplayValue;
            MessageFormat = messageFormat ?? string.Empty;
        }

        /// <summary>
        /// The node that was expected.  This cannot be null.
        /// </summary>
        public ObjectTreeNode ExpectedNode { get; }

        /// <summary>
        /// The default display value of <see cref="ExpectedNode"/>.  This may be null.
        /// </summary>
        public string ExpectedDisplayValue { get; }

        /// <summary>
        /// The node that was actually in the object tree.  This cannot be null.
        /// </summary>
        public ObjectTreeNode ActualNode { get; }

        /// <summary>
        /// The default display value of <see cref="ActualNode"/>.  This may be null.
        /// </summary>
        public string ActualDisplayValue { get; }

        /// <summary>
        /// The format that can generate the <see cref="Message"/> using <see cref="ExpectedDisplayValue"/> and <see cref="ActualDisplayValue"/>.
        /// </summary>
        private string MessageFormat { get; }

        /// <summary>
        /// A message describing the difference between the nodes.  This may be blank, but it will never be null.
        /// </summary>
        public string Message => GenerateMessage(ExpectedDisplayValue, ActualDisplayValue);

        /// <summary>
        /// Generates a version of the <see cref="Message"/> with the specified expected and actual values.
        /// </summary>
        /// <param name="expectedDisplayValue"></param>
        /// <param name="actualDisplayValue"></param>
        /// <returns></returns>
        public string GenerateMessage(string expectedDisplayValue, string actualDisplayValue)
        {
            try
            {
                return string.Format(MessageFormat, expectedDisplayValue, actualDisplayValue);
            }
            catch (FormatException)
            {
                return MessageFormat;
            }
        }

        /// <summary>
        /// Returns the <see cref="Message"/> of this difference.
        /// </summary>
        /// <returns><see cref="Message"/></returns>
        public override string ToString()
        {
            return Message;
        }
    }
}

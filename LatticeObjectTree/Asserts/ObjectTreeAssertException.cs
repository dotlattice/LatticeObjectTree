using LatticeObjectTree.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree.Asserts
{
    /// <summary>
    /// The base class for <see cref="ObjectTree"/> assertions.
    /// </summary>
    public class ObjectTreeAssertException : Exception
    {
        /// <summary>
        /// Constructs an exception for the expected and actual ObjectTree values.
        /// </summary>
        /// <param name="expectedTree">the expected tree</param>
        /// <param name="actualTree">the actual tree</param>
        /// <param name="message">the base message describing the exception</param>
        public ObjectTreeAssertException(ObjectTree expectedTree, ObjectTree actualTree, string message)
            : base(message)
        {
            ExpectedTree = expectedTree;
            ActualTree = actualTree;
        }

        /// <summary>
        /// The expected <c>ObjectTree</c> value.
        /// </summary>
        public ObjectTree ExpectedTree { get; }

        /// <summary>
        /// A string representation of the expected value.
        /// </summary>
        public virtual string Expected => ObjectTreeValueFormatter.Instance.Format(ExpectedTree?.RootNode.Value);

        /// <summary>
        /// The actual <c>ObjectTree</c> value.
        /// </summary>
        public ObjectTree ActualTree { get; }

        /// <summary>
        /// A string representation of the actual value.
        /// </summary>
        public virtual string Actual => ObjectTreeValueFormatter.Instance.Format(ActualTree?.RootNode.Value);

        /// <summary>
        /// A message that describes the exception, including the expected and actual values.
        /// </summary>
        public override string Message => $"{base.Message}{Environment.NewLine}Expected: {Expected}{Environment.NewLine}Actual:   {Actual}";
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeObjectTree.Exceptions
{
    /// <summary>
    /// An exception thrown if a circular reference is detected in an <see cref="ObjectTree"/>.
    /// </summary>
    public class ObjectTreeCircularReferenceException : Exception
    {
        /// <summary />
        public ObjectTreeCircularReferenceException() : base() { }

        /// <summary>
        /// Constructs an exception with the given message.
        /// </summary>
        /// <param name="message">the message that describes the exception</param>
        public ObjectTreeCircularReferenceException(string message) : base(message) { }

        /// <summary>
        /// Constructs an exception with the given message and the inner exception that caused this error.
        /// </summary>
        /// <param name="message">the message that describes the exception</param>
        /// <param name="innerException">the exception that caused of the current exception</param>
        public ObjectTreeCircularReferenceException(string message, Exception innerException) : base(message, innerException) { }
    }
}

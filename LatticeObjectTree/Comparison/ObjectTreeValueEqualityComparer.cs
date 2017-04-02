using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.Comparison
{
    /// <summary>
    /// The default value equality comparer used by the <see cref="ObjectTreeEqualityComparer"/> class.
    /// </summary>
    public class ObjectTreeValueEqualityComparer : IEqualityComparer<object>
    {
        /// <summary>
        /// A default equality comparer instance.
        /// </summary>
        public static ObjectTreeValueEqualityComparer Instance { get; } = new ObjectTreeValueEqualityComparer();

        /// <summary>
        /// The default constructor (for subclasses).
        /// </summary>
        protected ObjectTreeValueEqualityComparer() { }

        /// <summary>
        /// Returns true if the expected and actual values are equal.
        /// </summary>
        /// <param name="expected">the expected value</param>
        /// <param name="actual">the actual value</param>
        /// <returns>true if the objects are equal</returns>
        public new bool Equals(object expected, object actual)
        {
            if (object.ReferenceEquals(expected, actual)) return true;
            if (object.ReferenceEquals(expected, null) || object.ReferenceEquals(actual, null)) return object.Equals(expected, actual);

            var expectedType = expected.GetType();
            var actualType = actual.GetType();
            if (expectedType != actualType)
            {
                return object.Equals(expected, actual);
            }

            // Special handling for a few types
            var type = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
            if (type == typeof(float))
            {
                return Math.Abs((float)expected - (float)actual) < float.Epsilon;
            }
            else if (type == typeof(double))
            {
                return Math.Abs((double)expected - (double)actual) < double.Epsilon;
            }
            else if (type == typeof(byte[]))
            {
                return Enumerable.SequenceEqual(expected as byte[], actual as byte[]);
            }
            else
            {
                return object.Equals(expected, actual);
            }
        }

        /// <summary>
        /// Returns a hash code for the specified object that is compatible with the <see cref="Equals(object, object)"/> method.
        /// </summary>
        /// <param name="obj">the object for which to get a hash code</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(object obj)
        {
            if (obj == null) return 0;

            // Special handling for a few types
            var type = obj.GetType();
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof(byte[]))
            {
                var elements = (byte[])obj;
                return elements.Select(element => element.GetHashCode()).Aggregate(37, (current, elementHashCode) => (current * 397) ^ elementHashCode);
            }

            return obj.GetHashCode();
        }
    }
}

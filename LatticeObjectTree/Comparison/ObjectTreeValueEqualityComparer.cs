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
        /// Creates an equality comparer, or returns <see cref="Instance"/> if the options are null.
        /// </summary>
        public static ObjectTreeValueEqualityComparer Create(IObjectTreeCompareOptions options)
        {
            if (options == null) return Instance;
            return new ObjectTreeValueEqualityComparer(options);
        }

        /// <summary>
        /// The default constructor (for subclasses).
        /// </summary>
        protected ObjectTreeValueEqualityComparer() { }

        /// <summary>
        /// Constructs a comparer with the specified options.
        /// </summary>
        /// <param name="options">the options to use in the comparison</param>
        /// <exception cref="ArgumentNullException">if <paramref name="options"/> is null</exception>
        public ObjectTreeValueEqualityComparer(IObjectTreeCompareOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            this.Options = options;
        }

        /// <summary>
        /// The options used in this comparer, or null if no options are specified.
        /// </summary>
        public IObjectTreeCompareOptions Options { get; }

        /// <summary>
        /// Always returns true.
        /// </summary>
        public virtual bool CanCompare(Type expectedType, Type actualType)
        {
            return true;
        }

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
                var delta = Options?.FloatComparisonDelta ?? float.Epsilon;
                return Math.Abs((float)expected - (float)actual) <= delta;
            }
            else if (type == typeof(double))
            {
                var delta = Options?.DoubleComparisonDelta ?? double.Epsilon;
                return Math.Abs((double)expected - (double)actual) <= double.Epsilon;
            }
            else if (type == typeof(decimal) && (Options?.DecimalComparisonDelta).HasValue)
            {
                var delta = Options.DecimalComparisonDelta.Value;
                return Math.Abs((decimal)expected - (decimal)actual) <= delta;
            }
            else if (type == typeof(DateTime) && (Options?.DateTimeComparisonDelta).HasValue)
            {
                var expectedDt = (DateTime)expected;
                var actualDt = (DateTime)actual;
                return Math.Abs(expectedDt.Ticks - actualDt.Ticks) <= Options.DateTimeComparisonDelta.Value.Ticks;
            }
            else if (type == typeof(DateTimeOffset) && (Options?.DateTimeComparisonDelta).HasValue)
            {
                var expectedDto = (DateTimeOffset)expected;
                var actualDto = (DateTimeOffset)actual;
                return Math.Abs(expectedDto.Ticks - actualDto.Ticks) <= Options.DateTimeComparisonDelta.Value.Ticks;
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

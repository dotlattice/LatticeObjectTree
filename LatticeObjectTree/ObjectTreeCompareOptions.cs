using System;
using System.Collections.Generic;

namespace LatticeObjectTree
{
    /// <summary>
    /// Options that control how two object trees are compared.
    /// </summary>
    public interface IObjectTreeCompareOptions : IObjectTreeOptions
    {
        /// <summary>
        /// The maximum acceptable difference between two double values to consider them equal.
        /// </summary>
        double? DoubleComparisonDelta { get; }

        /// <summary>
        /// The maximum acceptable difference between two float values  to consider them equal.
        /// </summary>
        float? FloatComparisonDelta { get; }

        /// <summary>
        /// The maximum acceptable difference between two decimal values to consider them equal.
        /// </summary>
        decimal? DecimalComparisonDelta { get; }

        /// <summary>
        /// The maximum acceptable difference between two DateTime or DateTimeOffset values to consider them equal.
        /// </summary>
        TimeSpan? DateTimeComparisonDelta { get; }

        /// <summary>
        /// A custom comparer used to compare object tree node values.
        /// </summary>
        IEqualityComparer<object> ValueEqualityComparer { get; }

        /// <summary>
        /// A custom value formatter used to generate display values for differences.
        /// </summary>
        ICustomFormatter ValueFormatter { get; }
    }

    /// <summary>
    /// A default implementation of <see cref="IObjectTreeCompareOptions"/>.
    /// </summary>
    public class ObjectTreeCompareOptions : ObjectTreeOptions, IObjectTreeCompareOptions
    {
        /// <inheritdoc />
        public double? DoubleComparisonDelta { get; set; }

        /// <inheritdoc />
        public float? FloatComparisonDelta { get; set; }

        /// <inheritdoc />
        public decimal? DecimalComparisonDelta { get; set; }

        /// <inheritdoc />
        public TimeSpan? DateTimeComparisonDelta { get; set; }

        /// <inheritdoc />
        public IEqualityComparer<object> ValueEqualityComparer { get; set; }

        /// <inheritdoc />
        public ICustomFormatter ValueFormatter { get; set; }
    }
}

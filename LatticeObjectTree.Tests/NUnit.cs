using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.Framework
{
    /// <summary>
    /// Adapter assertion methods for <see cref="Xunit.Assert"/> that are compatible with NUnit.
    /// </summary>
    internal static class Assert
    {
        public static void AreEqual(object expected, object actual) => Xunit.Assert.Equal(expected, actual);
        public static void AreNotEqual(object expected, object actual) => Xunit.Assert.NotEqual(expected, actual);
        public static void AreSame(object expected, object actual) => Xunit.Assert.Same(expected, actual);
        public static void AreNotSame(object expected, object actual) => Xunit.Assert.NotSame(expected, actual);
        public static void IsTrue(bool? condition) => Xunit.Assert.True(condition);
        public static void IsFalse(bool? condition) => Xunit.Assert.False(condition);
        public static void IsNull(object anObject) => Xunit.Assert.Null(anObject);
        public static void IsNotNull(object anObject) => Xunit.Assert.NotNull(anObject);
        public static void IsInstanceOf<TExpected>(object actual)  => Xunit.Assert.IsAssignableFrom<TExpected>(actual);
        public static TActual Throws<TActual>(Action code) where TActual : Exception => Xunit.Assert.Throws<TActual>(code);
    }

    /// <summary>
    /// Adapter assertion methods for <see cref="Xunit.Assert"/> that are compatible with NUnit.
    /// </summary>
    internal static class StringAssert
    {
        public static void Contains(string expected, string actual) => Xunit.Assert.Contains(expected, actual);
    }
}
